using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Services;

/// <summary>
/// <para>Implementation of <see cref="IMonthlySummaryService"/> based on EF Core.</para>
/// </summary>
public sealed class MonthlySummaryService : IMonthlySummaryService
{
    private readonly FinancialPlannerDbContext _context;

    /// <summary>
    /// <para>Initializes the service.</para>
    /// </summary>
    public MonthlySummaryService(FinancialPlannerDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<MonthlySummaryDto>> GetSummariesAsync(int year, Guid userId, int? startDay = null, int? endDay = null, CancellationToken cancellationToken = default)
    {
        var yearStart = new DateOnly(year, 1, 1);
        var yearEnd = new DateOnly(year, 12, 31);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var useDayFilter = startDay.HasValue && endDay.HasValue;

        // Get all expenses for the year (actual and planned)
        var expenses = await _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.ExpenseDate >= yearStart && e.ExpenseDate <= yearEnd)
            .ToListAsync(cancellationToken);

        // Get all income records for the year (actual and planned)
        var incomeRecords = await _context.IncomeRecords
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.ReceivedDate >= yearStart && i.ReceivedDate <= yearEnd)
            .ToListAsync(cancellationToken);

        // Get income cycles for the year (similar to recurring expenses)
        var incomeCycles = await _context.IncomeCycles
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.StartDate <= yearEnd && (i.EndDate == null || i.EndDate >= yearStart))
            .ToListAsync(cancellationToken);

        // Get recurring expenses for the year
        var recurringExpenses = await _context.RecurringExpenses
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.IsActive && r.StartDate <= yearEnd && (r.EndDate == null || r.EndDate >= yearStart))
            .ToListAsync(cancellationToken);

        // Get credit payments for the year (from credit transactions payment schedule)
        var creditPayments = await _context.CreditPaymentSchedules
            .AsNoTracking()
            .Where(p => p.UserId == userId &&
                p.ScheduledYear >= yearStart.Year &&
                p.ScheduledYear <= yearEnd.Year &&
                ((p.ScheduledYear > yearStart.Year && p.ScheduledYear < yearEnd.Year) ||
                 (p.ScheduledYear == yearStart.Year && p.ScheduledMonth >= yearStart.Month) ||
                 (p.ScheduledYear == yearEnd.Year && p.ScheduledMonth <= yearEnd.Month)))
            .ToListAsync(cancellationToken);

        // Get loan accounts and calculate their monthly payments
        var loanAccounts = await _context.CreditAccounts
            .AsNoTracking()
            .Where(a => a.UserId == userId && 
                       a.AccountType == Domain.Entities.CreditAccountType.Loan &&
                       a.IsActive &&
                       a.MonthlyPayment.HasValue &&
                       a.MonthlyPayment.Value > 0 &&
                       a.PaymentStartDate.HasValue &&
                       a.TermMonths.HasValue &&
                       a.TermMonths.Value > 0)
            .ToListAsync(cancellationToken);

        // Фактические расходы: все записи с !IsPlanned за месяц, кроме оплаты кредитной картой.
        // Расход по карте создаёт транзакцию, которую пользователь погашает позже — в общий баланс и «факт расход» не включаем, иначе двойной учёт.
        var actualExpenseLookup = expenses
            .Where(e => !e.IsPlanned && e.CreditAccountId == null)
            .Where(e => !useDayFilter || (e.ExpenseDate.Day >= startDay!.Value && e.ExpenseDate.Day <= endDay!.Value))
            .GroupBy(e => (e.ExpenseDate.Year, e.ExpenseDate.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Все плановые расходы по месяцам (как в списке «Планируемые расходы»), не только с датой в будущем
        var plannedExpenseLookup = expenses
            .Where(e => e.IsPlanned)
            .Where(e => !useDayFilter || (e.ExpenseDate.Day >= startDay!.Value && e.ExpenseDate.Day <= endDay!.Value))
            .GroupBy(e => (e.ExpenseDate.Year, e.ExpenseDate.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Фактический доход: все записи с !IsPlanned за месяц (без фильтра по today),
        // чтобы сумма в сводке совпадала с отображаемыми в интерфейсе
        var actualIncomeLookup = incomeRecords
            .Where(i => !i.IsPlanned)
            .Where(i => !useDayFilter || (i.ReceivedDate.Day >= startDay!.Value && i.ReceivedDate.Day <= endDay!.Value))
            .GroupBy(i => (i.ReceivedDate.Year, i.ReceivedDate.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Плановый доход: будущие записи и плановые с уже наступившей датой, но без реализованных (FirstConfirmedDate = факт уже создан)
        var plannedIncomeLookup = incomeRecords
            .Where(i => !i.FirstConfirmedDate.HasValue && (i.ReceivedDate > today || (i.IsPlanned && i.ReceivedDate <= today)))
            .Where(i => !useDayFilter || (i.ReceivedDate.Day >= startDay!.Value && i.ReceivedDate.Day <= endDay!.Value))
            .GroupBy(i => (i.ReceivedDate.Year, i.ReceivedDate.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Слоты дохода из циклов, уже реализованные фактом (чтобы не дублировать в плане)
        var actualIncomeCycleSlots = incomeRecords
            .Where(i => !i.IsPlanned && i.IncomeCycleId.HasValue)
            .Select(i => (i.ReceivedDate, i.IncomeCycleId!.Value))
            .ToHashSet();

        // Calculate income cycles by month (similar to recurring expenses)
        // Separate actual and planned income cycles; в план не включаем слоты, по которым уже есть факт
        var actualIncomeCyclesByMonth = new Dictionary<(int Year, int Month), decimal>();
        var plannedIncomeCyclesByMonth = new Dictionary<(int Year, int Month), decimal>();
        foreach (var incomeCycle in incomeCycles)
        {
            var date = incomeCycle.StartDate;
            while (date <= yearEnd && (incomeCycle.EndDate == null || date <= incomeCycle.EndDate.Value))
            {
                if (date >= yearStart)
                {
                    if (!useDayFilter || (date.Day >= startDay!.Value && date.Day <= endDay!.Value))
                    {
                        var key = (date.Year, date.Month);
                        if (incomeCycle.IsPlanned)
                        {
                            if (!actualIncomeCycleSlots.Contains((date, incomeCycle.Id)))
                            {
                                if (!plannedIncomeCyclesByMonth.ContainsKey(key))
                                    plannedIncomeCyclesByMonth[key] = 0;
                                plannedIncomeCyclesByMonth[key] += incomeCycle.Amount;
                            }
                        }
                        else
                        {
                            if (!actualIncomeCyclesByMonth.ContainsKey(key))
                                actualIncomeCyclesByMonth[key] = 0;
                            actualIncomeCyclesByMonth[key] += incomeCycle.Amount;
                        }
                    }
                }

                date = incomeCycle.Frequency switch
                {
                    Domain.Enumerations.IncomeFrequency.Weekly => date.AddDays(7),
                    Domain.Enumerations.IncomeFrequency.BiWeekly => date.AddDays(14),
                    Domain.Enumerations.IncomeFrequency.Monthly => date.AddMonths(1),
                    Domain.Enumerations.IncomeFrequency.Quarterly => date.AddMonths(3),
                    Domain.Enumerations.IncomeFrequency.Yearly => date.AddYears(1),
                    _ => date.AddMonths(1)
                };
            }
        }

        // Merge actual income cycles into actual income lookup
        foreach (var kvp in actualIncomeCyclesByMonth)
        {
            if (actualIncomeLookup.ContainsKey(kvp.Key))
                actualIncomeLookup[kvp.Key] += kvp.Value;
            else
                actualIncomeLookup[kvp.Key] = kvp.Value;
        }

        // Merge planned income cycles into planned income lookup
        foreach (var kvp in plannedIncomeCyclesByMonth)
        {
            if (plannedIncomeLookup.ContainsKey(kvp.Key))
                plannedIncomeLookup[kvp.Key] += kvp.Value;
            else
                plannedIncomeLookup[kvp.Key] = kvp.Value;
        }

        // Слоты расходов, по которым уже есть факт (дата + категория + подкатегория), чтобы не дублировать план
        var actualExpenseSlots = expenses
            .Where(e => !e.IsPlanned && e.CreditAccountId == null)
            .Select(e => (e.ExpenseDate, e.CategoryId, e.SubcategoryId ?? Guid.Empty))
            .ToHashSet();

        // Calculate planned recurring expenses by month (исключаем старые авто-расходы «Платеж по» — дублируют creditPaymentsByMonth)
        // В план включаем только RecurringExpense.IsPlanned == true, и не включаем слоты, по которым уже есть фактический расход
        var recurringByMonth = new Dictionary<(int Year, int Month), decimal>();
        foreach (var recurring in recurringExpenses)
        {
            if (!recurring.IsPlanned)
                continue;
            if (recurring.Notes != null && (
                recurring.Notes.Contains("Автоматически создано для транзакции") ||
                recurring.Notes.Contains("Автоматически создан для кредита")))
                continue;
            var date = recurring.StartDate;
            var subId = recurring.SubcategoryId ?? Guid.Empty;
            while (date <= yearEnd && (recurring.EndDate == null || date <= recurring.EndDate.Value))
            {
                if (date >= yearStart)
                {
                    if (!useDayFilter || (date.Day >= startDay!.Value && date.Day <= endDay!.Value))
                    {
                        if (!actualExpenseSlots.Contains((date, recurring.CategoryId, subId)))
                        {
                            var key = (date.Year, date.Month);
                            if (!recurringByMonth.ContainsKey(key))
                                recurringByMonth[key] = 0;
                            recurringByMonth[key] += recurring.Amount;
                        }
                    }
                }

                date = recurring.Frequency switch
                {
                    Domain.Enumerations.IncomeFrequency.Weekly => date.AddDays(7),
                    Domain.Enumerations.IncomeFrequency.BiWeekly => date.AddDays(14),
                    Domain.Enumerations.IncomeFrequency.Monthly => date.AddMonths(1),
                    Domain.Enumerations.IncomeFrequency.Quarterly => date.AddMonths(3),
                    Domain.Enumerations.IncomeFrequency.Yearly => date.AddYears(1),
                    _ => date.AddMonths(1)
                };
            }
        }

        // Фактический расход в сводке = только записи из таблицы Expense (!IsPlanned), без развёрнутых повторяющихся,
        // чтобы сумма совпадала с карточкой «Траты» в разделе «Финансы» (там считается только список расходов за месяц).

        // Calculate credit payments by month (from credit transactions) — CreditPaymentSchedule has no ScheduledDay, include all when filtering
        var creditPaymentsByMonth = creditPayments
            .GroupBy(p => (p.ScheduledYear, p.ScheduledMonth))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.PaymentAmount));

        // Фактические платежи по кредитам/картам по месяцам — вычитаем из плана, чтобы не дублировать
        var actualCreditPaymentByMonth = expenses
            .Where(e => !e.IsPlanned && e.CreditPaymentScheduleId != null)
            .Where(e => !useDayFilter || (e.ExpenseDate.Day >= startDay!.Value && e.ExpenseDate.Day <= endDay!.Value))
            .GroupBy(e => (e.ExpenseDate.Year, e.ExpenseDate.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Платежи по займу (Loan): расход с CreditAccountId (тип Loan) без CreditPaymentScheduleId — подтверждённый платёж по кредиту
        // Расходы с кредитной картой (CreditCard) не включаем — это покупки по карте, не платежи по займу
        var loanAccountIds = loanAccounts.Select(a => a.Id).ToHashSet();
        var actualLoanPaymentByMonth = expenses
            .Where(e => !e.IsPlanned && e.CreditAccountId != null && e.CreditPaymentScheduleId == null && loanAccountIds.Contains(e.CreditAccountId.Value))
            .Where(e => !useDayFilter || (e.ExpenseDate.Day >= startDay!.Value && e.ExpenseDate.Day <= endDay!.Value))
            .GroupBy(e => (e.ExpenseDate.Year, e.ExpenseDate.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Calculate loan payments by month (from credit_accounts)
        var loanPaymentsByMonth = new Dictionary<(int Year, int Month), decimal>();
        foreach (var loan in loanAccounts)
        {
            if (!loan.PaymentStartDate.HasValue || !loan.MonthlyPayment.HasValue || !loan.TermMonths.HasValue)
                continue;

            var paymentDate = loan.PaymentStartDate.Value;
            var loanEndDate = paymentDate.AddMonths(loan.TermMonths.Value - 1);

            for (int i = 0; i < loan.TermMonths.Value; i++)
            {
                if (paymentDate >= yearStart && paymentDate <= yearEnd)
                {
                    if (!useDayFilter || (paymentDate.Day >= startDay!.Value && paymentDate.Day <= endDay!.Value))
                    {
                        var key = (paymentDate.Year, paymentDate.Month);
                        if (!loanPaymentsByMonth.ContainsKey(key))
                            loanPaymentsByMonth[key] = 0;
                        loanPaymentsByMonth[key] += loan.MonthlyPayment.Value;
                    }
                }
                paymentDate = paymentDate.AddMonths(1);
            }
        }

        // Get snapshots if they exist
        var snapshots = await _context.MonthlySnapshots
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.Plan != null && s.Plan.UserId == userId && s.Plan.PlanYear == year)
            .ToListAsync(cancellationToken);

        var snapshotLookup = snapshots
            .Where(s => s.Plan != null)
            .GroupBy(s => (s.Plan!.PlanYear, s.Plan.PlanMonth))
            .ToDictionary(g => g.Key, g => g.First());

        // Get previous year's December for January carry-over
        decimal previousClosingBalance = 0;
        decimal previousPlannedBalance = 0;
        decimal previousActualBalance = 0;
        // Для баланса «план»: перенос = баланс факт прошлого месяца, если факт расхода > 0, иначе баланс план прошлого месяца
        decimal prevCarryForPlannedBalance = 0;
        var minYear = DateTime.UtcNow.Year - 10;
        if (year > 1 && year > minYear)
        {
            try
            {
                var prevSummaries = await GetSummariesAsync(year - 1, userId, null, null, cancellationToken);
                var prevDec = prevSummaries.FirstOrDefault(s => s.Month == 12);
                if (prevDec != null)
                {
                    previousClosingBalance = prevDec.ClosingBalance;
                    previousPlannedBalance = prevDec.PlannedBalance;
                    previousActualBalance = prevDec.ActualBalance;
                    prevCarryForPlannedBalance = prevDec.ActualExpense > 0 ? prevDec.ActualBalance : prevDec.PlannedBalance;
                }
            }
            catch
            {
                // Ignore if previous year has no data
            }
        }

        // Generate summaries for all 12 months
        var result = new List<MonthlySummaryDto>();

        for (int month = 1; month <= 12; month++)
        {
            var key = (year, month);
            var monthStart = new DateOnly(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Fact expense: debit expenses + installments linked to credit schedule + standalone loan-account payments (debit/expense logged against Loan account).
            var actualExpenseBase = actualExpenseLookup.GetValueOrDefault(key, 0m);
            var actualCreditForMonth = actualCreditPaymentByMonth.GetValueOrDefault(key, 0m);
            var actualLoanForMonth = actualLoanPaymentByMonth.GetValueOrDefault(key, 0m);
            var actualExpense = actualExpenseBase + actualCreditForMonth + actualLoanForMonth;
            var actualIncome = actualIncomeLookup.GetValueOrDefault(key, 0m);

            // Check if snapshot exists (for historical data)
            if (snapshotLookup.TryGetValue(key, out var snapshot))
            {
                actualExpense = snapshot.ActualExpense;
                actualIncome = snapshot.ActualIncome;
            }

            // Плановый доход за месяц (уже без реализованных слотов)
            var plannedIncome = plannedIncomeLookup.GetValueOrDefault(key, 0m);

            // Planned expense: one-off planned + planned recurring + credit schedule (full month) + loan installments from account settings.
            // Remaining credit/loan = planned amount minus booked payments so plan vs actual does not double-count realized installments.
            // Half-month views: CreditPaymentSchedule has no calendar day → credit planned totals excluded; Loan uses payment calendar day → split by period.
            var plannedExpenseFromFuture = plannedExpenseLookup.GetValueOrDefault(key, 0m);
            var plannedRecurringForMonth = recurringByMonth.GetValueOrDefault(key, 0m);
            var creditPlanned = useDayFilter ? 0m : creditPaymentsByMonth.GetValueOrDefault(key, 0m);
            // Planned loan installments from Loan account settings (same source as Available funds / dashboard credit lines).
            var loanPlanned = loanPaymentsByMonth.GetValueOrDefault(key, 0m);
            var remainingCreditPlanned = Math.Max(0m, creditPlanned - actualCreditForMonth);
            var remainingLoanPlanned = Math.Max(0m, loanPlanned - actualLoanForMonth);
            var plannedExpensePending = plannedExpenseFromFuture + plannedRecurringForMonth + remainingCreditPlanned + remainingLoanPlanned;
            var fullPlannedExpenseForDisplay = plannedExpenseFromFuture + plannedRecurringForMonth + creditPlanned + loanPlanned;

            // Calculate carry over: опираемся на фактический баланс прошлого месяца
            var carryOver = month == 1 ? previousActualBalance : previousActualBalance;

            // Баланс: факт + оставшийся план (реализованное из плана уже в факте и из расчёта убрано)
            var incomeForBalance = actualIncome + plannedIncome;
            var expenseForBalance = actualExpense + plannedExpensePending;
            var closing = carryOver + incomeForBalance - expenseForBalance;

            // Actual balance: цепочка от фактического баланса прошлого месяца
            var prevActualCarry = month == 1 ? previousActualBalance : previousActualBalance;
            var actualBalance = prevActualCarry + actualIncome - actualExpense;

            // Planned balance: перенос = баланс факт прошлого месяца, если факт расхода > 0, иначе баланс план прошлого месяца
            var prevPlannedCarry = prevCarryForPlannedBalance;
            var plannedBalance = prevPlannedCarry + plannedIncome - plannedExpensePending;
            
            // Update color based on closing balance (including negative values)
            var color = closing < 0 ? "#DC2626" : closing < 2500m ? "#DC2626" : closing < 5000m ? "#F97316" : "#16A34A";

            result.Add(new MonthlySummaryDto
            {
                Year = year,
                Month = month,
                PlannedIncome = plannedIncome,
                PlannedExpense = plannedExpensePending,
                FullPlannedExpense = fullPlannedExpenseForDisplay,
                ActualIncome = actualIncome,
                ActualExpense = actualExpense,
                CarryOver = carryOver,
                ClosingBalance = closing,
                ActualBalance = actualBalance,
                PlannedBalance = plannedBalance,
                AlertColor = color
            });

            previousClosingBalance = closing;
            previousPlannedBalance = plannedBalance;
            previousActualBalance = actualBalance;
            // Для следующего месяца: если в этом месяце был факт расхода — переносим баланс факт, иначе баланс план
            prevCarryForPlannedBalance = actualExpense > 0 ? actualBalance : plannedBalance;
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<ForecastHorizonDto> GetForecastHorizonAsync(int startYear, int startMonth, int monthsCount, Guid userId, CancellationToken cancellationToken)
    {
        if (monthsCount < 3)
            monthsCount = 3; // Minimum 3 months as required

        var forecasts = new List<ForecastDto>();
        decimal currentCarryOver = 0;
        var problematicMonthsCount = 0;
        var totalFreeFunds = 0m;
        decimal? minFreeFunds = null;

        // Calculate period
        var endDate = new DateOnly(startYear, startMonth, 1).AddMonths(monthsCount - 1);
        var endYear = endDate.Year;
        var endMonth = endDate.Month;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Get actual and planned expenses and income for the period
        var startDate = new DateOnly(startYear, startMonth, 1);
        var expenses = await _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .ToListAsync(cancellationToken);

        var incomeRecords = await _context.IncomeRecords
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.ReceivedDate >= startDate && i.ReceivedDate <= endDate)
            .ToListAsync(cancellationToken);

        var incomeCycles = await _context.IncomeCycles
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.StartDate <= endDate && (i.EndDate == null || i.EndDate >= startDate))
            .ToListAsync(cancellationToken);

        // Separate actual and planned
        // Only non-planned expenses are considered actual; exclude credit-card expenses (transaction payment is counted separately)
        var actualExpenseLookup = expenses
            .Where(e => e.ExpenseDate <= today && !e.IsPlanned && e.CreditAccountId == null)
            .GroupBy(e => (e.ExpenseDate.Year, e.ExpenseDate.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Все плановые расходы по месяцам (как в списке «Планируемые расходы»)
        var plannedExpenseLookup = expenses
            .Where(e => e.IsPlanned)
            .GroupBy(e => (e.ExpenseDate.Year, e.ExpenseDate.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Calculate income cycles by month (similar to recurring expenses)
        // Separate actual and planned income cycles
        var actualIncomeCyclesByMonth = new Dictionary<(int Year, int Month), decimal>();
        var plannedIncomeCyclesByMonth = new Dictionary<(int Year, int Month), decimal>();
        foreach (var incomeCycle in incomeCycles)
        {
            var date = incomeCycle.StartDate;
            while (date <= endDate && (incomeCycle.EndDate == null || date <= incomeCycle.EndDate.Value))
            {
                if (date >= startDate)
                {
                    var key = (date.Year, date.Month);
                    if (incomeCycle.IsPlanned)
                    {
                        // Planned income cycles go to planned lookup
                        if (!plannedIncomeCyclesByMonth.ContainsKey(key))
                            plannedIncomeCyclesByMonth[key] = 0;
                        plannedIncomeCyclesByMonth[key] += incomeCycle.Amount;
                    }
                    else
                    {
                        // Actual income cycles go to actual lookup
                        if (!actualIncomeCyclesByMonth.ContainsKey(key))
                            actualIncomeCyclesByMonth[key] = 0;
                        actualIncomeCyclesByMonth[key] += incomeCycle.Amount;
                    }
                }

                date = incomeCycle.Frequency switch
                {
                    Domain.Enumerations.IncomeFrequency.Weekly => date.AddDays(7),
                    Domain.Enumerations.IncomeFrequency.BiWeekly => date.AddDays(14),
                    Domain.Enumerations.IncomeFrequency.Monthly => date.AddMonths(1),
                    Domain.Enumerations.IncomeFrequency.Quarterly => date.AddMonths(3),
                    Domain.Enumerations.IncomeFrequency.Yearly => date.AddYears(1),
                    _ => date.AddMonths(1)
                };
            }
        }

        var actualIncomeLookup = actualIncomeCyclesByMonth;

        // Add actual income records (only non-planned)
        var actualIncomeRecords = incomeRecords
            .Where(i => i.ReceivedDate <= today && !i.IsPlanned)
            .GroupBy(i => (i.ReceivedDate.Year, i.ReceivedDate.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        foreach (var kvp in actualIncomeRecords)
        {
            if (actualIncomeLookup.ContainsKey(kvp.Key))
                actualIncomeLookup[kvp.Key] += kvp.Value;
            else
                actualIncomeLookup[kvp.Key] = kvp.Value;
        }

        // Calculate planned income from income records with future dates
        var plannedIncomeLookup = incomeRecords
            .Where(i => i.ReceivedDate > today)
            .GroupBy(i => (i.ReceivedDate.Year, i.ReceivedDate.Month))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Get recurring expenses for the period
        var recurringExpenses = await _context.RecurringExpenses
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.IsActive && r.StartDate <= endDate && (r.EndDate == null || r.EndDate >= startDate))
            .ToListAsync(cancellationToken);

        // Get credit payments for the period (from credit transactions payment schedule)
        var creditPayments = await _context.CreditPaymentSchedules
            .AsNoTracking()
            .Where(p => p.UserId == userId &&
                p.ScheduledYear >= startDate.Year &&
                p.ScheduledYear <= endDate.Year &&
                ((p.ScheduledYear > startDate.Year && p.ScheduledYear < endDate.Year) ||
                 (p.ScheduledYear == startDate.Year && p.ScheduledMonth >= startDate.Month) ||
                 (p.ScheduledYear == endDate.Year && p.ScheduledMonth <= endDate.Month)))
            .ToListAsync(cancellationToken);

        // Get loan accounts and calculate their monthly payments
        var loanAccounts = await _context.CreditAccounts
            .AsNoTracking()
            .Where(a => a.UserId == userId && 
                       a.AccountType == Domain.Entities.CreditAccountType.Loan &&
                       a.IsActive &&
                       a.MonthlyPayment.HasValue &&
                       a.MonthlyPayment.Value > 0 &&
                       a.PaymentStartDate.HasValue &&
                       a.TermMonths.HasValue &&
                       a.TermMonths.Value > 0)
            .ToListAsync(cancellationToken);

        // Calculate recurring expenses by month (исключаем старые авто-расходы «Платеж по»)
        var recurringByMonth = new Dictionary<(int Year, int Month), decimal>();
        foreach (var recurring in recurringExpenses)
        {
            if (recurring.Notes != null && (
                recurring.Notes.Contains("Автоматически создано для транзакции") ||
                recurring.Notes.Contains("Автоматически создан для кредита")))
                continue;
            var date = recurring.StartDate;
            while (date <= endDate && (recurring.EndDate == null || date <= recurring.EndDate.Value))
            {
                if (date >= startDate)
                {
                    var key = (date.Year, date.Month);
                    if (!recurringByMonth.ContainsKey(key))
                        recurringByMonth[key] = 0;
                    recurringByMonth[key] += recurring.Amount;
                }

                date = recurring.Frequency switch
                {
                    Domain.Enumerations.IncomeFrequency.Weekly => date.AddDays(7),
                    Domain.Enumerations.IncomeFrequency.BiWeekly => date.AddDays(14),
                    Domain.Enumerations.IncomeFrequency.Monthly => date.AddMonths(1),
                    Domain.Enumerations.IncomeFrequency.Quarterly => date.AddMonths(3),
                    Domain.Enumerations.IncomeFrequency.Yearly => date.AddYears(1),
                    _ => date.AddMonths(1)
                };
            }
        }

        // Calculate credit payments by month (from credit transactions)
        var creditPaymentsByMonth = creditPayments
            .GroupBy(p => (p.ScheduledYear, p.ScheduledMonth))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.PaymentAmount));

        // Calculate loan payments by month (from credit_accounts)
        var loanPaymentsByMonth = new Dictionary<(int Year, int Month), decimal>();
        foreach (var loan in loanAccounts)
        {
            if (!loan.PaymentStartDate.HasValue || !loan.MonthlyPayment.HasValue || !loan.TermMonths.HasValue)
                continue;

            var paymentDate = loan.PaymentStartDate.Value;
            var loanEndDate = paymentDate.AddMonths(loan.TermMonths.Value - 1);

            for (int i = 0; i < loan.TermMonths.Value; i++)
            {
                if (paymentDate >= startDate && paymentDate <= endDate)
                {
                    var key = (paymentDate.Year, paymentDate.Month);
                    if (!loanPaymentsByMonth.ContainsKey(key))
                        loanPaymentsByMonth[key] = 0;
                    loanPaymentsByMonth[key] += loan.MonthlyPayment.Value;
                }
                paymentDate = paymentDate.AddMonths(1);
            }
        }

        // Get initial carry over from previous month summary
        if (currentCarryOver == 0)
        {
            var prevMonth = startDate.AddMonths(-1);
            var prevSummaries = await GetSummariesAsync(prevMonth.Year, userId, null, null, cancellationToken);
            var prevSummary = prevSummaries.FirstOrDefault(s => s.Year == prevMonth.Year && s.Month == prevMonth.Month);
            currentCarryOver = prevSummary?.ClosingBalance ?? 0m;
        }

        // Calculate forecast for each month
        var currentDate = new DateOnly(startYear, startMonth, 1);
        for (int i = 0; i < monthsCount; i++)
        {
            var year = currentDate.Year;
            var month = currentDate.Month;
            var key = (year, month);

            // Calculate planned income from income records with future dates
            var plannedIncome = plannedIncomeLookup.GetValueOrDefault(key, 0m);

            // Calculate planned expense from expenses with future dates, recurring expenses, credit payments, and loan payments
            var basePlannedExpense = plannedExpenseLookup.GetValueOrDefault(key, 0m);
            var recurringForMonth = recurringByMonth.GetValueOrDefault(key, 0m);
            var creditPaymentsForMonth = creditPaymentsByMonth.GetValueOrDefault(key, 0m);
            var loanPaymentsForMonth = loanPaymentsByMonth.GetValueOrDefault(key, 0m);
            var plannedExpense = basePlannedExpense + recurringForMonth + creditPaymentsForMonth + loanPaymentsForMonth;
            var carryOver = i == 0 ? currentCarryOver : forecasts.Last().ProjectedBalance;

            // Get actuals if available
            actualExpenseLookup.TryGetValue(key, out var actualExpense);
            actualIncomeLookup.TryGetValue(key, out var actualIncome);

            // Calculate projected balance
            var projectedBalance = carryOver + (actualIncome > 0 ? actualIncome : plannedIncome) - (actualExpense > 0 ? actualExpense : plannedExpense);
            var freeFunds = projectedBalance;

            // Determine if problematic
            var isProblematic = projectedBalance < 0 || plannedExpense > plannedIncome * 1.2m; // 20% over planned income
            var alertLevel = 0; // OK
            var alertColor = "#10B981"; // Green

            if (projectedBalance < 0)
            {
                alertLevel = 2; // Critical
                alertColor = "#DC2626"; // Red
                problematicMonthsCount++;
            }
            else if (projectedBalance < 2500m || plannedExpense > plannedIncome * 1.1m)
            {
                alertLevel = 1; // Warning
                alertColor = "#F97316"; // Orange
                problematicMonthsCount++;
            }

            if (minFreeFunds == null || freeFunds < minFreeFunds)
                minFreeFunds = freeFunds;

            totalFreeFunds += freeFunds;

            forecasts.Add(new ForecastDto
            {
                Year = year,
                Month = month,
                PlannedIncome = plannedIncome,
                PlannedExpense = plannedExpense,
                CarryOver = carryOver,
                ActualIncome = actualIncome,
                ActualExpense = actualExpense,
                ClosingBalance = projectedBalance,
                ProjectedBalance = projectedBalance,
                IsProblematic = isProblematic,
                AlertLevel = alertLevel,
                AlertColor = alertColor,
                FreeFunds = freeFunds
            });

            currentDate = currentDate.AddMonths(1);
        }

        return new ForecastHorizonDto
        {
            StartYear = startYear,
            StartMonth = startMonth,
            Forecasts = forecasts,
            ProblematicMonthsCount = problematicMonthsCount,
            MinimumFreeFunds = minFreeFunds ?? 0m,
            AverageFreeFunds = forecasts.Count > 0 ? totalFreeFunds / forecasts.Count : 0m
        };
    }

    /// <inheritdoc/>
    public async Task<CategoryStatisticsResponseDto> GetCategoryStatisticsAsync(DateOnly startDate, DateOnly endDate, Guid userId, CancellationToken cancellationToken)
    {
        // Get expenses for the period
        var expenses = await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Category)
            .Where(e => e.UserId == userId && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var totalExpenses = expenses.Sum(e => e.Amount);

        // Group expenses by category
        var categoryStats = expenses
            .GroupBy(e => new { e.CategoryId, e.Category!.Name, e.Category.HexColor })
            .Select(g => new
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                CategoryColor = g.Key.HexColor,
                TotalAmount = g.Sum(e => e.Amount),
                ExpenseCount = g.Count(),
                AverageAmount = g.Average(e => e.Amount)
            })
            .ToList();

        // Плановые суммы по категориям — все плановые расходы (как в списке)
        var plannedAmounts = expenses
            .Where(e => e.IsPlanned)
            .GroupBy(e => e.CategoryId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        var statistics = categoryStats.Select(stat =>
        {
            var plannedAmount = plannedAmounts.GetValueOrDefault(stat.CategoryId, 0m);
            var differenceFromPlan = stat.TotalAmount - plannedAmount;

            return new CategoryStatisticsDto
            {
                CategoryId = stat.CategoryId,
                CategoryName = stat.CategoryName,
                CategoryColor = stat.CategoryColor,
                TotalAmount = stat.TotalAmount,
                ExpenseCount = stat.ExpenseCount,
                AverageAmount = stat.AverageAmount,
                PercentageOfTotal = totalExpenses > 0 ? (stat.TotalAmount / totalExpenses) * 100 : 0,
                PlannedAmount = plannedAmount,
                DifferenceFromPlan = differenceFromPlan
            };
        })
        .OrderByDescending(s => s.TotalAmount)
        .ToList();

        return new CategoryStatisticsResponseDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalExpenses = totalExpenses,
            Categories = statistics
        };
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<AvailableFundsDto>> GetAvailableFundsForecastAsync(DateOnly startDate, DateOnly endDate, Guid userId, CancellationToken cancellationToken)
    {
        var result = new List<AvailableFundsDto>();
        var currentDate = startDate;
        decimal currentBalance = 0;

        // Get initial balance from previous month summary
        var previousMonth = startDate.AddMonths(-1);
        var previousSummaries = await GetSummariesAsync(previousMonth.Year, userId, null, null, cancellationToken);
        var previousSummary = previousSummaries.FirstOrDefault(s => s.Year == previousMonth.Year && s.Month == previousMonth.Month);
        if (previousSummary != null)
        {
            currentBalance = previousSummary.ClosingBalance;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var expenses = await _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .ToListAsync(cancellationToken);

        var incomeRecords = await _context.IncomeRecords
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.ReceivedDate >= startDate && i.ReceivedDate <= endDate)
            .ToListAsync(cancellationToken);

        // Get income cycles for the period
        var incomeCycles = await _context.IncomeCycles
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.StartDate <= endDate && (i.EndDate == null || i.EndDate >= startDate))
            .ToListAsync(cancellationToken);

        var recurringExpenses = await _context.RecurringExpenses
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.IsActive && r.StartDate <= endDate && (r.EndDate == null || r.EndDate >= startDate))
            .ToListAsync(cancellationToken);

        // Get credit payments for the period (from credit transactions payment schedule)
        var creditPayments = await _context.CreditPaymentSchedules
            .AsNoTracking()
            .Where(p => p.UserId == userId &&
                p.ScheduledYear >= startDate.Year &&
                p.ScheduledYear <= endDate.Year &&
                ((p.ScheduledYear > startDate.Year && p.ScheduledYear < endDate.Year) ||
                 (p.ScheduledYear == startDate.Year && p.ScheduledMonth >= startDate.Month) ||
                 (p.ScheduledYear == endDate.Year && p.ScheduledMonth <= endDate.Month)))
            .ToListAsync(cancellationToken);

        // Get loan accounts and calculate their monthly payments
        var loanAccounts = await _context.CreditAccounts
            .AsNoTracking()
            .Where(a => a.UserId == userId && 
                       a.AccountType == Domain.Entities.CreditAccountType.Loan &&
                       a.IsActive &&
                       a.MonthlyPayment.HasValue &&
                       a.MonthlyPayment.Value > 0 &&
                       a.PaymentStartDate.HasValue &&
                       a.TermMonths.HasValue &&
                       a.TermMonths.Value > 0)
            .ToListAsync(cancellationToken);

        // Group by date (separate actual and planned)
        // Only non-planned expenses are considered actual; exclude credit-card (payment via transaction)
        var actualExpenseByDate = expenses
            .Where(e => e.ExpenseDate <= today && !e.IsPlanned && e.CreditAccountId == null)
            .GroupBy(e => e.ExpenseDate)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Все плановые расходы по датам (как в списке «Планируемые расходы»)
        var plannedExpenseByDate = expenses
            .Where(e => e.IsPlanned)
            .GroupBy(e => e.ExpenseDate)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Only non-planned income records are considered actual
        var actualIncomeByDate = incomeRecords
            .Where(i => i.ReceivedDate <= today && !i.IsPlanned)
            .GroupBy(i => i.ReceivedDate)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        // Planned income includes both future income and planned income that has arrived
        var plannedIncomeByDate = incomeRecords
            .Where(i => i.ReceivedDate > today || (i.IsPlanned && i.ReceivedDate <= today))
            .GroupBy(i => i.ReceivedDate)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
        // Calculate credit payments by month (from credit transactions)
        var creditPaymentByMonth = creditPayments
            .GroupBy(p => new DateOnly(p.ScheduledYear, p.ScheduledMonth, 1))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.PaymentAmount));

        // Calculate loan payments by month (from credit_accounts)
        var loanPaymentByMonth = new Dictionary<DateOnly, decimal>();
        foreach (var loan in loanAccounts)
        {
            if (!loan.PaymentStartDate.HasValue || !loan.MonthlyPayment.HasValue || !loan.TermMonths.HasValue)
                continue;

            var paymentDate = loan.PaymentStartDate.Value;
            var loanEndDate = paymentDate.AddMonths(loan.TermMonths.Value - 1);

            for (int i = 0; i < loan.TermMonths.Value; i++)
            {
                var monthStart = new DateOnly(paymentDate.Year, paymentDate.Month, 1);
                if (monthStart >= startDate && monthStart <= endDate)
                {
                    if (!loanPaymentByMonth.ContainsKey(monthStart))
                        loanPaymentByMonth[monthStart] = 0;
                    loanPaymentByMonth[monthStart] += loan.MonthlyPayment.Value;
                }
                paymentDate = paymentDate.AddMonths(1);
            }
        }

        // Calculate income cycles for each date (separate actual and planned)
        var actualIncomeCyclesByDate = new Dictionary<DateOnly, decimal>();
        var plannedIncomeCyclesByDate = new Dictionary<DateOnly, decimal>();
        foreach (var incomeCycle in incomeCycles)
        {
            var date = incomeCycle.StartDate;
            while (date <= endDate && (incomeCycle.EndDate == null || date <= incomeCycle.EndDate.Value))
            {
                if (date >= startDate)
                {
                    if (incomeCycle.IsPlanned)
                    {
                        // Planned income cycles
                        if (!plannedIncomeCyclesByDate.ContainsKey(date))
                            plannedIncomeCyclesByDate[date] = 0;
                        plannedIncomeCyclesByDate[date] += incomeCycle.Amount;
                    }
                    else
                    {
                        // Actual income cycles
                        if (!actualIncomeCyclesByDate.ContainsKey(date))
                            actualIncomeCyclesByDate[date] = 0;
                        actualIncomeCyclesByDate[date] += incomeCycle.Amount;
                    }
                }

                date = incomeCycle.Frequency switch
                {
                    Domain.Enumerations.IncomeFrequency.Weekly => date.AddDays(7),
                    Domain.Enumerations.IncomeFrequency.BiWeekly => date.AddDays(14),
                    Domain.Enumerations.IncomeFrequency.Monthly => date.AddMonths(1),
                    Domain.Enumerations.IncomeFrequency.Quarterly => date.AddMonths(3),
                    Domain.Enumerations.IncomeFrequency.Yearly => date.AddYears(1),
                    _ => date.AddMonths(1)
                };
            }
        }

        // Merge actual income cycles into actualIncomeByDate
        foreach (var kvp in actualIncomeCyclesByDate)
        {
            if (actualIncomeByDate.ContainsKey(kvp.Key))
                actualIncomeByDate[kvp.Key] += kvp.Value;
            else
                actualIncomeByDate[kvp.Key] = kvp.Value;
        }

        // Merge planned income cycles into plannedIncomeByDate
        foreach (var kvp in plannedIncomeCyclesByDate)
        {
            if (plannedIncomeByDate.ContainsKey(kvp.Key))
                plannedIncomeByDate[kvp.Key] += kvp.Value;
            else
                plannedIncomeByDate[kvp.Key] = kvp.Value;
        }

        // Calculate recurring expenses for each date (исключаем старые авто-расходы «Платеж по»)
        var recurringByDate = new Dictionary<DateOnly, decimal>();
        foreach (var recurring in recurringExpenses)
        {
            if (recurring.Notes != null && (
                recurring.Notes.Contains("Автоматически создано для транзакции") ||
                recurring.Notes.Contains("Автоматически создан для кредита")))
                continue;
            var date = recurring.StartDate;
            while (date <= endDate && (recurring.EndDate == null || date <= recurring.EndDate.Value))
            {
                if (date >= startDate)
                {
                    if (!recurringByDate.ContainsKey(date))
                        recurringByDate[date] = 0;
                    recurringByDate[date] += recurring.Amount;
                }

                date = recurring.Frequency switch
                {
                    Domain.Enumerations.IncomeFrequency.Weekly => date.AddDays(7),
                    Domain.Enumerations.IncomeFrequency.BiWeekly => date.AddDays(14),
                    Domain.Enumerations.IncomeFrequency.Monthly => date.AddMonths(1),
                    Domain.Enumerations.IncomeFrequency.Quarterly => date.AddMonths(3),
                    Domain.Enumerations.IncomeFrequency.Yearly => date.AddYears(1),
                    _ => date.AddMonths(1)
                };
            }
        }

        // Calculate for each day
        while (currentDate <= endDate)
        {
            var monthStart = new DateOnly(currentDate.Year, currentDate.Month, 1);

            // Get actual expenses and income for the day
            var actualExpenses = actualExpenseByDate.GetValueOrDefault(currentDate, 0m);
            var actualIncome = actualIncomeByDate.GetValueOrDefault(currentDate, 0m);

            // Get planned expenses and income for the day (from future dates)
            var plannedExpensesForDay = plannedExpenseByDate.GetValueOrDefault(currentDate, 0m);
            var plannedIncomeForDay = plannedIncomeByDate.GetValueOrDefault(currentDate, 0m);

            var recurringExpensesToday = recurringByDate.GetValueOrDefault(currentDate, 0m);
            var creditPaymentsToday = currentDate.Day == 1 ? creditPaymentByMonth.GetValueOrDefault(monthStart, 0m) : 0m;
            var loanPaymentsToday = currentDate.Day == 1 ? loanPaymentByMonth.GetValueOrDefault(monthStart, 0m) : 0m;

            // Income for the day: use actual if available, otherwise planned
            var incomeToday = actualIncome > 0 ? actualIncome : plannedIncomeForDay;

            // Expenses for the day: actual + planned + recurring + credit payments + loan payments
            var expensesToday = actualExpenses + plannedExpensesForDay + recurringExpensesToday + creditPaymentsToday + loanPaymentsToday;

            currentBalance = currentBalance + incomeToday - expensesToday;

            result.Add(new AvailableFundsDto
            {
                Date = currentDate,
                AvailableAmount = currentBalance,
                PlannedIncome = incomeToday,
                PlannedExpenses = expensesToday,
                CreditPayments = creditPaymentsToday,
                RecurringExpenses = recurringExpensesToday
            });

            currentDate = currentDate.AddDays(1);
        }

        return result;
    }
}


using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Services;

/// <summary>
/// <para>EF Core implementation of <see cref="ICreditTransactionService"/>.</para>
/// </summary>
public sealed class CreditTransactionService : ICreditTransactionService
{
    private readonly FinancialPlannerDbContext _context;

    public CreditTransactionService(FinancialPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<CreditTransactionDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        var list = await _context.CreditTransactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Include(t => t.CreditAccount)
            .Include(t => t.Category)
            .Include(t => t.Subcategory)
            .Include(t => t.PaymentSchedule)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);

        return list.Select(t => new CreditTransactionDto
        {
            Id = t.Id,
            CreditAccountId = t.CreditAccountId,
            CreditAccountName = t.CreditAccount?.Name ?? string.Empty,
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name ?? string.Empty,
            SubcategoryId = t.SubcategoryId,
            SubcategoryName = t.Subcategory?.Name,
            TransactionDate = t.TransactionDate,
            Amount = t.Amount,
            Description = t.Description,
            IsIncomeRecorded = t.IsIncomeRecorded,
            PaymentSchedule = (t.PaymentSchedule ?? Array.Empty<CreditPaymentSchedule>()).Select(p => new CreditPaymentScheduleDto
            {
                Id = p.Id,
                ScheduledYear = p.ScheduledYear,
                ScheduledMonth = p.ScheduledMonth,
                PaymentAmount = p.PaymentAmount,
                IsPaid = p.IsPaid,
                PaidDate = p.PaidDate
            }).ToList()
        }).ToList();
    }

    public async Task<CreditTransactionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var t = await _context.CreditTransactions
            .AsNoTracking()
            .Include(t => t.CreditAccount)
            .Include(t => t.Category)
            .Include(t => t.Subcategory)
            .Include(t => t.PaymentSchedule)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (t == null)
            return null;

        return new CreditTransactionDto
        {
            Id = t.Id,
            CreditAccountId = t.CreditAccountId,
            CreditAccountName = t.CreditAccount?.Name ?? string.Empty,
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name ?? string.Empty,
            SubcategoryId = t.SubcategoryId,
            SubcategoryName = t.Subcategory?.Name,
            TransactionDate = t.TransactionDate,
            Amount = t.Amount,
            Description = t.Description,
            IsIncomeRecorded = t.IsIncomeRecorded,
            PaymentSchedule = (t.PaymentSchedule ?? Array.Empty<CreditPaymentSchedule>()).Select(p => new CreditPaymentScheduleDto
            {
                Id = p.Id,
                ScheduledYear = p.ScheduledYear,
                ScheduledMonth = p.ScheduledMonth,
                PaymentAmount = p.PaymentAmount,
                IsPaid = p.IsPaid,
                PaidDate = p.PaidDate
            }).ToList()
        };
    }

    public async Task<CreditTransactionDto> CreateAsync(CreateCreditTransactionRequest request, CancellationToken cancellationToken)
    {
        var account = await _context.CreditAccounts.FirstOrDefaultAsync(a => a.Id == request.CreditAccountId, cancellationToken);
        if (account is null)
        {
            throw new KeyNotFoundException("Credit account was not found.");
        }

        // Get or create default "Кредитная карта" category if categoryId is not provided.
        // Ищем по (UserId, Name) без фильтра по ParentId — в БД уникален (user_id, name), категория может существовать с любым parent_id.
        Guid categoryId = request.CategoryId ?? Guid.Empty;
        string categoryName = "Кредитная карта";
        if (categoryId == Guid.Empty)
        {
            var defaultCategory = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == account.UserId && c.Name == "Кредитная карта", cancellationToken);

            if (defaultCategory == null)
            {
                var newCategory = new Category
                {
                    Id = Guid.NewGuid(),
                    UserId = account.UserId,
                    Name = "Кредитная карта",
                    HexColor = "#FF6B6B",
                    Icon = "💳",
                    IsActive = true,
                    ParentId = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Categories.Add(newCategory);
                try
                {
                    await _context.SaveChangesAsync(cancellationToken);
                    defaultCategory = newCategory;
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message?.Contains("ux_categories_user_name") == true)
                {
                    _context.Entry(newCategory).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    defaultCategory = await _context.Categories
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.UserId == account.UserId && c.Name == "Кредитная карта", cancellationToken);
                    if (defaultCategory == null)
                        throw;
                }
            }
            categoryId = defaultCategory.Id;
            categoryName = defaultCategory.Name;
        }
        else
        {
            var cat = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
            if (cat != null)
                categoryName = cat.Name;
        }

        var months = Math.Min(request.PaymentMonths, 6);
        var paymentAmount = request.Amount / months;

        var transaction = new CreditTransaction
        {
            Id = Guid.NewGuid(),
            UserId = account.UserId,
            CreditAccountId = request.CreditAccountId,
            CategoryId = categoryId,
            SubcategoryId = request.SubcategoryId,
            TransactionDate = request.TransactionDate,
            Amount = request.Amount,
            Description = request.Description,
            IsIncomeRecorded = false
        };

        // Create payment schedule
        var schedule = new List<CreditPaymentSchedule>();
        var currentDate = new DateOnly(request.TransactionDate.Year, request.TransactionDate.Month, 1).AddMonths(1);
        
        for (int i = 0; i < months; i++)
        {
            var payment = new CreditPaymentSchedule
            {
                Id = Guid.NewGuid(),
                UserId = account.UserId,
                CreditTransactionId = transaction.Id,
                ScheduledYear = currentDate.Year,
                ScheduledMonth = currentDate.Month,
                PaymentAmount = i == months - 1 ? request.Amount - (paymentAmount * (months - 1)) : paymentAmount,
                IsPaid = false
            };
            schedule.Add(payment);
            currentDate = currentDate.AddMonths(1);
        }

        // Add payments to the collection instead of assigning
        foreach (var payment in schedule)
        {
            transaction.PaymentSchedule.Add(payment);
        }

        // Update account balance
        account.CurrentBalance += request.Amount;

        // Create income record if requested
        if (request.RecordAsIncome)
        {
            var incomeRecord = new IncomeRecord
            {
                Id = Guid.NewGuid(),
                UserId = account.UserId,
                Title = $"Снятие с {account.Name}",
                Amount = request.Amount,
                ReceivedDate = request.TransactionDate,
                IsFromCredit = true,
                CreditTransactionId = transaction.Id,
                Notes = request.Description
            };
            _context.IncomeRecords.Add(incomeRecord);
            transaction.IsIncomeRecorded = true;
        }

        _context.CreditTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        // Не создаём RecurringExpense для платежей по кредитной карте — они уже учитываются
        // в CreditPaymentSchedule и попадают в плановый расход в План/Факт (иначе было бы удвоение).

        var subcategory = transaction.SubcategoryId.HasValue
            ? await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == transaction.SubcategoryId.Value, cancellationToken)
            : null;

        return new CreditTransactionDto
        {
            Id = transaction.Id,
            CreditAccountId = transaction.CreditAccountId,
            CreditAccountName = account.Name,
            CategoryId = transaction.CategoryId,
            CategoryName = categoryName,
            SubcategoryId = transaction.SubcategoryId,
            SubcategoryName = subcategory?.Name,
            TransactionDate = transaction.TransactionDate,
            Amount = transaction.Amount,
            Description = transaction.Description,
            IsIncomeRecorded = transaction.IsIncomeRecorded,
            PaymentSchedule = schedule.Select(p => new CreditPaymentScheduleDto
            {
                Id = p.Id,
                ScheduledYear = p.ScheduledYear,
                ScheduledMonth = p.ScheduledMonth,
                PaymentAmount = p.PaymentAmount,
                IsPaid = p.IsPaid,
                PaidDate = p.PaidDate
            }).ToList()
        };
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _context.CreditTransactions
            .Include(t => t.CreditAccount)
            .Include(t => t.IncomeRecord)
            .Include(t => t.PaymentSchedule)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        
        if (transaction is null)
        {
            throw new KeyNotFoundException("Credit transaction was not found.");
        }

        // Update account balance
        if (transaction.CreditAccount != null)
        {
            transaction.CreditAccount.CurrentBalance -= transaction.Amount;
        }

        // Delete related income record
        if (transaction.IncomeRecord != null)
        {
            _context.IncomeRecords.Remove(transaction.IncomeRecord);
        }

        // Удаляем расходы, созданные при подтверждении платежей по этой транзакции
        var scheduleIds = transaction.PaymentSchedule.Select(p => p.Id).ToList();
        if (scheduleIds.Count > 0)
        {
            var expensesFromPayments = await _context.Expenses
                .Where(e => e.CreditPaymentScheduleId != null && scheduleIds.Contains(e.CreditPaymentScheduleId!.Value))
                .ToListAsync(cancellationToken);
            _context.Expenses.RemoveRange(expensesFromPayments);
        }

        _context.CreditTransactions.Remove(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordAsIncomeAsync(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _context.CreditTransactions
            .Include(t => t.CreditAccount)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        
        if (transaction is null)
        {
            throw new KeyNotFoundException("Credit transaction was not found.");
        }

        if (transaction.IsIncomeRecorded)
        {
            return; // Already recorded
        }

        var incomeRecord = new IncomeRecord
        {
            Id = Guid.NewGuid(),
            UserId = transaction.UserId,
            Title = $"Снятие с {transaction.CreditAccount?.Name ?? "кредитной карты"}",
            Amount = transaction.Amount,
            ReceivedDate = transaction.TransactionDate,
            IsFromCredit = true,
            CreditTransactionId = transaction.Id,
            Notes = transaction.Description
        };

        _context.IncomeRecords.Add(incomeRecord);
        transaction.IsIncomeRecorded = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PendingCreditPaymentDto>> GetPendingPaymentsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentYear = today.Year;
        var currentMonth = today.Month;

        var pendingPayments = await _context.CreditPaymentSchedules
            .AsNoTracking()
            .Include(p => p.CreditTransaction)
                .ThenInclude(t => t!.CreditAccount)
            .Include(p => p.CreditTransaction)
                .ThenInclude(t => t!.Category)
            .Include(p => p.CreditTransaction)
                .ThenInclude(t => t!.Subcategory)
            .Where(p => p.UserId == userId &&
                       !p.IsPaid &&
                       (p.ScheduledYear < currentYear || 
                        (p.ScheduledYear == currentYear && p.ScheduledMonth <= currentMonth)))
            .Select(p => new PendingCreditPaymentDto
            {
                PaymentScheduleId = p.Id,
                CreditTransactionId = p.CreditTransactionId,
                CreditAccountName = p.CreditTransaction!.CreditAccount!.Name,
                CreditAccountType = p.CreditTransaction!.CreditAccount!.AccountType.ToString(),
                ScheduledYear = p.ScheduledYear,
                ScheduledMonth = p.ScheduledMonth,
                ScheduledDay = p.CreditTransaction!.TransactionDate.Day,
                PaymentAmount = p.PaymentAmount,
                CategoryName = p.CreditTransaction!.Category!.Name,
                CategoryId = p.CreditTransaction!.CategoryId,
                SubcategoryId = p.CreditTransaction!.SubcategoryId,
                SubcategoryName = p.CreditTransaction!.Subcategory != null ? p.CreditTransaction!.Subcategory.Name : null
            })
            .ToListAsync(cancellationToken);

        return pendingPayments;
    }

    public async Task<IReadOnlyCollection<PendingCreditPaymentDto>> GetCreditPaymentsForMonthAsync(int year, int month, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.CreditPaymentSchedules
            .AsNoTracking()
            .Include(p => p.CreditTransaction)
                .ThenInclude(t => t!.CreditAccount)
            .Include(p => p.CreditTransaction)
                .ThenInclude(t => t!.Category)
            .Include(p => p.CreditTransaction)
                .ThenInclude(t => t!.Subcategory)
            .Where(p => p.UserId == userId &&
                       !p.IsPaid &&
                       p.ScheduledYear == year &&
                       p.ScheduledMonth == month)
            .Select(p => new PendingCreditPaymentDto
            {
                PaymentScheduleId = p.Id,
                CreditTransactionId = p.CreditTransactionId,
                CreditAccountName = p.CreditTransaction!.CreditAccount!.Name,
                CreditAccountType = p.CreditTransaction!.CreditAccount!.AccountType.ToString(),
                ScheduledYear = p.ScheduledYear,
                ScheduledMonth = p.ScheduledMonth,
                ScheduledDay = p.CreditTransaction!.TransactionDate.Day,
                PaymentAmount = p.PaymentAmount,
                CategoryName = p.CreditTransaction!.Category!.Name,
                CategoryId = p.CreditTransaction!.CategoryId,
                SubcategoryId = p.CreditTransaction!.SubcategoryId,
                SubcategoryName = p.CreditTransaction!.Subcategory != null ? p.CreditTransaction.Subcategory!.Name : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task ConfirmPaymentAsync(Guid paymentScheduleId, Guid userId, CancellationToken cancellationToken)
    {
        var payment = await _context.CreditPaymentSchedules
            .Include(p => p.CreditTransaction)
                .ThenInclude(t => t!.CreditAccount)
            .Include(p => p.CreditTransaction)
                .ThenInclude(t => t!.Category)
            .Include(p => p.CreditTransaction)
                .ThenInclude(t => t!.Subcategory)
            .FirstOrDefaultAsync(p => p.Id == paymentScheduleId && p.UserId == userId, cancellationToken);

        if (payment is null)
        {
            throw new KeyNotFoundException("Payment schedule was not found.");
        }

        if (payment.IsPaid)
        {
            return; // Already paid
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Mark payment as paid (в последующих месяцах этот платёж уже не будет отображаться как ожидающий)
        payment.IsPaid = true;
        payment.PaidDate = today;
        payment.UpdatedAt = DateTime.UtcNow;

        // Используем фактическую дату внесения платежа, чтобы расход попал в тот месяц, когда платёж реально внесён
        var expenseDate = today;

        var account = payment.CreditTransaction!.CreditAccount!;
        var paymentLabel = account.AccountType == Domain.Entities.CreditAccountType.Loan
            ? "Платеж по кредиту"
            : "Платеж по кредитной карте";

        // Create expense for the payment (связь с графиком — при удалении транзакции удалим и эти расходы)
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = payment.CreditTransaction!.CategoryId,
            SubcategoryId = payment.CreditTransaction!.SubcategoryId,
            ExpenseDate = expenseDate,
            Amount = payment.PaymentAmount,
            Description = $"{paymentLabel} {account.Name}",
            AccountId = null, // Credit payment doesn't affect regular account balance
            CreditPaymentScheduleId = payment.Id
        };

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);
    }
}


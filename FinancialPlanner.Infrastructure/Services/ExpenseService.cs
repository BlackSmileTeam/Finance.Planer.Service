using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Services;

/// <summary>
/// <para>EF Core implementation of <see cref="IExpenseService"/>.</para>
/// </summary>
public sealed class ExpenseService : IExpenseService
{
    private readonly FinancialPlannerDbContext _context;
    private readonly ICreditTransactionService _creditTransactionService;

    /// <summary>
    /// <para>Initializes the service.</para>
    /// </summary>
    public ExpenseService(FinancialPlannerDbContext context, ICreditTransactionService creditTransactionService)
    {
        _context = context;
        _creditTransactionService = creditTransactionService;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<ExpenseDto>> GetByMonthAsync(int year, int month, Guid userId, CancellationToken cancellationToken)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        return await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Account)
            .Include(e => e.CreditPaymentSchedule)
                .ThenInclude(s => s!.CreditTransaction)
            .Where(e => e.UserId == userId && e.ExpenseDate >= start && e.ExpenseDate <= end)
            .OrderBy(e => e.ExpenseDate)
            .ThenBy(e => e.Category!.Name)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : string.Empty,
                SubcategoryId = e.SubcategoryId,
                SubcategoryName = e.Subcategory != null ? e.Subcategory.Name : null,
                ExpenseDate = e.ExpenseDate,
                Amount = e.Amount,
                Description = e.Description,
                PlannedBudgetId = e.PlannedBudgetId,
                AccountId = e.AccountId,
                IsPlanned = e.IsPlanned,
                CreditPaymentScheduleId = e.CreditPaymentScheduleId,
                CreditAccountId = e.CreditAccountId ?? (e.CreditPaymentSchedule != null && e.CreditPaymentSchedule.CreditTransaction != null
                    ? e.CreditPaymentSchedule.CreditTransaction.CreditAccountId
                    : (Guid?)null),
                Currency = e.Account != null ? e.Account.Currency : "RUB"
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ExpenseDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Account)
            .Include(e => e.CreditPaymentSchedule)
                .ThenInclude(s => s!.CreditTransaction)
            .Where(e => e.Id == id && e.UserId == userId)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : string.Empty,
                SubcategoryId = e.SubcategoryId,
                SubcategoryName = e.Subcategory != null ? e.Subcategory.Name : null,
                ExpenseDate = e.ExpenseDate,
                Amount = e.Amount,
                Description = e.Description,
                PlannedBudgetId = e.PlannedBudgetId,
                AccountId = e.AccountId,
                IsPlanned = e.IsPlanned,
                CreditPaymentScheduleId = e.CreditPaymentScheduleId,
                CreditAccountId = e.CreditAccountId ?? (e.CreditPaymentSchedule != null && e.CreditPaymentSchedule.CreditTransaction != null
                    ? e.CreditPaymentSchedule.CreditTransaction.CreditAccountId
                    : (Guid?)null),
                Currency = e.Account != null ? e.Account.Currency : "RUB"
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new KeyNotFoundException("Category was not found.");
        }

        if (request.CreditAccountId.HasValue)
        {
            var creditAccountExists = await _context.CreditAccounts.AnyAsync(
                a => a.Id == request.CreditAccountId.Value && a.UserId == userId,
                cancellationToken);
            if (!creditAccountExists)
            {
                throw new KeyNotFoundException("Credit account was not found.");
            }
        }

        // Если добавляем фактический расход, а в этот день по этой категории уже есть плановый,
        // считаем, что план «исполнен» и плановые записи можно убрать, чтобы не было «остатка».
        if (!request.IsPlanned)
        {
            var plannedForSameSlot = await _context.Expenses
                .Where(e =>
                    e.UserId == userId &&
                    e.IsPlanned &&
                    e.ExpenseDate == request.ExpenseDate &&
                    e.CategoryId == request.CategoryId &&
                    e.SubcategoryId == request.SubcategoryId)
                .ToListAsync(cancellationToken);

            if (plannedForSameSlot.Count > 0)
            {
                _context.Expenses.RemoveRange(plannedForSameSlot);
            }
        }

        // Update account balance: when expense is not planned and money leaves the account.
        // Банк-Кредит: при погашении кредита (CreditAccountId = Loan) списываем с AccountId.
        // Кредитная карта: создаём транзакцию по карте, с банка не списываем.
        CreditAccount? creditAccountEntity = null;
        if (request.CreditAccountId.HasValue)
        {
            creditAccountEntity = await _context.CreditAccounts.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.CreditAccountId.Value && a.UserId == userId, cancellationToken);
        }
        var isLoanPaymentFromBank = creditAccountEntity?.AccountType == CreditAccountType.Loan && request.AccountId.HasValue;

        if (request.AccountId.HasValue && !request.IsPlanned && (!request.CreditAccountId.HasValue || isLoanPaymentFromBank))
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId.Value && a.UserId == userId, cancellationToken);
            if (account == null)
            {
                throw new KeyNotFoundException("Account was not found.");
            }
            account.Balance -= request.Amount;
            account.UpdatedAt = DateTime.UtcNow;
        }

        var entity = new Expense
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccountId = request.CreditAccountId.HasValue && !isLoanPaymentFromBank ? null : request.AccountId,
            CreditAccountId = request.CreditAccountId,
            CategoryId = request.CategoryId,
            SubcategoryId = request.SubcategoryId,
            ExpenseDate = request.ExpenseDate,
            Amount = request.Amount,
            Description = request.Description,
            PlannedBudgetId = request.PlannedBudgetId,
            IsPlanned = request.IsPlanned
        };

        _context.Expenses.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // When expense is paid by credit card, create the credit transaction (do not for Loan — Банк-Кредит is just an expense)
        if (creditAccountEntity?.AccountType == CreditAccountType.CreditCard)
        {
            var creditRequest = new CreateCreditTransactionRequest
            {
                CreditAccountId = request.CreditAccountId.Value,
                CategoryId = request.CategoryId,
                SubcategoryId = request.SubcategoryId,
                TransactionDate = request.ExpenseDate,
                Amount = request.Amount,
                Description = request.Description,
                RecordAsIncome = false,
                PaymentMonths = request.PaymentMonths ?? 6
            };
            await _creditTransactionService.CreateAsync(creditRequest, cancellationToken);
        }

        var category = await _context.Categories.AsNoTracking().FirstAsync(c => c.Id == entity.CategoryId, cancellationToken);
        var subcategory = entity.SubcategoryId.HasValue 
            ? await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == entity.SubcategoryId.Value, cancellationToken)
            : null;

        return new ExpenseDto
        {
            Id = entity.Id,
            CategoryId = entity.CategoryId,
            CategoryName = category.Name,
            SubcategoryId = entity.SubcategoryId,
            SubcategoryName = subcategory?.Name,
            ExpenseDate = entity.ExpenseDate,
            Amount = entity.Amount,
            Description = entity.Description,
            PlannedBudgetId = entity.PlannedBudgetId,
            AccountId = entity.AccountId,
            IsPlanned = entity.IsPlanned
        };
    }

    /// <inheritdoc/>
    public async Task<ExpenseDto> UpdateAsync(Guid id, UpdateExpenseRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await _context.Expenses.Include(e => e.Category).FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Expense was not found.");
        }

        // Handle account balance changes only if not planned
        var oldAmount = entity.Amount;
        var newAmount = request.Amount;
        var wasPlanned = entity.IsPlanned;
        var willBePlanned = request.IsPlanned;

        // Revert old account balance if account was set and expense was not planned
        if (entity.AccountId.HasValue && !wasPlanned)
        {
            var oldAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == entity.AccountId.Value, cancellationToken);
            if (oldAccount != null)
            {
                oldAccount.Balance += oldAmount; // Revert the expense
                oldAccount.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Update new account balance only if bank account is specified and expense will not be planned (not credit card)
        if (request.AccountId.HasValue && !willBePlanned && !request.CreditAccountId.HasValue)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId.Value && a.UserId == userId, cancellationToken);
            if (account == null)
            {
                throw new KeyNotFoundException("Account was not found.");
            }
            account.Balance -= newAmount;
            account.UpdatedAt = DateTime.UtcNow;
        }

        entity.CategoryId = request.CategoryId;
        entity.SubcategoryId = request.SubcategoryId;
        entity.ExpenseDate = request.ExpenseDate;
        entity.Amount = request.Amount;
        entity.Description = request.Description;
        entity.PlannedBudgetId = request.PlannedBudgetId;
        entity.AccountId = request.CreditAccountId.HasValue ? null : request.AccountId;
        entity.CreditAccountId = request.CreditAccountId;
        entity.IsPlanned = request.IsPlanned;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var category = entity.Category ?? await _context.Categories.AsNoTracking().FirstAsync(c => c.Id == entity.CategoryId, cancellationToken);
        var subcategory = entity.SubcategoryId.HasValue
            ? await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == entity.SubcategoryId.Value, cancellationToken)
            : null;

        return new ExpenseDto
        {
            Id = entity.Id,
            CategoryId = entity.CategoryId,
            CategoryName = category.Name,
            SubcategoryId = entity.SubcategoryId,
            SubcategoryName = subcategory?.Name,
            ExpenseDate = entity.ExpenseDate,
            Amount = entity.Amount,
            Description = entity.Description,
            PlannedBudgetId = entity.PlannedBudgetId,
            AccountId = entity.AccountId,
            CreditAccountId = entity.CreditAccountId,
            IsPlanned = entity.IsPlanned,
            Currency = request.Currency ?? "RUB"
        };
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Expense was not found.");
        }

        // Revert account balance if account was set
        if (entity.AccountId.HasValue)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == entity.AccountId.Value, cancellationToken);
            if (account != null)
            {
                account.Balance += entity.Amount; // Revert the expense
                account.UpdatedAt = DateTime.UtcNow;
            }
        }

        _context.Expenses.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<PendingPlannedTransactionDto>> GetPendingPlannedExpensesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Фактические расходы по (дата, категория, подкатегория) — считаем, что планируемый расход уже «закрыт»
        var actualKeys = await _context.Expenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && !e.IsPlanned && e.ExpenseDate <= today)
            .Select(e => new { e.ExpenseDate, e.CategoryId, e.SubcategoryId })
            .Distinct()
            .ToListAsync(cancellationToken);

        var actualSet = new HashSet<(DateOnly, Guid, Guid?)>(
            actualKeys.Select(k => (k.ExpenseDate, k.CategoryId, k.SubcategoryId)));

        var pendingExpenses = await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Subcategory)
            .Where(e => e.UserId == userId &&
                       e.IsPlanned &&
                       e.ExpenseDate <= today)
            .ToListAsync(cancellationToken);

        var filtered = pendingExpenses
            .Where(e => !actualSet.Contains((e.ExpenseDate, e.CategoryId, e.SubcategoryId)))
            .Select(e => new PendingPlannedTransactionDto
            {
                Id = e.Id,
                Type = "Expense",
                Title = e.Description ?? e.Category!.Name,
                Amount = e.Amount,
                Date = e.ExpenseDate,
                CategoryName = e.Category!.Name,
                CategoryId = e.CategoryId,
                SubcategoryId = e.SubcategoryId,
                SubcategoryName = e.Subcategory != null ? e.Subcategory.Name : null,
                Description = e.Description,
                AccountId = e.AccountId
            })
            .ToList();

        return filtered;
    }

    /// <inheritdoc/>
    public async Task ConfirmPlannedExpenseAsync(Guid expenseId, Guid userId, decimal? actualAmount, CancellationToken cancellationToken)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId, cancellationToken);

        if (expense is null)
        {
            throw new KeyNotFoundException("Expense was not found.");
        }

        if (!expense.IsPlanned)
        {
            return; // Already confirmed
        }

        // Use actual amount if provided, otherwise use planned amount
        var finalAmount = actualAmount ?? expense.Amount;

        // Превращаем плановый расход в фактический: обновляем запись, не создаём дубликат
        expense.Amount = finalAmount;
        expense.IsPlanned = false;
        expense.UpdatedAt = DateTime.UtcNow;

        // Update account balance if account is specified
        if (expense.AccountId.HasValue)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == expense.AccountId.Value && a.UserId == userId, cancellationToken);
            if (account != null)
            {
                account.Balance -= finalAmount;
                account.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Confirms a planned recurring expense by creating an actual expense.
    /// </summary>
    public async Task ConfirmPlannedRecurringExpenseAsync(Guid recurringExpenseId, DateOnly expenseDate, Guid userId, decimal? actualAmount, CancellationToken cancellationToken)
    {
        var recurringExpense = await _context.RecurringExpenses
            .FirstOrDefaultAsync(re => re.Id == recurringExpenseId && re.UserId == userId, cancellationToken);

        if (recurringExpense is null)
        {
            throw new KeyNotFoundException("Recurring expense was not found.");
        }

        if (!recurringExpense.IsPlanned)
        {
            return; // Already confirmed or not planned
        }

        // Use actual amount if provided, otherwise use planned amount
        var finalAmount = actualAmount ?? recurringExpense.Amount;

        // Check if expense already exists for this date (same category, subcategory, amount)
        var existingExpense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.UserId == userId &&
                                     e.ExpenseDate == expenseDate &&
                                     e.CategoryId == recurringExpense.CategoryId &&
                                     e.SubcategoryId == recurringExpense.SubcategoryId &&
                                     e.Amount == finalAmount &&
                                     !e.IsPlanned, cancellationToken);

        if (existingExpense != null)
        {
            return; // Already confirmed
        }

        // Create a new actual expense based on the recurring expense
        var actualExpense = new Expense
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = recurringExpense.CategoryId,
            SubcategoryId = recurringExpense.SubcategoryId,
            ExpenseDate = expenseDate,
            Amount = finalAmount,
            Description = recurringExpense.Title,
            PlannedBudgetId = null,
            AccountId = null, // Recurring expenses don't have accountId
            IsPlanned = false, // This is an actual expense
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Expenses.Add(actualExpense);

        // Keep the planned RecurringExpense as is (IsPlanned = true) for plan vs actual reporting
        // Don't modify the original planned RecurringExpense

        await _context.SaveChangesAsync(cancellationToken);
    }
}


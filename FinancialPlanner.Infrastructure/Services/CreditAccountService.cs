using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enumerations;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Services;

/// <summary>
/// <para>EF Core implementation of <see cref="ICreditAccountService"/>.</para>
/// </summary>
public sealed class CreditAccountService : ICreditAccountService
{
    private readonly FinancialPlannerDbContext _context;

    public CreditAccountService(FinancialPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<CreditAccountDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.CreditAccounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .Select(a => new CreditAccountDto
            {
                Id = a.Id,
                Name = a.Name,
                AccountType = a.AccountType.ToString(),
                CreditLimit = a.CreditLimit,
                MonthlyPayment = a.MonthlyPayment,
                TotalAmount = a.TotalAmount,
                TermMonths = a.TermMonths,
                PaymentStartDate = a.PaymentStartDate,
                CurrentBalance = a.CurrentBalance,
                InterestRate = a.InterestRate,
                IsActive = a.IsActive,
                Notes = a.Notes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CreditAccountDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.CreditAccounts
            .AsNoTracking()
            .Where(a => a.Id == id && a.UserId == userId)
            .Select(a => new CreditAccountDto
            {
                Id = a.Id,
                Name = a.Name,
                AccountType = a.AccountType.ToString(),
                CreditLimit = a.CreditLimit,
                MonthlyPayment = a.MonthlyPayment,
                TotalAmount = a.TotalAmount,
                TermMonths = a.TermMonths,
                PaymentStartDate = a.PaymentStartDate,
                CurrentBalance = a.CurrentBalance,
                InterestRate = a.InterestRate,
                IsActive = a.IsActive,
                Notes = a.Notes
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CreditAccountDto> CreateAsync(CreateCreditAccountRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CreditAccountType>(request.AccountType, out var accountType))
        {
            accountType = CreditAccountType.CreditCard;
        }

        var entity = new CreditAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            AccountType = accountType,
            CreditLimit = request.CreditLimit,
            MonthlyPayment = request.MonthlyPayment,
            TotalAmount = request.TotalAmount,
            TermMonths = request.TermMonths,
            PaymentStartDate = request.PaymentStartDate,
            CurrentBalance = request.CurrentBalance ?? 0,
            InterestRate = request.InterestRate,
            IsActive = true,
            Notes = request.Notes
        };

        _context.CreditAccounts.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Не создаём RecurringExpense для кредита — ежемесячный платёж уже учитывается
        // в MonthlySummaryService через loanPaymentsByMonth (иначе в План/Факт было бы удвоение).

        return new CreditAccountDto
        {
            Id = entity.Id,
            Name = entity.Name,
            AccountType = entity.AccountType.ToString(),
            CreditLimit = entity.CreditLimit,
            MonthlyPayment = entity.MonthlyPayment,
            TotalAmount = entity.TotalAmount,
            TermMonths = entity.TermMonths,
            PaymentStartDate = entity.PaymentStartDate,
            CurrentBalance = entity.CurrentBalance,
            InterestRate = entity.InterestRate,
            IsActive = entity.IsActive,
            Notes = entity.Notes
        };
    }

    public async Task<CreditAccountDto> UpdateAsync(Guid id, UpdateCreditAccountRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await _context.CreditAccounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Credit account was not found.");
        }

        entity.Name = request.Name;
        entity.CreditLimit = request.CreditLimit;
        entity.MonthlyPayment = request.MonthlyPayment;
        entity.TotalAmount = request.TotalAmount;
        entity.TermMonths = request.TermMonths;
        entity.PaymentStartDate = request.PaymentStartDate;
        entity.CurrentBalance = request.CurrentBalance;
        entity.InterestRate = request.InterestRate;
        entity.IsActive = request.IsActive;
        entity.Notes = request.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new CreditAccountDto
        {
            Id = entity.Id,
            Name = entity.Name,
            AccountType = entity.AccountType.ToString(),
            CreditLimit = entity.CreditLimit,
            MonthlyPayment = entity.MonthlyPayment,
            TotalAmount = entity.TotalAmount,
            TermMonths = entity.TermMonths,
            PaymentStartDate = entity.PaymentStartDate,
            CurrentBalance = entity.CurrentBalance,
            InterestRate = entity.InterestRate,
            IsActive = entity.IsActive,
            Notes = entity.Notes
        };
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await _context.CreditAccounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Credit account was not found.");
        }

        _context.CreditAccounts.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LoanPaymentForMonthDto>> GetLoanPaymentsForMonthAsync(int year, int month, Guid userId, CancellationToken cancellationToken)
    {
        var loanAccounts = await _context.CreditAccounts
            .AsNoTracking()
            .Where(a => a.UserId == userId &&
                        a.AccountType == CreditAccountType.Loan &&
                        a.IsActive &&
                        a.MonthlyPayment.HasValue &&
                        a.MonthlyPayment.Value > 0 &&
                        a.PaymentStartDate.HasValue &&
                        a.TermMonths.HasValue &&
                        a.TermMonths.Value > 0)
            .ToListAsync(cancellationToken);

        var result = new List<LoanPaymentForMonthDto>();
        var targetDate = new DateOnly(year, month, 1);

        foreach (var loan in loanAccounts)
        {
            if (!loan.PaymentStartDate.HasValue || !loan.MonthlyPayment.HasValue || !loan.TermMonths.HasValue)
                continue;

            var paymentDate = loan.PaymentStartDate.Value;
            var endDate = paymentDate.AddMonths(loan.TermMonths.Value - 1);

            for (int i = 0; i < loan.TermMonths.Value; i++)
            {
                if (paymentDate.Year == year && paymentDate.Month == month)
                {
                    result.Add(new LoanPaymentForMonthDto
                    {
                        CreditAccountId = loan.Id,
                        CreditAccountName = loan.Name,
                        ScheduledYear = year,
                        ScheduledMonth = month,
                        ScheduledDay = paymentDate.Day,
                        PaymentAmount = loan.MonthlyPayment!.Value
                    });
                    break;
                }
                paymentDate = paymentDate.AddMonths(1);
                if (paymentDate > endDate)
                    break;
            }
        }

        return result;
    }

    public async Task ConfirmLoanPaymentAsync(Guid creditAccountId, int year, int month, int day, decimal? amount, Guid userId, CancellationToken cancellationToken)
    {
        var loan = await _context.CreditAccounts
            .FirstOrDefaultAsync(a => a.Id == creditAccountId && a.UserId == userId && a.AccountType == CreditAccountType.Loan, cancellationToken);
        if (loan is null)
            throw new KeyNotFoundException("Loan account was not found.");

        if (!loan.MonthlyPayment.HasValue || loan.MonthlyPayment.Value <= 0)
            throw new InvalidOperationException("Loan has no monthly payment configured.");

        var paymentAmount = amount ?? loan.MonthlyPayment.Value;
        var expenseDate = new DateOnly(year, month, Math.Min(day, DateTime.DaysInMonth(year, month)));

        // Используем существующую категорию «Кредиты» или «Кредитная карта» (любая по имени, без требования ParentId)
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Кредиты", cancellationToken)
            ?? await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == "Кредитная карта", cancellationToken);
        if (category is null)
        {
            category = new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Кредиты",
                HexColor = "#FF6B6B",
                Icon = "💳",
                IsActive = true,
                ParentId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = category.Id,
            SubcategoryId = null,
            ExpenseDate = expenseDate,
            Amount = paymentAmount,
            Description = $"Платеж по кредиту {loan.Name}",
            AccountId = null,
            IsPlanned = false,
            CreditPaymentScheduleId = null,
            CreditAccountId = loan.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);
    }
}


using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enumerations;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Services;

/// <summary>
/// <para>
/// Service that wipes actual income/expense data for a user while preserving
/// planned and recurring items, adjusting their dates to start from today.
/// </para>
/// </summary>
public sealed class DataResetService : IDataResetService
{
    private readonly FinancialPlannerDbContext _context;

    public DataResetService(FinancialPlannerDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task ResetActualDataAsync(Guid userId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 1. Remove all actual (non-planned) expenses.
        var actualExpenses = await _context.Expenses
            .Where(e => e.UserId == userId && !e.IsPlanned)
            .ToListAsync(cancellationToken);
        if (actualExpenses.Count > 0)
        {
            _context.Expenses.RemoveRange(actualExpenses);
        }

        // 2. Remove all actual (non-planned) income records.
        var actualIncomeRecords = await _context.IncomeRecords
            .Where(i => i.UserId == userId && !i.IsPlanned)
            .ToListAsync(cancellationToken);
        if (actualIncomeRecords.Count > 0)
        {
            _context.IncomeRecords.RemoveRange(actualIncomeRecords);
        }

        // 3. Remove all non-planned income cycles (they represent actual recurring income).
        var actualIncomeCycles = await _context.IncomeCycles
            .Where(c => c.UserId == userId && !c.IsPlanned)
            .ToListAsync(cancellationToken);
        if (actualIncomeCycles.Count > 0)
        {
            _context.IncomeCycles.RemoveRange(actualIncomeCycles);
        }

        // 4. Shift one-off planned expenses so that their date is not earlier than today.
        var plannedExpenses = await _context.Expenses
            .Where(e => e.UserId == userId && e.IsPlanned && e.ExpenseDate < today)
            .ToListAsync(cancellationToken);
        foreach (var expense in plannedExpenses)
        {
            expense.ExpenseDate = today;
            expense.UpdatedAt = DateTime.UtcNow;
        }

        // 5. Shift one-off planned income records so that their date is not earlier than today.
        var plannedIncomeRecords = await _context.IncomeRecords
            .Where(i => i.UserId == userId && i.IsPlanned && i.ReceivedDate < today)
            .ToListAsync(cancellationToken);
        foreach (var income in plannedIncomeRecords)
        {
            income.ReceivedDate = today;
            income.FirstConfirmedDate = null;
            income.UpdatedAt = DateTime.UtcNow;
        }

        // 6. Shift planned recurring expenses to the next occurrence on or after today.
        var plannedRecurringExpenses = await _context.RecurringExpenses
            .Where(r => r.UserId == userId && r.IsPlanned && r.StartDate < today)
            .ToListAsync(cancellationToken);
        foreach (var recurring in plannedRecurringExpenses)
        {
            recurring.StartDate = GetNextOccurrenceOnOrAfter(recurring.StartDate, today, recurring.Frequency);
            recurring.UpdatedAt = DateTime.UtcNow;
        }

        // 7. Shift planned income cycles to the next occurrence on or after today.
        var plannedIncomeCycles = await _context.IncomeCycles
            .Where(c => c.UserId == userId && c.IsPlanned && c.StartDate < today)
            .ToListAsync(cancellationToken);
        foreach (var cycle in plannedIncomeCycles)
        {
            cycle.StartDate = GetNextOccurrenceOnOrAfter(cycle.StartDate, today, cycle.Frequency);
            cycle.FirstConfirmedDate = null;
            cycle.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static DateOnly GetNextOccurrenceOnOrAfter(DateOnly startDate, DateOnly today, IncomeFrequency frequency)
    {
        if (startDate >= today)
        {
            return startDate;
        }

        var current = startDate;

        switch (frequency)
        {
            case IncomeFrequency.Weekly:
                while (current < today)
                {
                    current = current.AddDays(7);
                }
                break;
            case IncomeFrequency.BiWeekly:
                while (current < today)
                {
                    current = current.AddDays(14);
                }
                break;
            case IncomeFrequency.Monthly:
                while (current < today)
                {
                    current = current.AddMonths(1);
                }
                break;
            case IncomeFrequency.Quarterly:
                while (current < today)
                {
                    current = current.AddMonths(3);
                }
                break;
            case IncomeFrequency.Yearly:
                while (current < today)
                {
                    current = current.AddYears(1);
                }
                break;
            default:
                while (current < today)
                {
                    current = current.AddMonths(1);
                }
                break;
        }

        return current;
    }
}


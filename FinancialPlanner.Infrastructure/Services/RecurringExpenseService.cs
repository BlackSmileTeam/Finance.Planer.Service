using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Domain.Enumerations;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Services;

/// <summary>
/// <para>EF Core implementation of <see cref="IRecurringExpenseService"/>.</para>
/// </summary>
public sealed class RecurringExpenseService : IRecurringExpenseService
{
    private readonly FinancialPlannerDbContext _context;

    public RecurringExpenseService(FinancialPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<RecurringExpenseDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.RecurringExpenses
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Include(e => e.Category)
            .Include(e => e.Subcategory)
            .OrderBy(e => e.Title)
            .Select(e => new RecurringExpenseDto
            {
                Id = e.Id,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : string.Empty,
                SubcategoryId = e.SubcategoryId,
                SubcategoryName = e.Subcategory != null ? e.Subcategory.Name : null,
                Title = e.Title,
                Amount = e.Amount,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Frequency = e.Frequency.ToString(),
                IsActive = e.IsActive,
                IsPlanned = e.IsPlanned,
                Notes = e.Notes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RecurringExpenseDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.RecurringExpenses
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Subcategory)
            .Where(e => e.Id == id && e.UserId == userId)
            .Select(e => new RecurringExpenseDto
            {
                Id = e.Id,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : string.Empty,
                SubcategoryId = e.SubcategoryId,
                SubcategoryName = e.Subcategory != null ? e.Subcategory.Name : null,
                Title = e.Title,
                Amount = e.Amount,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Frequency = e.Frequency.ToString(),
                IsActive = e.IsActive,
                IsPlanned = e.IsPlanned,
                Notes = e.Notes
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<RecurringExpenseDto> CreateAsync(CreateRecurringExpenseRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<IncomeFrequency>(request.Frequency, out var frequency))
        {
            frequency = IncomeFrequency.Monthly;
        }

        var entity = new RecurringExpense
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CategoryId = request.CategoryId,
            SubcategoryId = request.SubcategoryId,
            Title = request.Title,
            Amount = request.Amount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Frequency = frequency,
            IsActive = true,
            IsPlanned = request.IsPlanned,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.RecurringExpenses.Add(entity);

        // Не создаём отдельные записи в expenses для Weekly/BiWeekly — они уже учитываются
        // в План/Факт через recurringByMonth в MonthlySummaryService. Иначе один и тот же
        // расход считался бы дважды (recurringByMonth + plannedExpenseLookup из expenses).

        await _context.SaveChangesAsync(cancellationToken);

        var category = await _context.Categories.AsNoTracking().FirstAsync(c => c.Id == entity.CategoryId, cancellationToken);
        var subcategory = entity.SubcategoryId.HasValue
            ? await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == entity.SubcategoryId.Value, cancellationToken)
            : null;

        return new RecurringExpenseDto
        {
            Id = entity.Id,
            CategoryId = entity.CategoryId,
            CategoryName = category.Name,
            SubcategoryId = entity.SubcategoryId,
            SubcategoryName = subcategory?.Name,
            Title = entity.Title,
            Amount = entity.Amount,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Frequency = entity.Frequency.ToString(),
            IsActive = entity.IsActive,
            IsPlanned = entity.IsPlanned,
            Notes = entity.Notes
        };
    }

    public async Task<RecurringExpenseDto> UpdateAsync(Guid id, UpdateRecurringExpenseRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await _context.RecurringExpenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Recurring expense was not found.");
        }

        if (!Enum.TryParse<IncomeFrequency>(request.Frequency, out var frequency))
        {
            frequency = IncomeFrequency.Monthly;
        }

        entity.CategoryId = request.CategoryId;
        entity.SubcategoryId = request.SubcategoryId;
        entity.Title = request.Title;
        entity.Amount = request.Amount;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Frequency = frequency;
        entity.IsActive = request.IsActive;
        entity.IsPlanned = request.IsPlanned;
        entity.Notes = request.Notes;
        entity.UpdatedAt = DateTime.UtcNow;        

        await _context.SaveChangesAsync(cancellationToken);

        var category = await _context.Categories.AsNoTracking().FirstAsync(c => c.Id == entity.CategoryId, cancellationToken);
        var subcategory = entity.SubcategoryId.HasValue
            ? await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == entity.SubcategoryId.Value, cancellationToken)
            : null;

        return new RecurringExpenseDto
        {
            Id = entity.Id,
            CategoryId = entity.CategoryId,
            CategoryName = category.Name,
            SubcategoryId = entity.SubcategoryId,
            SubcategoryName = subcategory?.Name,
            Title = entity.Title,
            Amount = entity.Amount,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Frequency = entity.Frequency.ToString(),
            IsActive = entity.IsActive,
            IsPlanned = entity.IsPlanned,
            Notes = entity.Notes
        };
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await _context.RecurringExpenses.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Recurring expense was not found.");
        }

        _context.RecurringExpenses.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<RecurringExpenseDto>> GetForecastAsync(DateOnly startDate, DateOnly endDate, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.RecurringExpenses
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.IsActive && e.StartDate <= endDate && (e.EndDate == null || e.EndDate >= startDate))
            .Include(e => e.Category)
            .Include(e => e.Subcategory)
            .Select(e => new RecurringExpenseDto
            {
                Id = e.Id,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : string.Empty,
                SubcategoryId = e.SubcategoryId,
                SubcategoryName = e.Subcategory != null ? e.Subcategory.Name : null,
                Title = e.Title,
                Amount = e.Amount,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Frequency = e.Frequency.ToString(),
                IsActive = e.IsActive,
                IsPlanned = e.IsPlanned,
                Notes = e.Notes
            })
            .ToListAsync(cancellationToken);
    }
}


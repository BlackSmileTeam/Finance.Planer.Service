using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Services;

/// <summary>
/// <para>EF Core implementation of <see cref="IMonthlyPlanService"/>.</para>
/// </summary>
public sealed class MonthlyPlanService : IMonthlyPlanService
{
    private readonly FinancialPlannerDbContext _context;

    /// <summary>
    /// <para>Initializes the service.</para>
    /// </summary>
    public MonthlyPlanService(FinancialPlannerDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<MonthlyPlanDto?> GetAsync(int year, int month, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await _context.MonthlyPlans
            .AsNoTracking()
            .Include(p => p.PlannedBudgets)
            .ThenInclude(b => b.Category)
            .Include(p => p.PlannedBudgets)
            .ThenInclude(b => b.Subcategory)
            .FirstOrDefaultAsync(p => p.PlanYear == year && p.PlanMonth == month && p.UserId == userId, cancellationToken);

        return entity is null ? null : MapToDto(entity);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<MonthlyPlanDto>> GetByYearAsync(int year, Guid userId, CancellationToken cancellationToken)
    {
        var plans = await _context.MonthlyPlans
            .AsNoTracking()
            .Include(p => p.PlannedBudgets)
            .ThenInclude(b => b.Category)
            .Include(p => p.PlannedBudgets)
            .ThenInclude(b => b.Subcategory)
            .Where(p => p.PlanYear == year && p.UserId == userId)
            .OrderBy(p => p.PlanMonth)
            .ToListAsync(cancellationToken);

        return plans.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<MonthlyPlanDto> CreateAsync(CreateMonthlyPlanRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var exists = await _context.MonthlyPlans.AnyAsync(p => p.PlanYear == request.PlanYear && p.PlanMonth == request.PlanMonth && p.UserId == userId, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Plan for the selected month already exists.");
        }

        var plan = new MonthlyPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanYear = request.PlanYear,
            PlanMonth = request.PlanMonth,
            PlannedIncome = request.PlannedIncome,
            PlannedExpense = request.PlannedExpense,
            CarryOver = request.CarryOver,
            ExpectedPayCycles = request.ExpectedPayCycles,
            Notes = request.Notes
        };

        var budgets = request.Budgets.Select(b => new PlannedBudget
        {
            Id = Guid.NewGuid(),
            PlanId = plan.Id,
            CategoryId = b.CategoryId,
            SubcategoryId = b.SubcategoryId,
            PlannedAmount = b.PlannedAmount
        });

        plan.PlannedBudgets = budgets.ToList();

        _context.MonthlyPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);

        var entity = await _context.MonthlyPlans
            .AsNoTracking()
            .Include(p => p.PlannedBudgets)
            .ThenInclude(b => b.Category)
            .Include(p => p.PlannedBudgets)
            .ThenInclude(b => b.Subcategory)
            .FirstAsync(p => p.Id == plan.Id, cancellationToken);

        return MapToDto(entity);
    }

    /// <inheritdoc/>
    public async Task<MonthlyPlanDto> UpdateAsync(Guid id, UpdateMonthlyPlanRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var plan = await _context.MonthlyPlans
            .Include(p => p.PlannedBudgets)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, cancellationToken);
        if (plan is null)
        {
            throw new KeyNotFoundException("Plan was not found.");
        }

        plan.PlannedIncome = request.PlannedIncome;
        plan.PlannedExpense = request.PlannedExpense;
        plan.CarryOver = request.CarryOver;
        plan.ExpectedPayCycles = request.ExpectedPayCycles;
        plan.Notes = request.Notes;
        plan.UpdatedAt = DateTime.UtcNow;

        _context.PlannedBudgets.RemoveRange(plan.PlannedBudgets);
        var newBudgets = request.Budgets.Select(b => new PlannedBudget
        {
            Id = Guid.NewGuid(),
            PlanId = plan.Id,
            CategoryId = b.CategoryId,
            SubcategoryId = b.SubcategoryId,
            PlannedAmount = b.PlannedAmount
        });
        plan.PlannedBudgets = newBudgets.ToList();

        await _context.SaveChangesAsync(cancellationToken);

        var entity = await _context.MonthlyPlans
            .AsNoTracking()
            .Include(p => p.PlannedBudgets)
            .ThenInclude(b => b.Category)
            .Include(p => p.PlannedBudgets)
            .ThenInclude(b => b.Subcategory)
            .FirstAsync(p => p.Id == plan.Id, cancellationToken);

        return MapToDto(entity);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var plan = await _context.MonthlyPlans.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, cancellationToken);
        if (plan is null)
        {
            throw new KeyNotFoundException("Plan was not found.");
        }

        _context.MonthlyPlans.Remove(plan);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static MonthlyPlanDto MapToDto(MonthlyPlan plan)
    {
        return new MonthlyPlanDto
        {
            Id = plan.Id,
            PlanYear = plan.PlanYear,
            PlanMonth = plan.PlanMonth,
            PlannedIncome = plan.PlannedIncome,
            PlannedExpense = plan.PlannedExpense,
            CarryOver = plan.CarryOver,
            ExpectedPayCycles = plan.ExpectedPayCycles,
            Notes = plan.Notes,
            PlannedBudgets = plan.PlannedBudgets
                .OrderBy(b => b.Category?.Name)
                .Select(b => new PlannedBudgetDto
                {
                    Id = b.Id,
                    CategoryId = b.CategoryId,
                    SubcategoryId = b.SubcategoryId,
                    CategoryName = b.Category?.Name ?? string.Empty,
                    SubcategoryName = b.Subcategory?.Name,
                    PlannedAmount = b.PlannedAmount
                })
                .ToList()
        };
    }
}


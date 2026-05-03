using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Requests;

namespace FinancialPlanner.Application.Interfaces;

/// <summary>
/// <para>Provides operations for monthly planning.</para>
/// </summary>
public interface IMonthlyPlanService
{
    /// <summary>
    /// <para>Gets the plan for the specified month.</para>
    /// </summary>
    Task<MonthlyPlanDto?> GetAsync(int year, int month, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Gets all plans for a year.</para>
    /// </summary>
    Task<IReadOnlyCollection<MonthlyPlanDto>> GetByYearAsync(int year, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Creates a plan.</para>
    /// </summary>
    Task<MonthlyPlanDto> CreateAsync(CreateMonthlyPlanRequest request, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Updates a plan.</para>
    /// </summary>
    Task<MonthlyPlanDto> UpdateAsync(Guid id, UpdateMonthlyPlanRequest request, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Deletes a plan.</para>
    /// </summary>
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}


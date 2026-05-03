using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Requests;

namespace FinancialPlanner.Application.Interfaces;

/// <summary>
/// <para>Provides CRUD operations for recurring expenses.</para>
/// </summary>
public interface IRecurringExpenseService
{
    Task<IReadOnlyCollection<RecurringExpenseDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<RecurringExpenseDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<RecurringExpenseDto> CreateAsync(CreateRecurringExpenseRequest request, Guid userId, CancellationToken cancellationToken);
    Task<RecurringExpenseDto> UpdateAsync(Guid id, UpdateRecurringExpenseRequest request, Guid userId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<RecurringExpenseDto>> GetForecastAsync(DateOnly startDate, DateOnly endDate, Guid userId, CancellationToken cancellationToken);
}


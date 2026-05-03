using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Requests;

namespace FinancialPlanner.Application.Interfaces;

/// <summary>
/// <para>Provides CRUD operations for expenses.</para>
/// </summary>
public interface IExpenseService
{
    /// <summary>
    /// <para>Returns expenses filtered by year and month.</para>
    /// </summary>
    Task<IReadOnlyCollection<ExpenseDto>> GetByMonthAsync(int year, int month, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Returns a single expense by id, or null if not found.</para>
    /// </summary>
    Task<ExpenseDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Creates a new expense.</para>
    /// </summary>
    Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Updates an existing expense.</para>
    /// </summary>
    Task<ExpenseDto> UpdateAsync(Guid id, UpdateExpenseRequest request, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Deletes an expense.</para>
    /// </summary>
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Gets pending planned expenses that need to be confirmed.</para>
    /// </summary>
    Task<IReadOnlyCollection<PendingPlannedTransactionDto>> GetPendingPlannedExpensesAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Confirms a planned expense, marking it as actual.</para>
    /// </summary>
    Task ConfirmPlannedExpenseAsync(Guid expenseId, Guid userId, decimal? actualAmount, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Confirms a planned recurring expense by creating an actual expense.</para>
    /// </summary>
    Task ConfirmPlannedRecurringExpenseAsync(Guid recurringExpenseId, DateOnly expenseDate, Guid userId, decimal? actualAmount, CancellationToken cancellationToken);
}


namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents a pending planned expense or income that needs to be confirmed.</para>
/// </summary>
public sealed record PendingPlannedTransactionDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty; // "Expense" or "Income"
    public string Title { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateOnly Date { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public Guid? SubcategoryId { get; init; }
    public string? SubcategoryName { get; init; }
    public string? Description { get; init; }
    public Guid? AccountId { get; init; }
}

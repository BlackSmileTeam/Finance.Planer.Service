namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents a recurring expense DTO.</para>
/// </summary>
public sealed record RecurringExpenseDto
{
    public Guid Id { get; init; }
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public Guid? SubcategoryId { get; init; }
    public string? SubcategoryName { get; init; }
    public required string Title { get; init; }
    public decimal Amount { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string Frequency { get; init; } = "Monthly";
    public bool IsActive { get; init; }
    /// <summary>Gets a value indicating whether this recurring expense is planned (not yet actual).</summary>
    public bool IsPlanned { get; init; }
    public string? Notes { get; init; }
}


namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines the payload needed to update a recurring expense.</para>
/// </summary>
public sealed class UpdateRecurringExpenseRequest
{
    public Guid CategoryId { get; init; }
    public Guid? SubcategoryId { get; init; }
    public required string Title { get; init; }
    public decimal Amount { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string Frequency { get; init; } = "Monthly";
    public bool IsActive { get; init; }
    /// <summary>
    /// <para>Gets or sets a value indicating whether this recurring expense is planned (not yet actual).</para>
    /// </summary>
    public bool IsPlanned { get; init; } = false;
    public string? Notes { get; init; }
}


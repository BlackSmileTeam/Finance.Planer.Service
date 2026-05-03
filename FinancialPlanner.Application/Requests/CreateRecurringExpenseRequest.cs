namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines the payload needed to create a recurring expense.</para>
/// </summary>
public sealed class CreateRecurringExpenseRequest
{
    public Guid CategoryId { get; init; }
    public Guid? SubcategoryId { get; init; }
    public required string Title { get; init; }
    public decimal Amount { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string Frequency { get; init; } = "Monthly";
    /// <summary>
    /// <para>Gets or sets a value indicating whether this recurring expense is planned (not yet actual).</para>
    /// </summary>
    public bool IsPlanned { get; init; } = false;
    public string? Notes { get; init; }
}


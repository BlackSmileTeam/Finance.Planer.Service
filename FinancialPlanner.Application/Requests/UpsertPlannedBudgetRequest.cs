namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Describes a single planned budget line.</para>
/// </summary>
public sealed class UpsertPlannedBudgetRequest
{
    /// <summary>
    /// <para>Gets or sets the category identifier.</para>
    /// </summary>
    public Guid CategoryId { get; init; }

    /// <summary>
    /// <para>Gets or sets the optional subcategory identifier.</para>
    /// </summary>
    public Guid? SubcategoryId { get; init; }

    /// <summary>
    /// <para>Gets or sets the planned amount.</para>
    /// </summary>
    public decimal PlannedAmount { get; init; }
}


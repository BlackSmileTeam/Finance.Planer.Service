namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents a planned spending item inside a monthly plan.</para>
/// </summary>
public sealed record PlannedBudgetDto
{
    /// <summary>
    /// <para>Gets or sets the identifier.</para>
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// <para>Gets or sets the identifier of the category.</para>
    /// </summary>
    public Guid CategoryId { get; init; }

    /// <summary>
    /// <para>Gets or sets the optional identifier of the subcategory.</para>
    /// </summary>
    public Guid? SubcategoryId { get; init; }

    /// <summary>
    /// <para>Gets or sets the name of the category.</para>
    /// </summary>
    public required string CategoryName { get; init; }

    /// <summary>
    /// <para>Gets or sets the name of the subcategory.</para>
    /// </summary>
    public string? SubcategoryName { get; init; }

    /// <summary>
    /// <para>Gets or sets the planned amount.</para>
    /// </summary>
    public decimal PlannedAmount { get; init; }
}


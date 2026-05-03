namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Defines a planned spending limit for a specific category inside a monthly
/// plan and allows comparing plan versus actual values.
/// </para>
/// </summary>
public sealed class PlannedBudget
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the budget entry.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the parent monthly plan.</para>
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the related category.</para>
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// <para>Gets or sets the optional identifier of the related subcategory.</para>
    /// </summary>
    public Guid? SubcategoryId { get; set; }

    /// <summary>
    /// <para>Gets or sets the amount that is planned to be spent.</para>
    /// </summary>
    public decimal PlannedAmount { get; set; }

    /// <summary>
    /// <para>Gets or sets the creation timestamp in UTC.</para>
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Gets or sets the last update timestamp in UTC.</para>
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Navigation property to the parent plan.</para>
    /// </summary>
    public MonthlyPlan? Plan { get; set; }

    /// <summary>
    /// <para>Navigation property to the related category.</para>
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// <para>Navigation property to the related subcategory.</para>
    /// </summary>
    public Category? Subcategory { get; set; }

    /// <summary>
    /// <para>Gets the actual expenses linked to this plan entry.</para>
    /// </summary>
    public ICollection<Expense> Expenses { get; init; } = new HashSet<Expense>();
}


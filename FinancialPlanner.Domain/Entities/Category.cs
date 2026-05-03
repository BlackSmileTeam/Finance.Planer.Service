namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Describes a budgeting category that groups both planned and actual expenses,
/// allowing the system to analyze spending patterns with a consistent taxonomy.
/// </para>
/// </summary>
public sealed class Category
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the category.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the user who owns this category.</para>
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// <para>Gets or sets the optional identifier of the parent category (for subcategories).</para>
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// <para>Gets or sets the user friendly name that is shown in UI elements.</para>
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the hexadecimal color code that can be reused on charts.</para>
    /// </summary>
    public string HexColor { get; set; } = "#3B82F6";

    /// <summary>
    /// <para>Gets or sets the icon identifier or name for the category.</para>
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether the category can be selected.</para>
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// <para>Gets or sets the creation timestamp in UTC.</para>
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Gets or sets the last update timestamp in UTC.</para>
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Gets the expenses that belong to the current category.</para>
    /// </summary>
    public ICollection<Expense> Expenses { get; init; } = new HashSet<Expense>();

    /// <summary>
    /// <para>Gets the planned budgets that target the current category.</para>
    /// </summary>
    public ICollection<PlannedBudget> PlannedBudgets { get; init; } = new HashSet<PlannedBudget>();

    /// <summary>
    /// <para>Gets the subcategories that belong to this category.</para>
    /// </summary>
    public ICollection<Category> Subcategories { get; init; } = new HashSet<Category>();

    /// <summary>
    /// <para>Navigation property to the parent category.</para>
    /// </summary>
    public Category? Parent { get; set; }

    /// <summary>
    /// <para>Navigation property to the user who owns this category.</para>
    /// </summary>
    public User? User { get; set; }
}


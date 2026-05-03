namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Represents the budgeting template for a particular month and keeps both the
/// planned metrics and the carry-over from the previous period.
/// </para>
/// </summary>
public sealed class MonthlyPlan
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the plan.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the user who owns this plan.</para>
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// <para>Gets or sets the calendar year of the plan.</para>
    /// </summary>
    public int PlanYear { get; set; }

    /// <summary>
    /// <para>Gets or sets the calendar month of the plan.</para>
    /// </summary>
    public int PlanMonth { get; set; }

    /// <summary>
    /// <para>Gets or sets the amount of income expected during the month.</para>
    /// </summary>
    public decimal PlannedIncome { get; set; }

    /// <summary>
    /// <para>Gets or sets the planned total expense budget for the month.</para>
    /// </summary>
    public decimal PlannedExpense { get; set; }

    /// <summary>
    /// <para>Gets or sets the carry-over funds coming from the previous month.</para>
    /// </summary>
    public decimal CarryOver { get; set; }

    /// <summary>
    /// <para>Gets or sets the number of pay cycles expected for the month.</para>
    /// </summary>
    public int ExpectedPayCycles { get; set; } = 2;

    /// <summary>
    /// <para>Gets or sets optional notes provided by the planner.</para>
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// <para>Gets or sets the creation timestamp in UTC.</para>
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Gets or sets the last update timestamp in UTC.</para>
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Navigation property to the detailed budget lines.</para>
    /// </summary>
    public ICollection<PlannedBudget> PlannedBudgets { get; set; } = new HashSet<PlannedBudget>();

    /// <summary>
    /// <para>Navigation property to the generated monthly snapshot.</para>
    /// </summary>
    public MonthlySnapshot? Snapshot { get; set; }

    /// <summary>
    /// <para>Navigation property to the user who owns this plan.</para>
    /// </summary>
    public User? User { get; set; }
}


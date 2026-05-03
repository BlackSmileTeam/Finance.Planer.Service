namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Captures the actual performance of a monthly plan so that UI clients can
/// highlight the delta between planned and real values.
/// </para>
/// </summary>
public sealed class MonthlySnapshot
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the snapshot.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the plan that owns the snapshot.</para>
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// <para>Gets or sets the total actual income recorded for the month.</para>
    /// </summary>
    public decimal ActualIncome { get; set; }

    /// <summary>
    /// <para>Gets or sets the total actual expense recorded for the month.</para>
    /// </summary>
    public decimal ActualExpense { get; set; }

    /// <summary>
    /// <para>Gets or sets the resulting closing balance for the month.</para>
    /// </summary>
    public decimal ClosingBalance { get; set; }

    /// <summary>
    /// <para>Gets or sets the timestamp of when the snapshot was generated.</para>
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Navigation property to the parent plan.</para>
    /// </summary>
    public MonthlyPlan? Plan { get; set; }
}


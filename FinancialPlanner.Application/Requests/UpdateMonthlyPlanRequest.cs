namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Payload to update a monthly plan.</para>
/// </summary>
public sealed class UpdateMonthlyPlanRequest
{
    /// <summary>
    /// <para>Gets or sets the planned income.</para>
    /// </summary>
    public decimal PlannedIncome { get; init; }

    /// <summary>
    /// <para>Gets or sets the planned expense.</para>
    /// </summary>
    public decimal PlannedExpense { get; init; }

    /// <summary>
    /// <para>Gets or sets the carry over.</para>
    /// </summary>
    public decimal CarryOver { get; init; }

    /// <summary>
    /// <para>Gets or sets expected pay cycles.</para>
    /// </summary>
    public int ExpectedPayCycles { get; init; }

    /// <summary>
    /// <para>Gets or sets optional notes.</para>
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// <para>Gets or sets the detailed planned budgets.</para>
    /// </summary>
    public IReadOnlyCollection<UpsertPlannedBudgetRequest> Budgets { get; init; } = Array.Empty<UpsertPlannedBudgetRequest>();
}


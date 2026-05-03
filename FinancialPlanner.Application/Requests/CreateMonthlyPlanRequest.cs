namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Payload to create a monthly plan with detailed budgets.</para>
/// </summary>
public sealed class CreateMonthlyPlanRequest
{
    /// <summary>
    /// <para>Gets or sets the year.</para>
    /// </summary>
    public int PlanYear { get; init; }

    /// <summary>
    /// <para>Gets or sets the month.</para>
    /// </summary>
    public int PlanMonth { get; init; }

    /// <summary>
    /// <para>Gets or sets the planned income.</para>
    /// </summary>
    public decimal PlannedIncome { get; init; }

    /// <summary>
    /// <para>Gets or sets the planned expense.</para>
    /// </summary>
    public decimal PlannedExpense { get; init; }

    /// <summary>
    /// <para>Gets or sets the carry over from previous month.</para>
    /// </summary>
    public decimal CarryOver { get; init; }

    /// <summary>
    /// <para>Gets or sets the number of expected pay cycles.</para>
    /// </summary>
    public int ExpectedPayCycles { get; init; } = 2;

    /// <summary>
    /// <para>Gets or sets optional notes.</para>
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// <para>Gets or sets the planned budget lines.</para>
    /// </summary>
    public IReadOnlyCollection<UpsertPlannedBudgetRequest> Budgets { get; init; } = Array.Empty<UpsertPlannedBudgetRequest>();
}


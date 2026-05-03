namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents a monthly plan including its detailed budget lines.</para>
/// </summary>
public sealed record MonthlyPlanDto
{
    /// <summary>
    /// <para>Gets or sets the identifier.</para>
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// <para>Gets or sets the targeted year.</para>
    /// </summary>
    public int PlanYear { get; init; }

    /// <summary>
    /// <para>Gets or sets the targeted month.</para>
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
    /// <para>Gets or sets the carry over from the previous month.</para>
    /// </summary>
    public decimal CarryOver { get; init; }

    /// <summary>
    /// <para>Gets or sets the number of expected pay cycles.</para>
    /// </summary>
    public int ExpectedPayCycles { get; init; }

    /// <summary>
    /// <para>Gets or sets optional notes.</para>
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// <para>Gets or sets the collection of planned budgets.</para>
    /// </summary>
    public IReadOnlyCollection<PlannedBudgetDto> PlannedBudgets { get; init; } = Array.Empty<PlannedBudgetDto>();
}


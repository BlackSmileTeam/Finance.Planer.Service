namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>
/// Provides a monthly snapshot that combines plan metrics, actual performance
/// and UI-friendly color hints used by both web and MAUI clients.
/// </para>
/// </summary>
public sealed record MonthlySummaryDto
{
    /// <summary>
    /// <para>Gets or sets the calendar year.</para>
    /// </summary>
    public int Year { get; init; }

    /// <summary>
    /// <para>Gets or sets the calendar month.</para>
    /// </summary>
    public int Month { get; init; }

    /// <summary>
    /// <para>Gets or sets the planned income for the month.</para>
    /// </summary>
    public decimal PlannedIncome { get; init; }

    /// <summary>
    /// <para>Gets or sets the planned expense for the month.</para>
    /// </summary>
    public decimal PlannedExpense { get; init; }

    /// <summary>
    /// <para>
    /// Gets or sets the full planned expense for the month (what was planned in total),
    /// including items that may have already been realized as actual.
    /// </para>
    /// </summary>
    public decimal FullPlannedExpense { get; init; }

    /// <summary>
    /// <para>Gets or sets the actual income recorded for the month.</para>
    /// </summary>
    public decimal ActualIncome { get; init; }

    /// <summary>
    /// <para>Gets or sets the actual expense recorded for the month.</para>
    /// </summary>
    public decimal ActualExpense { get; init; }

    /// <summary>
    /// <para>Gets or sets the carry-over entering the month.</para>
    /// </summary>
    public decimal CarryOver { get; init; }

    /// <summary>
    /// <para>Gets or sets the calculated closing balance.</para>
    /// </summary>
    public decimal ClosingBalance { get; init; }

    /// <summary>
    /// <para>Gets or sets the balance based on actual income and expense only.</para>
    /// </summary>
    public decimal ActualBalance { get; init; }

    /// <summary>
    /// <para>Gets or sets the balance based on planned income and expense only.</para>
    /// </summary>
    public decimal PlannedBalance { get; init; }

    /// <summary>
    /// <para>
    /// Gets or sets the color hint that informs clients how to highlight the
    /// closing balance (green, orange, red).
    /// </para>
    /// </summary>
    public required string AlertColor { get; init; }
}


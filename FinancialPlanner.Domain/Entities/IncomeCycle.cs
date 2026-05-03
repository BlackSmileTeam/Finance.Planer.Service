using FinancialPlanner.Domain.Enumerations;

namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Represents a single income entry that is part of a recurring pay cycle and
/// drives the available balance for a month.
/// </para>
/// </summary>
public sealed class IncomeCycle
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the income cycle.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the user who owns this income cycle.</para>
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the account where this income is received.</para>
    /// </summary>
    public Guid? AccountId { get; set; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether this income cycle is planned (not yet actual).</para>
    /// </summary>
    public bool IsPlanned { get; set; } = false;

    /// <summary>
    /// <para>Gets or sets the friendly name of the pay source.</para>
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the amount received in the cycle.</para>
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// <para>Gets or sets the calendar date when the income was received.</para>
    /// </summary>
    public DateOnly ReceivedDate { get; set; }

    /// <summary>
    /// <para>Gets or sets the start date when the recurring income begins.</para>
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// <para>Gets or sets the optional end date when the recurring income stops.</para>
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// <para>Gets or sets the date when this income cycle was first confirmed (converted to actual income).</para>
    /// <para>Once set, this cycle should not appear in pending planned income list even if the actual record is deleted.</para>
    /// </summary>
    public DateOnly? FirstConfirmedDate { get; set; }

    /// <summary>
    /// <para>Gets or sets how often the income recurs.</para>
    /// </summary>
    public IncomeFrequency Frequency { get; set; } = IncomeFrequency.BiWeekly;

    /// <summary>
    /// <para>Gets or sets additional notes provided by the user.</para>
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
    /// <para>Navigation property to the user who owns this income cycle.</para>
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// <para>Navigation property to the account where this income is received.</para>
    /// </summary>
    public Account? Account { get; set; }
}


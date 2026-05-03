namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Payload to register a new income cycle.</para>
/// </summary>
public sealed class CreateIncomeRequest
{
    /// <summary>
    /// <para>Gets or sets the title.</para>
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// <para>Gets or sets the amount.</para>
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// <para>Gets or sets the received date.</para>
    /// </summary>
    public DateOnly ReceivedDate { get; init; }

    /// <summary>
    /// <para>Gets or sets the start date.</para>
    /// </summary>
    public DateOnly StartDate { get; init; }

    /// <summary>
    /// <para>Gets or sets the optional end date.</para>
    /// </summary>
    public DateOnly? EndDate { get; init; }

    /// <summary>
    /// <para>Gets or sets the frequency.</para>
    /// </summary>
    public string Frequency { get; init; } = "Monthly";

    /// <summary>
    /// <para>Gets or sets optional notes.</para>
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// <para>Gets or sets the account identifier where this income is received.</para>
    /// </summary>
    public Guid? AccountId { get; init; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether this income is planned (not yet actual).</para>
    /// </summary>
    public bool IsPlanned { get; init; } = false;
}


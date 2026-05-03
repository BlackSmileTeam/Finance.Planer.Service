namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines the payload needed to confirm a planned transaction (income or expense).</para>
/// </summary>
public sealed class ConfirmPlannedTransactionRequest
{
    /// <summary>
    /// <para>Gets or sets the actual amount (can differ from planned amount).</para>
    /// <para>If null, the planned amount will be used.</para>
    /// </summary>
    public decimal? Amount { get; init; }
}

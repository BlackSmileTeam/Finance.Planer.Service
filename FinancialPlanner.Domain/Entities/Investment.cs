namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Represents an investment entry that tracks purchased assets and their current value.
/// </para>
/// </summary>
public sealed class Investment
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the investment.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the user who owns this investment.</para>
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// <para>Gets or sets the friendly name of the investment.</para>
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the type of investment.</para>
    /// </summary>
    public InvestmentType InvestmentType { get; set; } = InvestmentType.Stock;

    /// <summary>
    /// <para>Gets or sets the amount invested (purchase price).</para>
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// <para>Gets or sets the date when the investment was purchased.</para>
    /// </summary>
    public DateOnly PurchaseDate { get; set; }

    /// <summary>
    /// <para>Gets or sets the current value of the investment.</para>
    /// </summary>
    public decimal? CurrentValue { get; set; }

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
    /// <para>Navigation property to the user who owns this investment.</para>
    /// </summary>
    public User? User { get; set; }
}

/// <summary>
/// <para>
/// Represents the type of investment.
/// </para>
/// </summary>
public enum InvestmentType
{
    /// <summary>
    /// <para>Stock investment.</para>
    /// </summary>
    Stock = 0,

    /// <summary>
    /// <para>Bond investment.</para>
    /// </summary>
    Bond = 1,

    /// <summary>
    /// <para>ETF investment.</para>
    /// </summary>
    ETF = 2,

    /// <summary>
    /// <para>Cryptocurrency investment.</para>
    /// </summary>
    Crypto = 3,

    /// <summary>
    /// <para>Real estate investment.</para>
    /// </summary>
    RealEstate = 4,

    /// <summary>
    /// <para>Other type of investment.</para>
    /// </summary>
    Other = 5
}


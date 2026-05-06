namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Represents a user's financial account (card, bank account, cash, savings).
/// </para>
/// </summary>
public sealed class Account
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the account.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the user who owns this account.</para>
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// <para>Gets or sets the account name.</para>
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the account number.</para>
    /// </summary>
    public string? AccountNumber { get; set; }

    /// <summary>
    /// <para>Gets or sets the account type.</para>
    /// </summary>
    public string AccountType { get; set; } = "Card";

    /// <summary>
    /// <para>Gets or sets the current balance.</para>
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// <para>Gets or sets the expiry date (for cards).</para>
    /// </summary>
    public string? ExpiryDate { get; set; }

    /// <summary>
    /// <para>Gets or sets the color for UI display.</para>
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// <para>Gets or sets the currency code (e.g., RUB, USD, EUR).</para>
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether the account is active.</para>
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// <para>Gets or sets the creation timestamp in UTC.</para>
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Gets or sets the last update timestamp in UTC.</para>
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// <para>Gets the user who owns this account.</para>
    /// </summary>
    public User User { get; set; } = null!;
}


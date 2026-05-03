namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines the payload needed to create an account.</para>
/// </summary>
public sealed class CreateAccountRequest
{
    /// <summary>
    /// <para>Gets or sets the account name.</para>
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// <para>Gets or sets the account number.</para>
    /// </summary>
    public string? AccountNumber { get; init; }

    /// <summary>
    /// <para>Gets or sets the account type.</para>
    /// </summary>
    public string AccountType { get; init; } = "Card";

    /// <summary>
    /// <para>Gets or sets the initial balance.</para>
    /// </summary>
    public decimal Balance { get; init; }

    /// <summary>
    /// <para>Gets or sets the card holder name.</para>
    /// </summary>
    public string? CardHolderName { get; init; }

    /// <summary>
    /// <para>Gets or sets the expiry date.</para>
    /// </summary>
    public string? ExpiryDate { get; init; }

    /// <summary>
    /// <para>Gets or sets the color for UI display.</para>
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// <para>Gets or sets the currency code (e.g., RUB, USD, EUR).</para>
    /// </summary>
    public string? Currency { get; init; }
}


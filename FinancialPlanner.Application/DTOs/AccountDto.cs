namespace FinancialPlanner.Application.DTOs;

/// <summary>
/// <para>Represents an account DTO consumed by the UI.</para>
/// </summary>
public sealed class AccountDto
{
    /// <summary>Gets the unique identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the account name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the account number.</summary>
    public string? AccountNumber { get; init; }

    /// <summary>Gets the account type.</summary>
    public required string AccountType { get; init; }

    /// <summary>Gets the current balance.</summary>
    public decimal Balance { get; init; }

    /// <summary>Gets the expiry date.</summary>
    public string? ExpiryDate { get; init; }

    /// <summary>Gets the color for UI display.</summary>
    public string? Color { get; init; }

    /// <summary>Gets the currency code.</summary>
    public string? Currency { get; init; }

    /// <summary>Indicates whether the account is active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets the creation timestamp.</summary>
    public DateTime CreatedAt { get; init; }
}


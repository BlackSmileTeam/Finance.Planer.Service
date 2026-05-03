namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Represents a credit card or loan account that can be used for transactions
/// and tracked for debt management.
/// </para>
/// </summary>
public sealed class CreditAccount
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the credit account.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the identifier of the user who owns this credit account.</para>
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// <para>Gets or sets the friendly name of the credit account.</para>
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the type of credit account (CreditCard or Loan).</para>
    /// </summary>
    public CreditAccountType AccountType { get; set; } = CreditAccountType.CreditCard;

    /// <summary>
    /// <para>Gets or sets the credit limit (for credit cards).</para>
    /// </summary>
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// <para>Gets or sets the monthly payment amount (for loans).</para>
    /// </summary>
    public decimal? MonthlyPayment { get; set; }

    /// <summary>
    /// <para>Gets or sets the total loan amount (for loans).</para>
    /// </summary>
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// <para>Gets or sets the loan term in months (for loans).</para>
    /// </summary>
    public int? TermMonths { get; set; }

    /// <summary>
    /// <para>Gets or sets the payment start date (for loans).</para>
    /// </summary>
    public DateOnly? PaymentStartDate { get; set; }

    /// <summary>
    /// <para>Gets or sets the current balance on the account.</para>
    /// </summary>
    public decimal CurrentBalance { get; set; } = 0;

    /// <summary>
    /// <para>Gets or sets the interest rate (as percentage).</para>
    /// </summary>
    public decimal? InterestRate { get; set; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether the account is active.</para>
    /// </summary>
    public bool IsActive { get; set; } = true;

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
    /// <para>Navigation property to the credit transactions.</para>
    /// </summary>
    public ICollection<CreditTransaction> Transactions { get; init; } = new HashSet<CreditTransaction>();

    /// <summary>
    /// <para>Navigation property to the user who owns this credit account.</para>
    /// </summary>
    public User? User { get; set; }
}

/// <summary>
/// <para>
/// Represents the type of credit account.
/// </para>
/// </summary>
public enum CreditAccountType
{
    /// <summary>
    /// <para>Credit card account.</para>
    /// </summary>
    CreditCard = 0,

    /// <summary>
    /// <para>Loan account.</para>
    /// </summary>
    Loan = 1
}


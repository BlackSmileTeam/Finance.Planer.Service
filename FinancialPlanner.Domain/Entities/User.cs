namespace FinancialPlanner.Domain.Entities;

/// <summary>
/// <para>
/// Represents a user account in the system.
/// </para>
/// </summary>
public sealed class User
{
    /// <summary>
    /// <para>Gets or sets the unique identifier of the user.</para>
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// <para>Gets or sets the username.</para>
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the email address.</para>
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the hashed password.</para>
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// <para>Gets or sets the full name of the user.</para>
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// <para>Gets or sets a value indicating whether the user account is active.</para>
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
    /// <para>Gets the categories that belong to this user.</para>
    /// </summary>
    public ICollection<Category> Categories { get; init; } = new HashSet<Category>();

    /// <summary>
    /// <para>Gets the expenses that belong to this user.</para>
    /// </summary>
    public ICollection<Expense> Expenses { get; init; } = new HashSet<Expense>();

    /// <summary>
    /// <para>Gets the income cycles that belong to this user.</para>
    /// </summary>
    public ICollection<IncomeCycle> IncomeCycles { get; init; } = new HashSet<IncomeCycle>();

    /// <summary>
    /// <para>Gets the monthly plans that belong to this user.</para>
    /// </summary>
    public ICollection<MonthlyPlan> MonthlyPlans { get; init; } = new HashSet<MonthlyPlan>();

    /// <summary>
    /// <para>Gets the recurring expenses that belong to this user.</para>
    /// </summary>
    public ICollection<RecurringExpense> RecurringExpenses { get; init; } = new HashSet<RecurringExpense>();

    /// <summary>
    /// <para>Gets the credit accounts that belong to this user.</para>
    /// </summary>
    public ICollection<CreditAccount> CreditAccounts { get; init; } = new HashSet<CreditAccount>();

    /// <summary>
    /// <para>Gets the credit transactions that belong to this user.</para>
    /// </summary>
    public ICollection<CreditTransaction> CreditTransactions { get; init; } = new HashSet<CreditTransaction>();

    /// <summary>
    /// <para>Gets the credit payment schedules that belong to this user.</para>
    /// </summary>
    public ICollection<CreditPaymentSchedule> CreditPaymentSchedules { get; init; } = new HashSet<CreditPaymentSchedule>();

    /// <summary>
    /// <para>Gets the investments that belong to this user.</para>
    /// </summary>
    public ICollection<Investment> Investments { get; init; } = new HashSet<Investment>();

    /// <summary>
    /// <para>Gets the income records that belong to this user.</para>
    /// </summary>
    public ICollection<IncomeRecord> IncomeRecords { get; init; } = new HashSet<IncomeRecord>();

    /// <summary>
    /// <para>Gets the accounts that belong to this user.</para>
    /// </summary>
    public ICollection<Account> Accounts { get; init; } = new HashSet<Account>();
}


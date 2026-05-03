namespace FinancialPlanner.Application.Requests;

/// <summary>
/// <para>Defines the payload needed to create a credit transaction.</para>
/// </summary>
public sealed class CreateCreditTransactionRequest
{
    public Guid CreditAccountId { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? SubcategoryId { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public bool RecordAsIncome { get; init; } = true;
    public int PaymentMonths { get; init; } = 6;
}


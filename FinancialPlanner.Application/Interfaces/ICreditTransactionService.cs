using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Requests;

namespace FinancialPlanner.Application.Interfaces;

/// <summary>
/// <para>Provides CRUD operations for credit transactions.</para>
/// </summary>
public interface ICreditTransactionService
{
    Task<IReadOnlyCollection<CreditTransactionDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<CreditTransactionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CreditTransactionDto> CreateAsync(CreateCreditTransactionRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task RecordAsIncomeAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PendingCreditPaymentDto>> GetPendingPaymentsAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PendingCreditPaymentDto>> GetCreditPaymentsForMonthAsync(int year, int month, Guid userId, CancellationToken cancellationToken);
    Task ConfirmPaymentAsync(Guid paymentScheduleId, Guid userId, decimal? amount, CancellationToken cancellationToken);
}


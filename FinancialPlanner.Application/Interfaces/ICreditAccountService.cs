using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Requests;

namespace FinancialPlanner.Application.Interfaces;

/// <summary>
/// <para>Provides CRUD operations for credit accounts.</para>
/// </summary>
public interface ICreditAccountService
{
    Task<IReadOnlyCollection<CreditAccountDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<CreditAccountDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<CreditAccountDto> CreateAsync(CreateCreditAccountRequest request, Guid userId, CancellationToken cancellationToken);
    Task<CreditAccountDto> UpdateAsync(Guid id, UpdateCreditAccountRequest request, Guid userId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LoanPaymentForMonthDto>> GetLoanPaymentsForMonthAsync(int year, int month, Guid userId, CancellationToken cancellationToken);
    Task ConfirmLoanPaymentAsync(Guid creditAccountId, int year, int month, int day, decimal? amount, Guid userId, CancellationToken cancellationToken);
}


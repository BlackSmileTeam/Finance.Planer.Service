using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Requests;

namespace FinancialPlanner.Application.Interfaces;

/// <summary>
/// <para>Provides CRUD operations for investments.</para>
/// </summary>
public interface IInvestmentService
{
    Task<IReadOnlyCollection<InvestmentDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<InvestmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<InvestmentDto> CreateAsync(CreateInvestmentRequest request, CancellationToken cancellationToken);
    Task<InvestmentDto> UpdateAsync(Guid id, UpdateInvestmentRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}


using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Requests;

namespace FinancialPlanner.Application.Interfaces;

/// <summary>
/// <para>Provides CRUD operations for income records.</para>
/// </summary>
public interface IIncomeRecordService
{
    Task<IReadOnlyCollection<IncomeRecordDto>> GetAllAsync(int? year, Guid userId, CancellationToken cancellationToken);
    Task<IncomeRecordDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<IncomeRecordDto> CreateAsync(CreateIncomeRecordRequest request, Guid userId, CancellationToken cancellationToken);
    Task<IncomeRecordDto> UpdateAsync(Guid id, CreateIncomeRecordRequest request, Guid userId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}


using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Requests;

namespace FinancialPlanner.Application.Interfaces;

/// <summary>
/// <para>Provides CRUD operations for categories.</para>
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// <para>Returns all categories.</para>
    /// </summary>
    Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Returns a category by identifier.</para>
    /// </summary>
    Task<CategoryDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Creates a new category.</para>
    /// </summary>
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Updates a category.</para>
    /// </summary>
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// <para>Deletes a category.</para>
    /// </summary>
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}


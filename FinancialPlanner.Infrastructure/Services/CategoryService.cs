using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Services;

/// <summary>
/// <para>EF Core implementation of <see cref="ICategoryService"/>.</para>
/// </summary>
public sealed class CategoryService : ICategoryService
{
    private readonly FinancialPlannerDbContext _context;

    /// <summary>
    /// <para>Initializes the service.</para>
    /// </summary>
    public CategoryService(FinancialPlannerDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.UserId == userId && c.ParentId == null)
            .Include(c => c.Subcategories.Where(s => s.UserId == userId))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            HexColor = c.HexColor,
            Icon = c.Icon,
            ParentId = c.ParentId,
            IsActive = c.IsActive,
            Subcategories = c.Subcategories.Select(s => new CategoryDto
            {
                Id = s.Id,
                Name = s.Name,
                HexColor = s.HexColor,
                Icon = s.Icon,
                ParentId = s.ParentId,
                IsActive = s.IsActive
            }).ToList()
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<CategoryDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == id && c.UserId == userId)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                HexColor = c.HexColor,
                Icon = c.Icon,
                ParentId = c.ParentId,
                IsActive = c.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, Guid userId, CancellationToken cancellationToken)
    {
        // Check if category with the same name already exists for this user
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == request.Name, cancellationToken);
        
        if (existingCategory != null)
        {
            throw new InvalidOperationException($"Категория с названием '{request.Name}' уже существует.");
        }

        var entity = new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            HexColor = request.HexColor,
            Icon = request.Icon,
            ParentId = request.ParentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload entity to get database-generated values
        await _context.Entry(entity).ReloadAsync(cancellationToken);

        return new CategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            HexColor = entity.HexColor,
            Icon = entity.Icon,
            ParentId = entity.ParentId,
            IsActive = entity.IsActive
        };
    }

    /// <inheritdoc/>
    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Category was not found.");
        }

        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == id)
                throw new InvalidOperationException("Категория не может быть родителем самой себя.");
            var parentExists = await _context.Categories.AnyAsync(c => c.Id == request.ParentId.Value && c.UserId == userId, cancellationToken);
            if (!parentExists)
                throw new InvalidOperationException("Указанная родительская категория не найдена.");
            var descendantIds = await GetDescendantIdsAsync(id, userId, cancellationToken);
            if (descendantIds.Contains(request.ParentId.Value))
                throw new InvalidOperationException("Нельзя указать подкатегорию в качестве родителя (создание цикла).");
        }

        entity.Name = request.Name;
        entity.HexColor = request.HexColor;
        entity.Icon = request.Icon;
        if (request.IsActive.HasValue)
            entity.IsActive = request.IsActive.Value;
        entity.ParentId = request.ParentId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            HexColor = entity.HexColor,
            Icon = entity.Icon,
            ParentId = entity.ParentId,
            IsActive = entity.IsActive
        };
    }

    private async Task<HashSet<Guid>> GetDescendantIdsAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken)
    {
        var result = new HashSet<Guid>();
        var toProcess = new Queue<Guid>();
        var children = await _context.Categories
            .Where(c => c.ParentId == categoryId && c.UserId == userId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
        foreach (var c in children)
            toProcess.Enqueue(c);
        while (toProcess.TryDequeue(out var current))
        {
            result.Add(current);
            var next = await _context.Categories
                .Where(c => c.ParentId == current && c.UserId == userId)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);
            foreach (var n in next)
                toProcess.Enqueue(n);
        }
        return result;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Category was not found.");
        }

        _context.Categories.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}


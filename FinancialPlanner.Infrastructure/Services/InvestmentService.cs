using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Services;

/// <summary>
/// <para>EF Core implementation of <see cref="IInvestmentService"/>.</para>
/// </summary>
public sealed class InvestmentService : IInvestmentService
{
    private readonly FinancialPlannerDbContext _context;

    public InvestmentService(FinancialPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<InvestmentDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Investments
            .AsNoTracking()
            .OrderByDescending(i => i.PurchaseDate)
            .Select(i => new InvestmentDto
            {
                Id = i.Id,
                Title = i.Title,
                InvestmentType = i.InvestmentType.ToString(),
                Amount = i.Amount,
                PurchaseDate = i.PurchaseDate,
                CurrentValue = i.CurrentValue,
                Notes = i.Notes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InvestmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Investments
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new InvestmentDto
            {
                Id = i.Id,
                Title = i.Title,
                InvestmentType = i.InvestmentType.ToString(),
                Amount = i.Amount,
                PurchaseDate = i.PurchaseDate,
                CurrentValue = i.CurrentValue,
                Notes = i.Notes
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<InvestmentDto> CreateAsync(CreateInvestmentRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<InvestmentType>(request.InvestmentType, out var investmentType))
        {
            investmentType = InvestmentType.Stock;
        }

        var entity = new Investment
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            InvestmentType = investmentType,
            Amount = request.Amount,
            PurchaseDate = request.PurchaseDate,
            CurrentValue = request.CurrentValue,
            Notes = request.Notes
        };

        _context.Investments.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new InvestmentDto
        {
            Id = entity.Id,
            Title = entity.Title,
            InvestmentType = entity.InvestmentType.ToString(),
            Amount = entity.Amount,
            PurchaseDate = entity.PurchaseDate,
            CurrentValue = entity.CurrentValue,
            Notes = entity.Notes
        };
    }

    public async Task<InvestmentDto> UpdateAsync(Guid id, UpdateInvestmentRequest request, CancellationToken cancellationToken)
    {
        var entity = await _context.Investments.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Investment was not found.");
        }

        if (!Enum.TryParse<InvestmentType>(request.InvestmentType, out var investmentType))
        {
            investmentType = InvestmentType.Stock;
        }

        entity.Title = request.Title;
        entity.InvestmentType = investmentType;
        entity.Amount = request.Amount;
        entity.PurchaseDate = request.PurchaseDate;
        entity.CurrentValue = request.CurrentValue;
        entity.Notes = request.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new InvestmentDto
        {
            Id = entity.Id,
            Title = entity.Title,
            InvestmentType = entity.InvestmentType.ToString(),
            Amount = entity.Amount,
            PurchaseDate = entity.PurchaseDate,
            CurrentValue = entity.CurrentValue,
            Notes = entity.Notes
        };
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _context.Investments.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Investment was not found.");
        }

        _context.Investments.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}


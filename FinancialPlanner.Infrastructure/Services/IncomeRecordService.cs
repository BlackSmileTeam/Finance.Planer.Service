using FinancialPlanner.Application.DTOs;
using FinancialPlanner.Application.Interfaces;
using FinancialPlanner.Application.Requests;
using FinancialPlanner.Domain.Entities;
using FinancialPlanner.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialPlanner.Infrastructure.Services;

/// <summary>
/// <para>EF Core implementation of <see cref="IIncomeRecordService"/>.</para>
/// </summary>
public sealed class IncomeRecordService : IIncomeRecordService
{
    private readonly FinancialPlannerDbContext _context;

    public IncomeRecordService(FinancialPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<IncomeRecordDto>> GetAllAsync(int? year, Guid userId, CancellationToken cancellationToken)
    {
        var query = _context.IncomeRecords.AsNoTracking()
            .Where(r => r.UserId == userId);

        if (year.HasValue)
        {
            var start = new DateOnly(year.Value, 1, 1);
            var end = new DateOnly(year.Value, 12, 31);
            query = query.Where(r => r.ReceivedDate >= start && r.ReceivedDate <= end);
        }

        // Не показываем плановые записи, уже подтверждённые (есть отдельная фактическая запись; иначе дубликат в списке)
        query = query.Where(r => !(r.IsPlanned && r.FirstConfirmedDate.HasValue));

        // Возвращаем и фактические, и плановые записи — плановый доход (аванс и т.д.) должен отображаться в том месяце, на который указана дата
        return await query
            .OrderByDescending(r => r.ReceivedDate)
            .Select(r => new IncomeRecordDto
            {
                Id = r.Id,
                IncomeCycleId = r.IncomeCycleId,
                Title = r.Title,
                Amount = r.Amount,
                ReceivedDate = r.ReceivedDate,
                IsFromCredit = r.IsFromCredit,
                Notes = r.Notes,
                IsPlanned = r.IsPlanned
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IncomeRecordDto?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.IncomeRecords
            .AsNoTracking()
            .Where(r => r.Id == id && r.UserId == userId)
            .Select(r => new IncomeRecordDto
            {
                Id = r.Id,
                IncomeCycleId = r.IncomeCycleId,
                Title = r.Title,
                Amount = r.Amount,
                ReceivedDate = r.ReceivedDate,
                IsFromCredit = r.IsFromCredit,
                Notes = r.Notes,
                IsPlanned = r.IsPlanned
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IncomeRecordDto> CreateAsync(CreateIncomeRecordRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = new IncomeRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IncomeCycleId = request.IncomeCycleId,
            Title = request.Title,
            Amount = request.Amount,
            ReceivedDate = request.ReceivedDate,
            IsFromCredit = false,
            Notes = request.Notes,
            IsPlanned = request.IsPlanned,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.IncomeRecords.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new IncomeRecordDto
        {
            Id = entity.Id,
            IncomeCycleId = entity.IncomeCycleId,
            Title = entity.Title,
            Amount = entity.Amount,
            ReceivedDate = entity.ReceivedDate,
            IsFromCredit = entity.IsFromCredit,
            Notes = entity.Notes,
            IsPlanned = entity.IsPlanned
        };
    }

    public async Task<IncomeRecordDto> UpdateAsync(Guid id, CreateIncomeRecordRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var entity = await _context.IncomeRecords.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Income record was not found.");
        }

        entity.IncomeCycleId = request.IncomeCycleId;
        entity.Title = request.Title;
        entity.Amount = request.Amount;
        entity.ReceivedDate = request.ReceivedDate;
        entity.Notes = request.Notes;
        entity.IsPlanned = request.IsPlanned;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new IncomeRecordDto
        {
            Id = entity.Id,
            IncomeCycleId = entity.IncomeCycleId,
            Title = entity.Title,
            Amount = entity.Amount,
            ReceivedDate = entity.ReceivedDate,
            IsFromCredit = entity.IsFromCredit,
            Notes = entity.Notes,
            IsPlanned = entity.IsPlanned
        };
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        // First check if record exists at all
        var entity = await _context.IncomeRecords.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException("Income record was not found.");
        }

        // Then check if it belongs to the user
        if (entity.UserId != userId)
        {
            throw new KeyNotFoundException("Income record was not found.");
        }

        _context.IncomeRecords.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}


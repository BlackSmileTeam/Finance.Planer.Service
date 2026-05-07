using FinancialPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace FinancialPlanner.Infrastructure.Persistence;

public sealed partial class FinancialPlannerDbContext
{
    private void AppendAuditLogs()
    {
        if (_auditActorContext?.SuppressAudit == true)
            return;

        var userId = ChangeTrackerAuditExtensions.TryGetCurrentUserId(_httpContextAccessor, _auditActorContext);
        foreach (var log in ChangeTrackerAuditExtensions.BuildAuditLogs(ChangeTracker, userId))
            AuditLogs.Add(log);
    }

    /// <inheritdoc/>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        try
        {
            AppendAuditLogs();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        catch (DbUpdateException ex) when (IsAuditLogTableMissing(ex))
        {
            return RetrySaveWithoutAudit(() => base.SaveChanges(acceptAllChangesOnSuccess));
        }
    }

    /// <inheritdoc/>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        return SaveChangesAsyncWithAuditFallback(acceptAllChangesOnSuccess, cancellationToken);
    }

    private async Task<int> SaveChangesAsyncWithAuditFallback(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken)
    {
        try
        {
            AppendAuditLogs();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException ex) when (IsAuditLogTableMissing(ex))
        {
            return await RetrySaveWithoutAuditAsync(
                () => base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken));
        }
    }

    private int RetrySaveWithoutAudit(Func<int> saveChangesAction)
    {
        DetachPendingAuditLogs();
        var previous = _auditActorContext?.SuppressAudit ?? false;
        if (_auditActorContext != null)
            _auditActorContext.SuppressAudit = true;
        try
        {
            return saveChangesAction();
        }
        finally
        {
            if (_auditActorContext != null)
                _auditActorContext.SuppressAudit = previous;
        }
    }

    private async Task<int> RetrySaveWithoutAuditAsync(Func<Task<int>> saveChangesAction)
    {
        DetachPendingAuditLogs();
        var previous = _auditActorContext?.SuppressAudit ?? false;
        if (_auditActorContext != null)
            _auditActorContext.SuppressAudit = true;
        try
        {
            return await saveChangesAction();
        }
        finally
        {
            if (_auditActorContext != null)
                _auditActorContext.SuppressAudit = previous;
        }
    }

    private void DetachPendingAuditLogs()
    {
        foreach (var entry in ChangeTracker.Entries<AuditLog>().Where(e => e.State == EntityState.Added).ToList())
            entry.State = EntityState.Detached;
    }

    private static bool IsAuditLogTableMissing(DbUpdateException exception)
    {
        return exception.InnerException is MySqlException mySqlException &&
               mySqlException.Number == 1146 &&
               mySqlException.Message.Contains("audit_logs", StringComparison.OrdinalIgnoreCase);
    }
}

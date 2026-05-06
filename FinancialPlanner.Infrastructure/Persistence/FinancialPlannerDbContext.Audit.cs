namespace FinancialPlanner.Infrastructure.Persistence;

public sealed partial class FinancialPlannerDbContext
{
    private void AppendAuditLogs()
    {
        var userId = ChangeTrackerAuditExtensions.TryGetCurrentUserId(_httpContextAccessor, _auditActorContext);
        foreach (var log in ChangeTrackerAuditExtensions.BuildAuditLogs(ChangeTracker, userId))
            AuditLogs.Add(log);
    }

    /// <inheritdoc/>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        AppendAuditLogs();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc/>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        AppendAuditLogs();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}

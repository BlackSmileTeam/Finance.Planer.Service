namespace FinancialPlanner.Infrastructure.Persistence;

/// <summary>
/// Scoped override for the acting user id when building audit rows (e.g. login before JWT is issued).
/// </summary>
public sealed class AuditActorContext
{
    /// <summary>When set, audit entries use this user id instead of JWT claims.</summary>
    public Guid? ActingUserId { get; set; }
}

using System.Text.Json;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Domain.Common;
using Fintrox.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fintrox.Infrastructure.Audit;

public sealed class AuditSaveChangesInterceptor(
    IAuditContext auditContext,
    TimeProvider timeProvider) : SaveChangesInterceptor
{
    private static readonly HashSet<string> AuditMetadataProperties =
        new(StringComparer.Ordinal)
        {
            nameof(IAuditableEntity.CreatedAtUtc),
            nameof(IAuditableEntity.CreatedByUserId),
            nameof(IAuditableEntity.UpdatedAtUtc),
            nameof(IAuditableEntity.UpdatedByUserId)
        };

    private static readonly string[] SensitiveFragments =
    [
        "password",
        "token",
        "secret",
        "hash",
        "securitystamp"
    ];

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        PrepareAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        PrepareAudit(eventData.Context);

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    private void PrepareAudit(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        RemovePendingAuditEntries(context);
        context.ChangeTracker.DetectChanges();

        var timestamp = timeProvider.GetUtcNow();
        var actorUserId = auditContext.UserId;

        var entries = context.ChangeTracker
            .Entries()
            .Where(entry =>
                entry.Entity is IAuditableEntity &&
                entry.Entity is Entity &&
                entry.State is EntityState.Added
                    or EntityState.Modified
                    or EntityState.Deleted)
            .ToArray();

        if (entries.Length == 0)
        {
            return;
        }

        foreach (var entry in entries)
        {
            if (entry.Entity is not IAuditableEntity auditable)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                auditable.SetCreationAudit(timestamp, actorUserId);
            }
            else if (entry.State == EntityState.Modified)
            {
                auditable.SetModificationAudit(timestamp, actorUserId);
            }
        }

        context.ChangeTracker.DetectChanges();

        foreach (var entry in entries)
        {
            var auditEntry = CreateAuditLogEntry(
                entry,
                timestamp,
                actorUserId);

            context.Set<AuditLogEntry>().Add(auditEntry);
        }
    }

    private AuditLogEntry CreateAuditLogEntry(
        EntityEntry entry,
        DateTimeOffset timestamp,
        Guid? actorUserId)
    {
        var entity = (Entity)entry.Entity;

        return new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            OrganizationId = ResolveOrganizationId(entry.Entity),
            UserId = actorUserId,
            EntityType = entry.Metadata.ClrType.Name,
            EntityId = entity.Id.ToString(),
            Action = ToAuditAction(entry.State),
            ChangesJson = SerializeChanges(entry),
            CorrelationId = NormalizeCorrelationId(
                auditContext.CorrelationId),
            OccurredAtUtc = timestamp
        };
    }

    private Guid? ResolveOrganizationId(object entity)
    {
        return entity switch
        {
            IOrganizationScopedEntity scoped => scoped.OrganizationId,
            Organization organization => organization.Id,
            _ => auditContext.OrganizationId
        };
    }

    private static AuditAction ToAuditAction(EntityState state)
    {
        return state switch
        {
            EntityState.Added => AuditAction.Created,
            EntityState.Modified => AuditAction.Updated,
            EntityState.Deleted => AuditAction.Deleted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "Unsupported audit entity state.")
        };
    }

    private static string SerializeChanges(EntityEntry entry)
    {
        var changes = new Dictionary<string, AuditPropertyChange>(
            StringComparer.Ordinal);

        foreach (var property in entry.Properties)
        {
            var propertyName = property.Metadata.Name;

            if (AuditMetadataProperties.Contains(propertyName))
            {
                continue;
            }

            if (entry.State == EntityState.Modified &&
                !property.IsModified)
            {
                continue;
            }

            var originalValue = entry.State == EntityState.Added
                ? null
                : RedactIfSensitive(propertyName, property.OriginalValue);

            var currentValue = entry.State == EntityState.Deleted
                ? null
                : RedactIfSensitive(propertyName, property.CurrentValue);

            changes[propertyName] = new AuditPropertyChange(
                originalValue,
                currentValue);
        }

        return JsonSerializer.Serialize(changes);
    }

    private static object? RedactIfSensitive(
        string propertyName,
        object? value)
    {
        if (SensitiveFragments.Any(fragment =>
                propertyName.Contains(
                    fragment,
                    StringComparison.OrdinalIgnoreCase)))
        {
            return value is null ? null : "[REDACTED]";
        }

        return value;
    }

    private static string? NormalizeCorrelationId(string? correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return null;
        }

        var normalized = correlationId.Trim();

        return normalized.Length <= 128
            ? normalized
            : normalized[..128];
    }

    private static void RemovePendingAuditEntries(DbContext context)
    {
        var pendingAuditEntries = context.ChangeTracker
            .Entries<AuditLogEntry>()
            .Where(entry => entry.State == EntityState.Added)
            .ToArray();

        foreach (var pendingEntry in pendingAuditEntries)
        {
            pendingEntry.State = EntityState.Detached;
        }
    }

    private sealed record AuditPropertyChange(
        object? OldValue,
        object? NewValue);
}

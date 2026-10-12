namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped FieldServiceTravel domain record. No cross-module navigation properties.</summary>
public sealed class FieldServiceTravel : OrganizationEntity
{
    public FieldServiceTravel(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fieldServiceVisitId,
        decimal distanceKm,
        decimal travelHours,
        decimal expenseAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        FieldServiceVisitId = DomainRules.Id(fieldServiceVisitId, nameof(fieldServiceVisitId));
        DistanceKm = DomainRules.Nonnegative(distanceKm, nameof(distanceKm));
        TravelHours = DomainRules.Nonnegative(travelHours, nameof(travelHours));
        ExpenseAmount = DomainRules.Nonnegative(expenseAmount, nameof(expenseAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid FieldServiceVisitId { get; }
    public decimal DistanceKm { get; }
    public decimal TravelHours { get; }
    public decimal ExpenseAmount { get; }
    public string Currency { get; }
}

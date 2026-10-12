namespace Fintrox.Modules.Workflows.Domain.Entities;

/// <summary>Immutable, module-owned WorkflowNotificationTemplate; tenant scope and input fields validated at creation.</summary>
public sealed class WorkflowNotificationTemplate : OrganizationEntity
{
    public WorkflowNotificationTemplate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string templateCode,
        string channel,
        string subjectTemplate,
        string bodyTemplate,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        TemplateCode = DomainRules.Text(templateCode, nameof(templateCode));
        Channel = DomainRules.Text(channel, nameof(channel));
        SubjectTemplate = DomainRules.Text(subjectTemplate, nameof(subjectTemplate));
        BodyTemplate = DomainRules.Text(bodyTemplate, nameof(bodyTemplate));
        IsActive = isActive;
    }

    public string TemplateCode { get; }
    public string Channel { get; }
    public string SubjectTemplate { get; }
    public string BodyTemplate { get; }
    public bool IsActive { get; }
}

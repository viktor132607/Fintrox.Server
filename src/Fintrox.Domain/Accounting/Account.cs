using System.Text.RegularExpressions;
using Fintrox.Domain.Common;

namespace Fintrox.Domain.Accounting;

public sealed partial class Account : OrganizationScopedAuditableEntity, IAggregateRoot
{
    private Account()
    {
    }

    private Account(
        Guid id,
        Guid organizationId,
        string code,
        string name,
        AccountType type,
        Guid? parentAccountId,
        bool isAnalytical,
        bool allowManualPosting,
        DateTimeOffset now) : base(id, organizationId, now)
    {
        Code = NormalizeCode(code);
        Name = NormalizeName(name);
        Type = type;
        ParentAccountId = parentAccountId;
        IsAnalytical = isAnalytical;
        AllowManualPosting = allowManualPosting;
        IsActive = true;
    }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public AccountType Type { get; private set; }

    public Guid? ParentAccountId { get; private set; }

    public bool IsAnalytical { get; private set; }

    public bool IsActive { get; private set; }

    public bool AllowManualPosting { get; private set; }

    public static Account Create(
        Guid organizationId,
        string code,
        string name,
        AccountType type,
        Guid? parentAccountId,
        bool isAnalytical,
        bool allowManualPosting,
        DateTimeOffset now)
    {
        return new Account(
            Guid.NewGuid(),
            organizationId,
            code,
            name,
            type,
            parentAccountId,
            isAnalytical,
            allowManualPosting,
            now);
    }

    public void Update(
        string code,
        string name,
        AccountType type,
        Guid? parentAccountId,
        bool isAnalytical,
        bool allowManualPosting,
        DateTimeOffset now)
    {
        if (parentAccountId == Id)
        {
            throw new ArgumentException(
                "An account cannot be its own parent.",
                nameof(parentAccountId));
        }

        Code = NormalizeCode(code);
        Name = NormalizeName(name);
        Type = type;
        ParentAccountId = parentAccountId;
        IsAnalytical = isAnalytical;
        AllowManualPosting = allowManualPosting;
        Touch(now);
    }

    public void Activate(DateTimeOffset now)
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch(now);
    }

    public void Deactivate(DateTimeOffset now)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch(now);
    }

    private static string NormalizeCode(string value)
    {
        var normalized = value?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Account code is required.", nameof(value));
        }

        if (normalized.Length > 32 || !AccountCodeRegex().IsMatch(normalized))
        {
            throw new ArgumentException(
                "Account code must be 1-32 characters and contain only letters, numbers, '.', '/' or '-'.",
                nameof(value));
        }

        return normalized;
    }

    private static string NormalizeName(string value)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Account name is required.", nameof(value));
        }

        if (normalized.Length > 200)
        {
            throw new ArgumentException(
                "Account name cannot exceed 200 characters.",
                nameof(value));
        }

        return normalized;
    }

    [GeneratedRegex(
        "^[A-Z0-9][A-Z0-9./-]*$",
        RegexOptions.CultureInvariant)]
    private static partial Regex AccountCodeRegex();
}

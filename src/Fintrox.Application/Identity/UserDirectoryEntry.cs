namespace Fintrox.Application.Identity;

public sealed record UserDirectoryEntry(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsActive);

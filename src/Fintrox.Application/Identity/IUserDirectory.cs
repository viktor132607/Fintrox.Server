namespace Fintrox.Application.Identity;

public interface IUserDirectory
{
    Task<UserDirectoryEntry?> FindByEmailAsync(string email);

    Task<IReadOnlyDictionary<Guid, UserDirectoryEntry>> ListByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);
}

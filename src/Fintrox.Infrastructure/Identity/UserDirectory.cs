using Fintrox.Application.Identity;
using Fintrox.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Identity;

public sealed class UserDirectory(
    UserManager<ApplicationUser> userManager,
    FintroxDbContext dbContext) : IUserDirectory
{
    public async Task<UserDirectoryEntry?> FindByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        return user is null
            ? null
            : Map(user);
    }

    public async Task<IReadOnlyDictionary<Guid, UserDirectoryEntry>> ListByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, UserDirectoryEntry>();
        }

        var users = await dbContext.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToArrayAsync(cancellationToken);

        return users.ToDictionary(
            user => user.Id,
            Map);
    }

    private static UserDirectoryEntry Map(ApplicationUser user)
    {
        return new UserDirectoryEntry(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.IsActive);
    }
}

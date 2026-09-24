namespace Fintrox.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }

    Guid RequireUserId();
}

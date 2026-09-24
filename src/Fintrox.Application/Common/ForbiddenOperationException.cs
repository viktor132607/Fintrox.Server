namespace Fintrox.Application.Common;

public sealed class ForbiddenOperationException(string message) : Exception(message);

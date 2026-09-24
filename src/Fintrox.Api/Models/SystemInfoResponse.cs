namespace Fintrox.Api.Models;

public sealed record SystemInfoResponse(
    string Service,
    string ApiVersion,
    string RuntimeVersion);

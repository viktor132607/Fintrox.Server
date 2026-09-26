using System.Text.Json;

namespace Fintrox.Application.Integrations;

public static class IntegrationPayloadSerializer
{
    private static readonly JsonSerializerOptions Options =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

    public static T Deserialize<T>(string payloadJson)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(
                       payloadJson,
                       Options)
                   ?? throw new IntegrationProcessingException(
                       "Integration payload cannot be null.");
        }
        catch (JsonException exception)
        {
            throw new IntegrationProcessingException(
                $"Integration payload is invalid JSON for '{typeof(T).Name}': {exception.Message}");
        }
    }

    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(
            value,
            Options);
}

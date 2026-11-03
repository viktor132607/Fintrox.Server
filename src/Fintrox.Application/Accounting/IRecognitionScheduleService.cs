using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IRecognitionScheduleService
{
    Task<IReadOnlyList<RecognitionScheduleResponse>> ListAsync(
        CancellationToken cancellationToken);

    Task<RecognitionScheduleDetailResponse?> GetAsync(
        Guid scheduleId, CancellationToken cancellationToken);

    Task<RecognitionScheduleResponse> CreateAsync(
        CreateRecognitionScheduleRequest request, CancellationToken cancellationToken);

    Task<RecognitionGenerationResponse?> GenerateAsync(
        Guid scheduleId, GenerateRecognitionRequest request,
        CancellationToken cancellationToken);
}

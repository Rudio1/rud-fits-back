namespace RudFitAI.Application.Abstractions;

public interface IMealPhotoVisionClient
{
    Task<string> GetMealAnalysisJsonAsync(
        byte[] imageBytes,
        string imageMimeType,
        CancellationToken cancellationToken);
}

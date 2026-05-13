using System.Net.Http.Headers;
using System.Text.Json;
using Application.Dto.AI;
using Application.DTOs;
using Application.Interfaces;
using Domain.Models.AI;
using Infrastructure.DataAccess;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class AiService(
    AppDbContext context,
    IFileStorageService fileStorageService,
    IHttpClientFactory httpClientFactory,
    ILogger<AiService> logger) : IAiService
{
    private const string ModelEndpoint = "https://basel023-coloncancerdetection.hf.space/predict";

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public async Task<Result> AnalyzeImageAsync(int imageId)
    {
        var modelInput = await context.Set<ModelInput>()
            .Include(m => m.Output)
            .FirstOrDefaultAsync(m => m.Id == imageId && !m.IsDeleted);

        if (modelInput is null)
            return Fail("Image not found.");

        byte[] imageBytes;
        try
        {
            imageBytes = await fileStorageService.ReadFileAsync(modelInput.HistopathologyImagePath);
        }
        catch (FileNotFoundException)
        {
            logger.LogWarning("Image file missing on disk for ModelInput {ImageId}", imageId);
            return Fail("Image file not found on disk.");
        }

        AiPredictionResultDto prediction;
        try
        {
            prediction = await CallModelAsync(imageBytes, modelInput.OriginalFileName);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error calling AI model for image {ImageId}", imageId);
            return Fail("Failed to contact the AI model. Please try again later.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error calling AI model for image {ImageId}", imageId);
            return Fail("An unexpected error occurred while analysing the image.");
        }

        var now = DateTime.UtcNow;

        if (modelInput.Output is null)
        {
            var output = new ModelOutput
            {
                ModelInputId   = modelInput.Id,
                Classification = prediction.Label,
                Confidence     = prediction.Probability,
                ProcessedAt    = now
            };
            await context.Set<ModelOutput>().AddAsync(output);
        }
        else
        {
            modelInput.Output.Classification = prediction.Label;
            modelInput.Output.Confidence     = prediction.Probability;
            modelInput.Output.ProcessedAt    = now;
            modelInput.Output.UpdatedAt      = now;
        }

        modelInput.Status    = "Analyzed";
        modelInput.UpdatedAt = now;

        await context.SaveChangesAsync();

        var response = new AiAnalysisResponseDto(
            ImageId:          modelInput.Id,
            OriginalFileName: modelInput.OriginalFileName,
            PatientId:        modelInput.PatientId,
            Label:            prediction.Label,
            Probability:      prediction.Probability,
            IsCancerous:      prediction.Label.Equals("cancerous", StringComparison.OrdinalIgnoreCase),
            AnalyzedAt:       now
        );

        logger.LogInformation(
            "Image {ImageId} analysed successfully - {Label} ({Probability:P1})",
            imageId, prediction.Label, prediction.Probability);

        return new Result { Success = true, Message = "Analysis complete.", Data = response };
    }

    private async Task<AiPredictionResultDto> CallModelAsync(byte[] imageBytes, string fileName)
    {
        var client = httpClientFactory.CreateClient("AiModel");

        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(GetMimeType(fileName));
        form.Add(fileContent, "file", fileName);

        var response = await client.PostAsync(ModelEndpoint, form);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<AiPredictionResultDto>(json, JsonOptions)
               ?? throw new InvalidOperationException("AI model returned an empty or invalid response.");
    }

    private static string GetMimeType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".png"              => "image/png",
            ".jpg" or ".jpeg"   => "image/jpeg",
            ".tiff" or ".tif"   => "image/tiff",
            _                   => "application/octet-stream"
        };

    private static Result Fail(string message) =>
        new() { Success = false, Message = message };
}
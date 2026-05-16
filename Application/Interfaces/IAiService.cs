using Application.DTOs;

namespace Application.Interfaces;

public interface IAiService
{
    Task<Result> AnalyzeImageAsync(int imageId);
}
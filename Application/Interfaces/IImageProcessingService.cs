using Application.Dto.AI;
using Application.DTOs;
using Domain.Models.AI;

namespace Application.Interfaces;

public interface IImageProcessingService
{
    Task<Result> UploadImageAsync(UploadImageDto dto);
    Task<ModelInput?> GetImageByIdAsync(int id);
    Task<IEnumerable<ModelInput>> GetImagesByPatientIdAsync(int patientId);
    Task<bool> DeleteImageAsync(int id);
}

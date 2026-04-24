using Application.Dto;
using Application.Dto.Doctor_approval;
using Application.DTOs;
using Domain.Models;

namespace Application.Interfaces;

public interface IDoctorAdminService
{
    Task<PagedResponse<DoctorRegistrationSummaryDto>> GetPendingDoctorsAsync(int page, int pageSize);
    Task<PagedResponse<DoctorRegistrationSummaryDto>> GetApprovedDoctorsAsync(int page, int pageSize);
    Task<Result> GetDoctorRegistrationAsync(int doctorUserId);
    Task<Result> ApproveDoctorAsync(int adminUserId, ApproveDoctorDto dto);
    Task<Result> RejectDoctorAsync(int adminUserId, RejectDoctorDto dto);
}

using Application.Dto.MedicalRecord;
using Application.DTOs;

namespace Application.Interfaces;

public interface IMedicalRecordService
{
    Task<Result> GetMedicalRecordAsync(int patientId, int requesterId, string requesterRole);
    Task<Result> GetPendingEntriesAsync(int patientId, int doctorId);
    Task<Result> AddAllergyAsync(int patientId, int submitterId, string submitterRole, AddAllergyDto dto);
    Task<Result> AddVisitAsync(int patientId, int submitterId, string submitterRole, AddVisitDto dto);
    Task<Result> AddSurgeryAsync(int patientId, int submitterId, string submitterRole, AddSurgeryDto dto);
    Task<Result> AddTestAsync(int patientId, int submitterId, string submitterRole, AddTestDto dto);
    Task<Result> AddMedicationAsync(int patientId, int submitterId, string submitterRole, AddMedicationDto dto);
    Task<Result> AddFamilyConditionAsync(int patientId, int submitterId, string submitterRole, AddFamilyConditionDto dto);
    Task<Result> UpdateAllergyAsync(int entryId, int requesterId, string requesterRole, UpdateAllergyDto dto);
    Task<Result> UpdateVisitAsync(int entryId, int requesterId, string requesterRole, UpdateVisitDto dto);
    Task<Result> UpdateSurgeryAsync(int entryId, int requesterId, string requesterRole, UpdateSurgeryDto dto);
    Task<Result> UpdateTestAsync(int entryId, int requesterId, string requesterRole, UpdateTestDto dto);
    Task<Result> UpdateMedicationAsync(int entryId, int requesterId, string requesterRole, UpdateMedicationDto dto);
    Task<Result> UpdateFamilyConditionAsync(int entryId, int requesterId, string requesterRole, UpdateFamilyConditionDto dto);
    Task<Result> ReviewAllergyAsync(int entryId, int doctorId, ReviewEntryDto dto);
    Task<Result> ReviewVisitAsync(int entryId, int doctorId, ReviewEntryDto dto);
    Task<Result> ReviewSurgeryAsync(int entryId, int doctorId, ReviewEntryDto dto);
    Task<Result> ReviewTestAsync(int entryId, int doctorId, ReviewEntryDto dto);
    Task<Result> ReviewMedicationAsync(int entryId, int doctorId, ReviewEntryDto dto);
    Task<Result> ReviewFamilyConditionAsync(int entryId, int doctorId, ReviewEntryDto dto);
}
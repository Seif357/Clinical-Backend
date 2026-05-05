using Application.Dto.MedicalRecord;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces;

public interface IMedicationService
{
    Task<IActionResult> GetPatientMedicationsAsync(int patientUserId);
    Task<IActionResult> GetByIdAsync(int patientUserId, int medicationId);
    Task<IActionResult> AddSelfAsync(int patientUserId, AddMedicationDto dto);
    Task<IActionResult> UpdateAsync(int patientUserId, int medicationId, UpdateMedicationDto dto);
    Task<IActionResult> DeleteAsync(int patientUserId, int medicationId);
    Task<IActionResult> GetForPatientAsync(int doctorUserId, int patientUserId);
    Task<MedicationDto?> PrescribeAsync(int patientUserId, int doctorUserId, int doctorRequestId, PrescribeMedicationDto dto);
}
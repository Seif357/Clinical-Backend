using Application.Dto.AuthDto;
using Application.Dto.MedicalRecord;
using Application.DTOs;
using Application.Interfaces;
using Domain.Models;
using Domain.Models.MedicalRecordAttributes;
using Infrastructure.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class MedicationService(AppDbContext context) : IMedicationService
{
    public async Task<IActionResult> GetPatientMedicationsAsync(int patientUserId)
    {
        var record = await GetOrCreateMedicalRecordAsync(patientUserId);
        if (record is null)
            return Fail("Patient not found.");

        var meds = await context.Medications
            .AsNoTracking()
            .Where(m => m.MedicalRecordId == record.Id && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => ToDto(m))
            .ToListAsync();

        return new Result<List<MedicationDto>> { Success = true, Data = meds };
    }

    public async Task<IActionResult> GetByIdAsync(int patientUserId, int medicationId)
    {
        var med = await FindPatientMedicationAsync(patientUserId, medicationId);
        if (med is null)
            return Fail("Medication not found.");

        return new Result<MedicationDto> { Success = true, Data = ToDto(med) };
    }

    public async Task<IActionResult> AddSelfAsync(int patientUserId, AddMedicationDto dto)
    {
        var record = await GetOrCreateMedicalRecordAsync(patientUserId);
        if (record is null)
            return Fail("Patient not found.");

        var med = new Medication
        {
            MedicalRecordId      = record.Id,
            Name                 = dto.Name,
            Dosage               = dto.Dosage,
            Frequency            = dto.Frequency,
            StartDate            = dto.StartDate,
            EndDate              = dto.EndDate,
            ReminderTimes        = dto.ReminderTimes ?? [],
            DaysOfWeek           = dto.DaysOfWeek ?? [],
            Notes                = dto.Notes,
            Source               = MedicationSource.SelfAdded,
            PrescribedByDoctorId = null,
            DoctorRequestId      = null,
            CreatedAt            = DateTime.UtcNow,
            UpdatedAt            = DateTime.UtcNow
        };

        await context.Medications.AddAsync(med);
        await context.SaveChangesAsync();

        return new Result<MedicationDto> { Success = true, Data = ToDto(med), Message = "Medication added." };
    }

    
    public async Task<IActionResult> UpdateAsync(int patientUserId, int medicationId, UpdateMedicationDto dto)
    {
        var med = await FindPatientMedicationAsync(patientUserId, medicationId);
        if (med is null)
            return Fail("Medication not found.");

        if (dto.Name       is not null) med.Name      = dto.Name;
        if (dto.Dosage     is not null) med.Dosage    = dto.Dosage;
        if (dto.Frequency  is not null) med.Frequency = dto.Frequency;
        if (dto.StartDate.HasValue)     med.StartDate = dto.StartDate.Value;
        if (dto.EndDate.HasValue)       med.EndDate   = dto.EndDate;
        if (dto.ReminderTimes is not null) med.ReminderTimes = dto.ReminderTimes;
        if (dto.DaysOfWeek    is not null) med.DaysOfWeek    = dto.DaysOfWeek;
        if (dto.Notes is not null) med.Notes = dto.Notes;

        med.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return new Result<MedicationDto> { Success = true, Data = ToDto(med), Message = "Medication updated." };
    }

    public async Task<IActionResult> DeleteAsync(int patientUserId, int medicationId)
    {
        var med = await FindPatientMedicationAsync(patientUserId, medicationId);
        if (med is null)
            return Fail("Medication not found.");

        med.IsDeleted = true;
        med.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return new Result { Success = true, Message = "Medication removed." };
    }

    public async Task<IActionResult> GetForPatientAsync(int doctorUserId, int patientUserId)
    {
        // Verify doctor exists and is approved
        var doctorExists = await context.Doctors
            .AnyAsync(d => d.UserId == doctorUserId && !d.IsDeleted &&
                           d.ApprovalStatus == Domain.Models.DoctorApprovalStatus.Approved);

        if (!doctorExists)
            return Fail("Doctor not found or not approved.");

        var record = await context.MedicalRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.PatientId == patientUserId);

        if (record is null)
            return new Result<List<MedicationDto>> { Success = true, Data = [] };

        var meds = await context.Medications
            .AsNoTracking()
            .Where(m => m.MedicalRecordId == record.Id && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => ToDto(m))
            .ToListAsync();

        return new Result<List<MedicationDto>> { Success = true, Data = meds };
    }

    public async Task<MedicationDto?> PrescribeAsync(
        int patientUserId,
        int doctorUserId,
        int doctorRequestId,
        PrescribeMedicationDto dto)
    {
        var record = await GetOrCreateMedicalRecordAsync(patientUserId);
        if (record is null) return null;

        var med = new Medication
        {
            MedicalRecordId      = record.Id,
            Name                 = dto.Name,
            Dosage               = dto.Dosage,
            Frequency            = dto.Frequency,
            StartDate            = dto.StartDate,
            EndDate              = dto.EndDate,
            ReminderTimes        = dto.ReminderTimes ?? [],
            DaysOfWeek           = dto.DaysOfWeek ?? [],
            Notes                = dto.Notes,
            Source               = MedicationSource.DoctorPrescribed,
            PrescribedByDoctorId = doctorUserId,
            DoctorRequestId      = doctorRequestId,
            CreatedAt            = DateTime.UtcNow,
            UpdatedAt            = DateTime.UtcNow
        };

        await context.Medications.AddAsync(med);
        return ToDto(med);
    }

    private async Task<MedicalRecord?> GetOrCreateMedicalRecordAsync(int patientUserId)
    {
        var patientExists = await context.Patients
            .AnyAsync(p => p.UserId == patientUserId && !p.IsDeleted);

        if (!patientExists) return null;

        var record = await context.MedicalRecords
            .FirstOrDefaultAsync(r => r.PatientId == patientUserId);

        if (record is not null) return record;

        record = new MedicalRecord { PatientId = patientUserId };
        await context.MedicalRecords.AddAsync(record);
        await context.SaveChangesAsync();
        return record;
    }

    private async Task<Medication?> FindPatientMedicationAsync(int patientUserId, int medicationId)
    {
        return await context.Medications
            .Include(m => m.MedicalRecord)
            .FirstOrDefaultAsync(m =>
                m.Id == medicationId &&
                !m.IsDeleted &&
                m.MedicalRecord.PatientId == patientUserId);
    }

    private static MedicationDto ToDto(Medication m) => new(
        m.Id,
        m.Name,
        m.Dosage,
        m.Frequency,
        m.StartDate,
        m.EndDate,
        m.Status,
        m.ReviewNote,
        m.CreatedAt,
        m.ReminderTimes,
        m.DaysOfWeek,
        m.Notes,
        m.Source,
        m.PrescribedByDoctorId,
        m.DoctorRequestId,
        m.UpdatedAt
    );

    private static Result Fail(string msg) => new() { Success = false, Message = msg };
}
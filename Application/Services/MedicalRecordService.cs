using Application.Dto.MedicalRecord;
using Application.DTOs;
using Application.ExtentionMethods;
using Application.Interfaces;
using Domain.Models.MedicalRecordAttributes;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class MedicalRecordService(AppDbContext context) : IMedicalRecordService
{

    private static Result Ok(string message, object? data = null) =>
        new() { Success = true, Message = message, Data = data };

    private static Result Fail(string message) =>
        new() { Success = false, Message = message };
    
    private async Task<Domain.Models.MedicalRecord?> GetOrCreateRecordAsync(int patientId)
    {
        var patient = await context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == patientId && !p.IsDeleted);

        if (patient is null) return null;

        var record = await context.MedicalRecords
            .FirstOrDefaultAsync(r => r.PatientId == patientId);

        if (record is not null) return record;

        record = new Domain.Models.MedicalRecord { PatientId = patientId };
        context.MedicalRecords.Add(record);
        await context.SaveChangesAsync();
        return record;
    }
    private static MedicalEntryStatus InitialStatus(string role) =>
        role.IsDoctor() ? MedicalEntryStatus.Approved : MedicalEntryStatus.PendingReview;
    private static bool CanSubmit(int patientId, int requesterId, string requesterRole) =>
        requesterRole.IsDoctor() || (requesterRole.IsPatient() && requesterId == patientId);
    private static bool CanUpdate(int submittedByUserId, MedicalEntryStatus status, int requesterId, string requesterRole)
    {
        if (requesterRole.IsDoctor()) return true;
        return requesterRole.IsPatient()
               && requesterId == submittedByUserId
               && status == MedicalEntryStatus.PendingReview;
    }
    
    public async Task<Result> GetMedicalRecordAsync(int patientId, int requesterId, string requesterRole)
    {
        if (requesterRole.IsPatient() && requesterId != patientId)
            return Fail("You are not authorized to view this medical record.");

        var record = await context.MedicalRecords
            .AsNoTracking()
            .Include(r => r.Allergies)
            .Include(r => r.Visits)
            .Include(r => r.Surgeries)
            .Include(r => r.TestsTaken)
            .Include(r => r.PrescribedMedications)
            .Include(r => r.FamilyConditions)
            .FirstOrDefaultAsync(r => r.PatientId == patientId && !r.IsDeleted);

        if (record is null)
            return Fail("Medical record not found.");

        bool isDoctor = requesterRole.IsDoctor();

        static bool ShowEntry(MedicalEntryStatus s, bool doctor) =>
            doctor || s == MedicalEntryStatus.Approved;

        var dto = new MedicalRecordDto(
            Id: record.Id,
            PatientId: record.PatientId,
            Allergies: record.Allergies
                .Where(e => !e.IsDeleted && ShowEntry(e.Status, isDoctor))
                .Select(MapAllergy),
            Visits: record.Visits
                .Where(e => !e.IsDeleted && ShowEntry(e.Status, isDoctor))
                .Select(MapVisit),
            Surgeries: record.Surgeries
                .Where(e => !e.IsDeleted && ShowEntry(e.Status, isDoctor))
                .Select(MapSurgery),
            Tests: record.TestsTaken
                .Where(e => !e.IsDeleted && ShowEntry(e.Status, isDoctor))
                .Select(MapTest),
            Medications: record.PrescribedMedications
                .Where(e => !e.IsDeleted && ShowEntry(e.Status, isDoctor))
                .Select(MapMedication),
            FamilyConditions: record.FamilyConditions
                .Where(e => !e.IsDeleted && ShowEntry(e.Status, isDoctor))
                .Select(MapFamilyCondition)
        );

        return Ok("Medical record retrieved successfully.", dto);
    }

    public async Task<Result> GetPendingEntriesAsync(int patientId, int doctorId)
    {
        var record = await context.MedicalRecords
            .AsNoTracking()
            .Include(r => r.Allergies)
            .Include(r => r.Visits)
            .Include(r => r.Surgeries)
            .Include(r => r.TestsTaken)
            .Include(r => r.PrescribedMedications)
            .Include(r => r.FamilyConditions)
            .FirstOrDefaultAsync(r => r.PatientId == patientId && !r.IsDeleted);

        if (record is null)
            return Fail("Medical record not found.");

        var dto = new PendingEntriesDto(
            Allergies: record.Allergies
                .Where(e => !e.IsDeleted && e.Status == MedicalEntryStatus.PendingReview)
                .Select(MapAllergy),
            Visits: record.Visits
                .Where(e => !e.IsDeleted && e.Status == MedicalEntryStatus.PendingReview)
                .Select(MapVisit),
            Surgeries: record.Surgeries
                .Where(e => !e.IsDeleted && e.Status == MedicalEntryStatus.PendingReview)
                .Select(MapSurgery),
            Tests: record.TestsTaken
                .Where(e => !e.IsDeleted && e.Status == MedicalEntryStatus.PendingReview)
                .Select(MapTest),
            Medications: record.PrescribedMedications
                .Where(e => !e.IsDeleted && e.Status == MedicalEntryStatus.PendingReview)
                .Select(MapMedication),
            FamilyConditions: record.FamilyConditions
                .Where(e => !e.IsDeleted && e.Status == MedicalEntryStatus.PendingReview)
                .Select(MapFamilyCondition)
        );

        return Ok("Pending entries retrieved successfully.", dto);
    }
    
    public async Task<Result> AddAllergyAsync(int patientId, int submitterId, string submitterRole, AddAllergyDto dto)
    {
        if (!CanSubmit(patientId, submitterId, submitterRole))
            return Fail("You are not authorized to add entries to this record.");

        var record = await GetOrCreateRecordAsync(patientId);
        if (record is null) return Fail("Patient not found.");

        var entry = new Allergy
        {
            MedicalRecordId   = record.Id,
            Name              = dto.Name,
            Severity          = dto.Severity,
            Reaction          = dto.Reaction,
            SubmittedByUserId = submitterId,
            Status            = InitialStatus(submitterRole),
            CreatedAt         = DateTime.UtcNow
        };

        context.Allergies.Add(entry);
        await context.SaveChangesAsync();

        var statusMsg = entry.Status == MedicalEntryStatus.Approved
            ? "Allergy added to the medical record."
            : "Allergy submitted and is pending doctor review.";

        return Ok(statusMsg, MapAllergy(entry));
    }

    public async Task<Result> AddVisitAsync(int patientId, int submitterId, string submitterRole, AddVisitDto dto)
    {
        if (!CanSubmit(patientId, submitterId, submitterRole))
            return Fail("You are not authorized to add entries to this record.");

        var record = await GetOrCreateRecordAsync(patientId);
        if (record is null) return Fail("Patient not found.");

        var entry = new Visit
        {
            MedicalRecordId   = record.Id,
            Date              = dto.Date,
            DoctorName        = dto.DoctorName,
            ReasonForVisit    = dto.ReasonForVisit,
            Diagnosis         = dto.Diagnosis,
            Treatment_Plan    = dto.TreatmentPlan,
            SubmittedByUserId = submitterId,
            Status            = InitialStatus(submitterRole),
            CreatedAt         = DateTime.UtcNow
        };

        context.Visits.Add(entry);
        await context.SaveChangesAsync();

        var statusMsg = entry.Status == MedicalEntryStatus.Approved
            ? "Visit added to the medical record."
            : "Visit submitted and is pending doctor review.";

        return Ok(statusMsg, MapVisit(entry));
    }

    public async Task<Result> AddSurgeryAsync(int patientId, int submitterId, string submitterRole, AddSurgeryDto dto)
    {
        if (!CanSubmit(patientId, submitterId, submitterRole))
            return Fail("You are not authorized to add entries to this record.");

        var record = await GetOrCreateRecordAsync(patientId);
        if (record is null) return Fail("Patient not found.");

        var entry = new Surgery
        {
            MedicalRecordId   = record.Id,
            Name              = dto.Name,
            Date              = dto.Date,
            Outcome           = dto.Outcome,
            SubmittedByUserId = submitterId,
            Status            = InitialStatus(submitterRole),
            CreatedAt         = DateTime.UtcNow
        };

        context.Surgeries.Add(entry);
        await context.SaveChangesAsync();

        var statusMsg = entry.Status == MedicalEntryStatus.Approved
            ? "Surgery added to the medical record."
            : "Surgery submitted and is pending doctor review.";

        return Ok(statusMsg, MapSurgery(entry));
    }

    public async Task<Result> AddTestAsync(int patientId, int submitterId, string submitterRole, AddTestDto dto)
    {
        if (!CanSubmit(patientId, submitterId, submitterRole))
            return Fail("You are not authorized to add entries to this record.");

        var record = await GetOrCreateRecordAsync(patientId);
        if (record is null) return Fail("Patient not found.");

        var entry = new TestTaken
        {
            MedicalRecordId   = record.Id,
            Name              = dto.Name,
            Date              = dto.Date,
            Result            = dto.Result,
            SubmittedByUserId = submitterId,
            Status            = InitialStatus(submitterRole),
            CreatedAt         = DateTime.UtcNow
        };

        context.TestsTaken.Add(entry);
        await context.SaveChangesAsync();

        var statusMsg = entry.Status == MedicalEntryStatus.Approved
            ? "Test added to the medical record."
            : "Test submitted and is pending doctor review.";

        return Ok(statusMsg, MapTest(entry));
    }

    public async Task<Result> AddMedicationAsync(int patientId, int submitterId, string submitterRole, AddMedicationDto dto)
    {
        if (!CanSubmit(patientId, submitterId, submitterRole))
            return Fail("You are not authorized to add entries to this record.");

        var record = await GetOrCreateRecordAsync(patientId);
        if (record is null) return Fail("Patient not found.");

        var entry = new PrescribedMedication
        {
            MedicalRecordId   = record.Id,
            Name              = dto.Name,
            Dosage            = dto.Dosage,
            Frequency         = dto.Frequency,
            StartDate         = dto.StartDate,
            EndDate           = dto.EndDate,
            SubmittedByUserId = submitterId,
            Status            = InitialStatus(submitterRole),
            CreatedAt         = DateTime.UtcNow
        };

        context.PrescribedMedications.Add(entry);
        await context.SaveChangesAsync();

        var statusMsg = entry.Status == MedicalEntryStatus.Approved
            ? "Medication added to the medical record."
            : "Medication submitted and is pending doctor review.";

        return Ok(statusMsg, MapMedication(entry));
    }

    public async Task<Result> AddFamilyConditionAsync(int patientId, int submitterId, string submitterRole, AddFamilyConditionDto dto)
    {
        if (!CanSubmit(patientId, submitterId, submitterRole))
            return Fail("You are not authorized to add entries to this record.");

        var record = await GetOrCreateRecordAsync(patientId);
        if (record is null) return Fail("Patient not found.");

        var entry = new FamilyCondition
        {
            MedicalRecordId   = record.Id,
            Name              = dto.Name,
            Relative          = dto.Relative,
            DiagnosisDate     = dto.DiagnosisDate,
            SubmittedByUserId = submitterId,
            Status            = InitialStatus(submitterRole),
            CreatedAt         = DateTime.UtcNow
        };

        context.FamilyConditions.Add(entry);
        await context.SaveChangesAsync();

        var statusMsg = entry.Status == MedicalEntryStatus.Approved
            ? "Family condition added to the medical record."
            : "Family condition submitted and is pending doctor review.";

        return Ok(statusMsg, MapFamilyCondition(entry));
    }
    
    public async Task<Result> UpdateAllergyAsync(int entryId, int requesterId, string requesterRole, UpdateAllergyDto dto)
    {
        var entry = await context.Allergies.FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);
        if (entry is null) return Fail("Allergy entry not found.");

        if (!CanUpdate(entry.SubmittedByUserId, entry.Status, requesterId, requesterRole))
            return Fail("You are not authorized to update this entry, or it has already been reviewed.");

        if (dto.Name     is not null) entry.Name     = dto.Name;
        if (dto.Severity is not null) entry.Severity = dto.Severity;
        if (dto.Reaction is not null) entry.Reaction = dto.Reaction;
        entry.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Ok("Allergy updated successfully.", MapAllergy(entry));
    }

    public async Task<Result> UpdateVisitAsync(int entryId, int requesterId, string requesterRole, UpdateVisitDto dto)
    {
        var entry = await context.Visits.FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);
        if (entry is null) return Fail("Visit entry not found.");

        if (!CanUpdate(entry.SubmittedByUserId, entry.Status, requesterId, requesterRole))
            return Fail("You are not authorized to update this entry, or it has already been reviewed.");

        if (dto.Date          is not null) entry.Date          = dto.Date.Value;
        if (dto.DoctorName    is not null) entry.DoctorName    = dto.DoctorName;
        if (dto.ReasonForVisit is not null) entry.ReasonForVisit = dto.ReasonForVisit;
        if (dto.Diagnosis     is not null) entry.Diagnosis     = dto.Diagnosis;
        if (dto.TreatmentPlan is not null) entry.Treatment_Plan = dto.TreatmentPlan;
        entry.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Ok("Visit updated successfully.", MapVisit(entry));
    }

    public async Task<Result> UpdateSurgeryAsync(int entryId, int requesterId, string requesterRole, UpdateSurgeryDto dto)
    {
        var entry = await context.Surgeries.FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);
        if (entry is null) return Fail("Surgery entry not found.");

        if (!CanUpdate(entry.SubmittedByUserId, entry.Status, requesterId, requesterRole))
            return Fail("You are not authorized to update this entry, or it has already been reviewed.");

        if (dto.Name    is not null) entry.Name    = dto.Name;
        if (dto.Date    is not null) entry.Date    = dto.Date.Value;
        if (dto.Outcome is not null) entry.Outcome = dto.Outcome;
        entry.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Ok("Surgery updated successfully.", MapSurgery(entry));
    }

    public async Task<Result> UpdateTestAsync(int entryId, int requesterId, string requesterRole, UpdateTestDto dto)
    {
        var entry = await context.TestsTaken.FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);
        if (entry is null) return Fail("Test entry not found.");

        if (!CanUpdate(entry.SubmittedByUserId, entry.Status, requesterId, requesterRole))
            return Fail("You are not authorized to update this entry, or it has already been reviewed.");

        if (dto.Name   is not null) entry.Name   = dto.Name;
        if (dto.Date   is not null) entry.Date   = dto.Date.Value;
        if (dto.Result is not null) entry.Result = dto.Result;
        entry.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Ok("Test updated successfully.", MapTest(entry));
    }

    public async Task<Result> UpdateMedicationAsync(int entryId, int requesterId, string requesterRole, UpdateMedicationDto dto)
    {
        var entry = await context.PrescribedMedications.FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);
        if (entry is null) return Fail("Medication entry not found.");

        if (!CanUpdate(entry.SubmittedByUserId, entry.Status, requesterId, requesterRole))
            return Fail("You are not authorized to update this entry, or it has already been reviewed.");

        if (dto.Name      is not null) entry.Name      = dto.Name;
        if (dto.Dosage    is not null) entry.Dosage    = dto.Dosage;
        if (dto.Frequency is not null) entry.Frequency = dto.Frequency;
        if (dto.StartDate is not null) entry.StartDate = dto.StartDate.Value;
        if (dto.EndDate   is not null) entry.EndDate   = dto.EndDate;
        entry.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Ok("Medication updated successfully.", MapMedication(entry));
    }

    public async Task<Result> UpdateFamilyConditionAsync(int entryId, int requesterId, string requesterRole, UpdateFamilyConditionDto dto)
    {
        var entry = await context.FamilyConditions.FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);
        if (entry is null) return Fail("Family condition entry not found.");

        if (!CanUpdate(entry.SubmittedByUserId, entry.Status, requesterId, requesterRole))
            return Fail("You are not authorized to update this entry, or it has already been reviewed.");

        if (dto.Name          is not null) entry.Name          = dto.Name;
        if (dto.Relative      is not null) entry.Relative      = dto.Relative;
        if (dto.DiagnosisDate is not null) entry.DiagnosisDate = dto.DiagnosisDate.Value;
        entry.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Ok("Family condition updated successfully.", MapFamilyCondition(entry));
    }
    
    public async Task<Result> ReviewAllergyAsync(int entryId, int doctorId, ReviewEntryDto dto)
    {
        var entry = await context.Allergies.FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);
        if (entry is null) return Fail("Allergy entry not found.");
        if (entry.Status != MedicalEntryStatus.PendingReview) return Fail("This entry is not pending review.");

        ApplyReview(entry, doctorId, dto);
        await context.SaveChangesAsync();
        return Ok($"Allergy {(dto.Approve ? "approved" : "rejected")} successfully.", MapAllergy(entry));
    }

    public async Task<Result> ReviewVisitAsync(int entryId, int doctorId, ReviewEntryDto dto)
    {
        var entry = await context.Visits.FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);
        if (entry is null) return Fail("Visit entry not found.");
        if (entry.Status != MedicalEntryStatus.PendingReview) return Fail("This entry is not pending review.");

        ApplyReview(entry, doctorId, dto);
        await context.SaveChangesAsync();
        return Ok($"Visit {(dto.Approve ? "approved" : "rejected")} successfully.", MapVisit(entry));
    }

    public async Task<Result> ReviewSurgeryAsync(int entryId, int doctorId, ReviewEntryDto dto)
    {
        var entry = await context.Surgeries.FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);
        if (entry is null) return Fail("Surgery entry not found.");
        if (entry.Status != MedicalEntryStatus.PendingReview) return Fail("This entry is not pending review.");

        ApplyReview(entry, doctorId, dto);
        await context.SaveChangesAsync();
        return Ok($"Surgery {(dto.Approve ? "approved" : "rejected")} successfully.", MapSurgery(entry));
    }

    public async Task<Result> ReviewTestAsync(int entryId, int doctorId, ReviewEntryDto dto)
    {
        var entry = await context.TestsTaken.FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);
        if (entry is null) return Fail("Test entry not found.");
        if (entry.Status != MedicalEntryStatus.PendingReview) return Fail("This entry is not pending review.");

        ApplyReview(entry, doctorId, dto);
        await context.SaveChangesAsync();
        return Ok($"Test {(dto.Approve ? "approved" : "rejected")} successfully.", MapTest(entry));
    }

    public async Task<Result> ReviewMedicationAsync(int entryId, int doctorId, ReviewEntryDto dto)
    {
        var entry = await context.PrescribedMedications.FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);
        if (entry is null) return Fail("Medication entry not found.");
        if (entry.Status != MedicalEntryStatus.PendingReview) return Fail("This entry is not pending review.");

        ApplyReview(entry, doctorId, dto);
        await context.SaveChangesAsync();
        return Ok($"Medication {(dto.Approve ? "approved" : "rejected")} successfully.", MapMedication(entry));
    }

    public async Task<Result> ReviewFamilyConditionAsync(int entryId, int doctorId, ReviewEntryDto dto)
    {
        var entry = await context.FamilyConditions.FirstOrDefaultAsync(e => e.Id == entryId && !e.IsDeleted);
        if (entry is null) return Fail("Family condition entry not found.");
        if (entry.Status != MedicalEntryStatus.PendingReview) return Fail("This entry is not pending review.");

        ApplyReview(entry, doctorId, dto);
        await context.SaveChangesAsync();
        return Ok($"Family condition {(dto.Approve ? "approved" : "rejected")} successfully.", MapFamilyCondition(entry));
    }
    
    private static void ApplyReview(ReviewableEntry entry, int doctorId, ReviewEntryDto dto)
    {
        entry.Status             = dto.Approve ? MedicalEntryStatus.Approved : MedicalEntryStatus.Rejected;
        entry.ReviewedByDoctorId = doctorId;
        entry.ReviewNote         = dto.Note;
        entry.UpdatedAt          = DateTime.UtcNow;
    }
    
    private static AllergyDto MapAllergy(Allergy e) =>
        new(e.Id, e.Name, e.Severity, e.Reaction, e.Status, e.ReviewNote, e.CreatedAt);

    private static VisitDto MapVisit(Visit e) =>
        new(e.Id, e.Date, e.DoctorName, e.ReasonForVisit, e.Diagnosis, e.Treatment_Plan, e.Status, e.ReviewNote, e.CreatedAt);

    private static SurgeryDto MapSurgery(Surgery e) =>
        new(e.Id, e.Name, e.Date, e.Outcome, e.Status, e.ReviewNote, e.CreatedAt);

    private static TestDto MapTest(TestTaken e) =>
        new(e.Id, e.Name, e.Date, e.Result, e.Status, e.ReviewNote, e.CreatedAt);

    private static MedicationDto MapMedication(PrescribedMedication e) =>
        new(e.Id, e.Name, e.Dosage, e.Frequency, e.StartDate, e.EndDate, e.Status, e.ReviewNote, e.CreatedAt);

    private static FamilyConditionDto MapFamilyCondition(FamilyCondition e) =>
        new(e.Id, e.Name, e.Relative, e.DiagnosisDate, e.Status, e.ReviewNote, e.CreatedAt);
}
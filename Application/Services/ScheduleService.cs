using Application.Dto.Schedule;
using Application.DTOs;
using Application.Interfaces;
using Domain.Models.Schedule;
using Infrastructure.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Domain.Models.Auth;

namespace Application.Services;

public class ScheduleService(
    AppDbContext context,
    UserManager<AppUser> userManager) : IScheduleService
{
    public async Task<Result> GenerateSlotsAsync(int doctorUserId, GenerateSlotsDto dto)
    {
        if (dto.SlotDurationMinutes <= 0)
            return Fail("Slot duration must be greater than zero.");

        if (dto.BlockEnd <= dto.BlockStart)
            return Fail("Block end time must be after start time.");

        if ((dto.BlockEnd - dto.BlockStart).TotalMinutes < dto.SlotDurationMinutes)
            return Fail("Block duration is shorter than the requested slot duration.");

        var schedule = await GetOrCreateScheduleAsync(doctorUserId);

        var slots = new List<ScheduleSlot>();
        var cursor = dto.BlockStart;

        while (cursor.AddMinutes(dto.SlotDurationMinutes) <= dto.BlockEnd)
        {
            var slotEnd = cursor.AddMinutes(dto.SlotDurationMinutes);

            var overlaps = await context.ScheduleSlots.AnyAsync(s =>
                s.ScheduleId == schedule.Id &&
                !s.IsDeleted &&
                s.Status != AppointmentStatus.Cancelled &&
                s.StartTime < slotEnd &&
                s.EndTime > cursor);

            if (!overlaps)
            {
                slots.Add(new ScheduleSlot
                {
                    ScheduleId = schedule.Id,
                    StartTime = cursor,
                    EndTime = slotEnd,
                    Status = AppointmentStatus.Available
                });
            }

            cursor = slotEnd;
        }

        if (slots.Count == 0)
            return Fail("No slots could be created — all times overlap with existing slots.");

        await context.ScheduleSlots.AddRangeAsync(slots);
        await context.SaveChangesAsync();

        return Ok($"{slots.Count} slot(s) created successfully.");
    }

    public async Task<Result> CreateSlotAsync(int doctorUserId, CreateSlotDto dto)
    {
        if (dto.EndTime <= dto.StartTime)
            return Fail("End time must be after start time.");

        var schedule = await GetOrCreateScheduleAsync(doctorUserId);

        var overlaps = await context.ScheduleSlots.AnyAsync(s =>
            s.ScheduleId == schedule.Id &&
            !s.IsDeleted &&
            s.Status != AppointmentStatus.Cancelled &&
            s.StartTime < dto.EndTime &&
            s.EndTime > dto.StartTime);

        if (overlaps)
            return Fail("This time slot overlaps with an existing slot.");

        var slot = new ScheduleSlot
        {
            ScheduleId = schedule.Id,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = AppointmentStatus.Available
        };

        await context.ScheduleSlots.AddAsync(slot);
        await context.SaveChangesAsync();

        return Ok("Slot created successfully.", MapSlot(slot, null));
    }

    public async Task<Result> DeleteSlotAsync(int doctorUserId, int slotId)
    {
        var slot = await GetDoctorSlotAsync(doctorUserId, slotId);
        if (slot is null) return Fail("Slot not found.");
        if (slot.IsBooked) return Fail("Cannot delete a booked slot. Cancel it first.");

        slot.IsDeleted = true;
        slot.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return Ok("Slot deleted.");
    }

    public async Task<Result> CompleteSlotAsync(int doctorUserId, CompleteSlotDto dto)
    {
        var slot = await GetDoctorSlotAsync(doctorUserId, dto.SlotId);
        if (slot is null) return Fail("Slot not found.");
        if (slot.Status != AppointmentStatus.Booked) return Fail("Only booked slots can be marked complete.");

        slot.Status = AppointmentStatus.Completed;
        slot.DoctorNotes = dto.DoctorNotes;
        slot.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return Ok("Appointment marked as completed.");
    }

    // ── Doctor: calendar views ────────────────────────────────────────────────

    public async Task<Result> GetMyScheduleAsync(int doctorUserId)
    {
        var schedule = await context.Schedules
            .Include(s => s.Doctor).ThenInclude(d => d.DoctorData)
            .FirstOrDefaultAsync(s => s.DoctorId == doctorUserId && !s.IsDeleted);

        if (schedule is null)
            return Ok("No schedule found.", new List<ScheduleSlotDto>());

        var slots = await GetSlotsWithPatients(schedule.Id, null, null);
        return Ok("Schedule retrieved.", new DoctorScheduleDto(
            schedule.Id,
            schedule.DoctorId,
            schedule.Doctor.DoctorData.UserName ?? "",
            slots));
    }

    public async Task<Result> GetDailyScheduleAsync(int doctorUserId, DateOnly date)
    {
        var schedule = await context.Schedules
            .FirstOrDefaultAsync(s => s.DoctorId == doctorUserId && !s.IsDeleted);

        if (schedule is null)
            return Ok("No schedule.", new List<ScheduleSlotDto>());

        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = date.ToDateTime(TimeOnly.MaxValue);
        var slots = await GetSlotsWithPatients(schedule.Id, dayStart, dayEnd);
        return Ok($"Schedule for {date}.", slots);
    }

    public async Task<Result> GetWeeklyScheduleAsync(int doctorUserId, DateOnly weekStart)
    {
        var schedule = await context.Schedules
            .FirstOrDefaultAsync(s => s.DoctorId == doctorUserId && !s.IsDeleted);

        if (schedule is null)
            return Ok("No schedule.", new List<ScheduleSlotDto>());

        var start = weekStart.ToDateTime(TimeOnly.MinValue);
        var end = weekStart.AddDays(7).ToDateTime(TimeOnly.MaxValue);
        var slots = await GetSlotsWithPatients(schedule.Id, start, end);
        return Ok($"Weekly schedule from {weekStart}.", slots);
    }

    // ── Patient: booking ──────────────────────────────────────────────────────

    public async Task<Result> GetAvailableSlotsAsync(int doctorUserId)
    {
        var schedule = await context.Schedules
            .Include(s => s.Doctor).ThenInclude(d => d.DoctorData)
            .FirstOrDefaultAsync(s => s.DoctorId == doctorUserId && !s.IsDeleted);

        if (schedule is null)
            return Ok("This doctor has no schedule yet.", new List<ScheduleSlotDto>());

        var now = DateTime.UtcNow;
        var slots = await context.ScheduleSlots
            .Where(s => s.ScheduleId == schedule.Id &&
                        !s.IsDeleted &&
                        s.Status == AppointmentStatus.Available &&
                        s.StartTime > now)
            .OrderBy(s => s.StartTime)
            .Select(s => MapSlot(s, null))
            .ToListAsync();

        return Ok($"{slots.Count} available slot(s).", slots);
    }

    public async Task<Result> BookSlotAsync(int patientUserId, BookSlotDto dto)
    {
        var slot = await context.ScheduleSlots
            .Include(s => s.Schedule)
            .FirstOrDefaultAsync(s => s.Id == dto.SlotId && !s.IsDeleted);

        if (slot is null) return Fail("Slot not found.");
        if (slot.Status != AppointmentStatus.Available) return Fail("This slot is not available for booking.");
        if (slot.StartTime <= DateTime.UtcNow) return Fail("Cannot book a slot in the past.");

        // Check patient doesn't already have a booking at the same time with this doctor
        var conflict = await context.ScheduleSlots.AnyAsync(s =>
            s.PatientId == patientUserId &&
            !s.IsDeleted &&
            s.Status == AppointmentStatus.Booked &&
            s.ScheduleId == slot.ScheduleId &&
            s.StartTime < slot.EndTime &&
            s.EndTime > slot.StartTime);

        if (conflict) return Fail("You already have an appointment in this time window.");

        slot.PatientId = patientUserId;
        slot.Status = AppointmentStatus.Booked;
        slot.PatientNotes = dto.PatientNotes;
        slot.BookedAt = DateTime.UtcNow;
        slot.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return Ok("Appointment booked successfully.", MapSlot(slot, null));
    }

    public async Task<Result> RescheduleAsync(int patientUserId, RescheduleDto dto)
    {
        if (dto.OldSlotId == dto.NewSlotId) return Fail("New slot must be different from the current slot.");

        var oldSlot = await context.ScheduleSlots
            .Include(s => s.Schedule)
            .FirstOrDefaultAsync(s => s.Id == dto.OldSlotId && !s.IsDeleted);

        if (oldSlot is null) return Fail("Original slot not found.");
        if (oldSlot.PatientId != patientUserId) return Fail("You do not own this appointment.");
        if (oldSlot.Status != AppointmentStatus.Booked) return Fail("Only booked appointments can be rescheduled.");
        if (oldSlot.StartTime <= DateTime.UtcNow) return Fail("Cannot reschedule a past appointment.");

        var newSlot = await context.ScheduleSlots
            .Include(s => s.Schedule)
            .FirstOrDefaultAsync(s => s.Id == dto.NewSlotId && !s.IsDeleted);

        if (newSlot is null) return Fail("New slot not found.");
        if (newSlot.Status != AppointmentStatus.Available) return Fail("The target slot is not available.");
        if (newSlot.StartTime <= DateTime.UtcNow) return Fail("Cannot book a slot in the past.");

        // Must be same doctor
        if (newSlot.Schedule.DoctorId != oldSlot.Schedule.DoctorId)
            return Fail("You can only reschedule within the same doctor's schedule.");

        // Free the old slot
        oldSlot.PatientId = null;
        oldSlot.Status = AppointmentStatus.Available;
        oldSlot.PatientNotes = null;
        oldSlot.BookedAt = null;
        oldSlot.UpdatedAt = DateTime.UtcNow;

        // Book the new slot
        newSlot.PatientId = patientUserId;
        newSlot.Status = AppointmentStatus.Booked;
        newSlot.PatientNotes = dto.PatientNotes;
        newSlot.BookedAt = DateTime.UtcNow;
        newSlot.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return Ok("Appointment rescheduled successfully.", MapSlot(newSlot, null));
    }

    public async Task<Result> CancelSlotAsync(int callerUserId, CancelSlotDto dto)
    {
        var slot = await context.ScheduleSlots
            .Include(s => s.Schedule)
            .FirstOrDefaultAsync(s => s.Id == dto.SlotId && !s.IsDeleted);

        if (slot is null) return Fail("Slot not found.");
        if (slot.Status != AppointmentStatus.Booked) return Fail("Only booked appointments can be cancelled.");

        // Allow either the patient or the owning doctor to cancel
        bool isPatient = slot.PatientId == callerUserId;
        bool isDoctor = slot.Schedule.DoctorId == callerUserId;

        if (!isPatient && !isDoctor) return Fail("You are not authorised to cancel this appointment.");

        slot.Status = AppointmentStatus.Cancelled;
        slot.CancellationReason = dto.Reason;
        slot.PatientId = null;
        slot.BookedAt = null;
        slot.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return Ok("Appointment cancelled.");
    }

    public async Task<Result> GetMyAppointmentsAsync(int patientUserId)
    {
        var now = DateTime.UtcNow;
        var slots = await context.ScheduleSlots
            .Include(s => s.Schedule).ThenInclude(sc => sc.Doctor).ThenInclude(d => d.DoctorData)
            .Where(s => s.PatientId == patientUserId &&
                        !s.IsDeleted &&
                        s.Status == AppointmentStatus.Booked &&
                        s.StartTime > now)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        var result = slots.Select(s => new
        {
            SlotId = s.Id,
            s.StartTime,
            s.EndTime,
            s.Status,
            s.PatientNotes,
            DoctorName = s.Schedule.Doctor.DoctorData.UserName,
            DoctorId = s.Schedule.DoctorId,
            s.BookedAt
        });

        return Ok($"{slots.Count} upcoming appointment(s).", result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Domain.Models.Schedule.Schedule> GetOrCreateScheduleAsync(int doctorUserId)
    {
        var schedule = await context.Schedules
            .FirstOrDefaultAsync(s => s.DoctorId == doctorUserId && !s.IsDeleted);

        if (schedule is not null) return schedule;

        schedule = new Domain.Models.Schedule.Schedule { DoctorId = doctorUserId };
        await context.Schedules.AddAsync(schedule);
        await context.SaveChangesAsync();
        return schedule;
    }

    private async Task<ScheduleSlot?> GetDoctorSlotAsync(int doctorUserId, int slotId)
    {
        return await context.ScheduleSlots
            .Include(s => s.Schedule)
            .FirstOrDefaultAsync(s =>
                s.Id == slotId &&
                !s.IsDeleted &&
                s.Schedule.DoctorId == doctorUserId);
    }

    private async Task<List<ScheduleSlotDto>> GetSlotsWithPatients(int scheduleId, DateTime? from, DateTime? to)
    {
        var query = context.ScheduleSlots
            .Include(s => s.Patient).ThenInclude(p => p != null ? p.PatientData : null)
            .Where(s => s.ScheduleId == scheduleId && !s.IsDeleted);

        if (from.HasValue) query = query.Where(s => s.StartTime >= from.Value);
        if (to.HasValue) query = query.Where(s => s.EndTime <= to.Value);

        var slots = await query.OrderBy(s => s.StartTime).ToListAsync();
        return slots.Select(s => MapSlot(s, s.Patient?.PatientData?.UserName)).ToList();
    }

    private static ScheduleSlotDto MapSlot(ScheduleSlot s, string? patientName) => new(
        s.Id,
        s.StartTime,
        s.EndTime,
        s.Status,
        s.PatientId,
        patientName,
        s.PatientNotes,
        s.DoctorNotes,
        s.CancellationReason,
        s.BookedAt
    );

    private static Result Ok(string message, object? data = null) =>
        new() { Success = true, Message = message, Data = data };

    private static Result Fail(string message) =>
        new() { Success = false, Message = message };
}
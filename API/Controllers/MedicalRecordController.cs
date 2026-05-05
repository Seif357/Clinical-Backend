using System.Security.Claims;
using Application.Dto.MedicalRecord;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/medical-records")]
[Authorize]
public class MedicalRecordController(IMedicalRecordService medicalRecordService) : ControllerBase
{
    private int? GetUserId()
    {
        var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(val, out var id) ? id : null;
    }

    private string GetUserRole() =>
        User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    [HttpGet("{patientId:int}")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetMedicalRecord(int patientId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.GetMedicalRecordAsync(patientId, userId.Value, GetUserRole());
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpGet("{patientId:int}/pending")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetPendingEntries(int patientId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.GetPendingEntriesAsync(patientId, userId.Value);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("{patientId:int}/allergies")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddAllergy(int patientId, [FromBody] AddAllergyDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.AddAllergyAsync(patientId, userId.Value, GetUserRole(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("{patientId:int}/visits")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddVisit(int patientId, [FromBody] AddVisitDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.AddVisitAsync(patientId, userId.Value, GetUserRole(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("{patientId:int}/surgeries")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddSurgery(int patientId, [FromBody] AddSurgeryDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.AddSurgeryAsync(patientId, userId.Value, GetUserRole(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("{patientId:int}/tests")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddTest(int patientId, [FromBody] AddTestDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.AddTestAsync(patientId, userId.Value, GetUserRole(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpPost("{patientId:int}/medications")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddMedication(int patientId, [FromBody] AddMedicationDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.AddMedicationAsync(patientId, userId.Value, GetUserRole(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("{patientId:int}/family-conditions")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddFamilyCondition(int patientId, [FromBody] AddFamilyConditionDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.AddFamilyConditionAsync(patientId, userId.Value, GetUserRole(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPut("allergies/{entryId:int}")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateAllergy(int entryId, [FromBody] UpdateAllergyDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.UpdateAllergyAsync(entryId, userId.Value, GetUserRole(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("visits/{entryId:int}")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateVisit(int entryId, [FromBody] UpdateVisitDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.UpdateVisitAsync(entryId, userId.Value, GetUserRole(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("surgeries/{entryId:int}")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateSurgery(int entryId, [FromBody] UpdateSurgeryDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.UpdateSurgeryAsync(entryId, userId.Value, GetUserRole(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("tests/{entryId:int}")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateTest(int entryId, [FromBody] UpdateTestDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.UpdateTestAsync(entryId, userId.Value, GetUserRole(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("medications/{entryId:int}")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateMedication(int entryId, [FromBody] UpdateMedicationDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.UpdateMedicationAsync(entryId, userId.Value, GetUserRole(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("family-conditions/{entryId:int}")]
    [Authorize(Roles = "Doctor,Patient")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateFamilyCondition(int entryId, [FromBody] UpdateFamilyConditionDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.UpdateFamilyConditionAsync(entryId, userId.Value, GetUserRole(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }


    [HttpPatch("allergies/{entryId:int}/review")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ReviewAllergy(int entryId, [FromBody] ReviewEntryDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.ReviewAllergyAsync(entryId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("visits/{entryId:int}/review")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ReviewVisit(int entryId, [FromBody] ReviewEntryDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.ReviewVisitAsync(entryId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("surgeries/{entryId:int}/review")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ReviewSurgery(int entryId, [FromBody] ReviewEntryDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.ReviewSurgeryAsync(entryId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("tests/{entryId:int}/review")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ReviewTest(int entryId, [FromBody] ReviewEntryDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.ReviewTestAsync(entryId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("medications/{entryId:int}/review")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ReviewMedication(int entryId, [FromBody] ReviewEntryDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.ReviewMedicationAsync(entryId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("family-conditions/{entryId:int}/review")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ReviewFamilyCondition(int entryId, [FromBody] ReviewEntryDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await medicalRecordService.ReviewFamilyConditionAsync(entryId, userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
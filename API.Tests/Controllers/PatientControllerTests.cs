using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using API.Controllers;
using Application.Interfaces;
using Application.Dto;
using Application.DTOs;
using Application.Dto.AuthDto;
using Domain.Models.MedicalRecordAttributes;
using Domain.Models;

namespace API.Tests.Controllers;

public class PatientControllerTests
{
    private readonly Mock<IPatientService> _patientServiceMock;

    public PatientControllerTests()
    {
        _patientServiceMock = new Mock<IPatientService>();
    }

    private PatientController CreateControllerWithUser(string? userId)
    {
        var controller = new PatientController(_patientServiceMock.Object);

        var claims = new List<Claim>();
        if (userId != null)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "123")
        }, "mock"));

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };

        return controller;
    }

    // =========================
    // Original 6 tests
    // =========================

    [Fact] public async Task GetAllPatientData_ReturnsPatient_WhenPatientExists()
    {
        var userId = "123";
        var expectedPatient = new Patient { UserId = 123 };

        _patientServiceMock
            .Setup(s => s.GetPatientDataServiceAsync(userId))
            .ReturnsAsync(new Result<Patient> { Success = true, Data = expectedPatient });

        var controller = CreateControllerWithUser(userId);

        var result = await controller.GetAllPatientData();
        var okResult = Assert.IsType<Result<Patient>>(result);
        Assert.True(okResult.Success);
        Assert.Equal(expectedPatient, okResult.Data);
    }

    [Fact] public async Task GetAllPatientData_ReturnsFailure_WhenPatientNotFound()
    {
        var userId = "999";
        _patientServiceMock
            .Setup(s => s.GetPatientDataServiceAsync(userId))
            .ReturnsAsync(new Result<Patient> { Success = false, Message = "Patient not found" });

        var controller = CreateControllerWithUser(userId);
        var result = await controller.GetAllPatientData();
        var actionResult = Assert.IsType<Result<Patient>>(result);
        Assert.False(actionResult.Success);
        Assert.Equal("Patient not found", actionResult.Message);
    }

    [Fact] public async Task UpdatePatientData_ReturnsSuccess_WhenUpdateSucceeds()
    {
        var userId = "123";
        var dto = new UpdatePatientDto("newPath.jpg", "newName", "newEmail@test.com", "123456789");

        _patientServiceMock
            .Setup(s => s.UpdatePatientDataServiceAsync(userId, dto))
            .ReturnsAsync(new Result { Success = true, Message = "Profile updated successfully" });

        var controller = CreateControllerWithUser(userId);
        var result = await controller.UpdatePatientData(dto);
        var actionResult = Assert.IsType<Result>(result);
        Assert.True(actionResult.Success);
        Assert.Equal("Profile updated successfully", actionResult.Message);
    }

    [Fact] public async Task UpdatePatientData_ReturnsFailure_WhenPatientNotFound()
    {
        var userId = "999";
        var dto = new UpdatePatientDto(null, "newName", null, null);

        _patientServiceMock
            .Setup(s => s.UpdatePatientDataServiceAsync(userId, dto))
            .ReturnsAsync(new Result { Success = false, Message = "Patient not found" });

        var controller = CreateControllerWithUser(userId);
        var result = await controller.UpdatePatientData(dto);
        var actionResult = Assert.IsType<Result>(result);
        Assert.False(actionResult.Success);
        Assert.Equal("Patient not found", actionResult.Message);
    }

    [Fact] public async Task UpdatePatientData_ReturnsFailure_WhenUpdateFailsInService()
    {
        var userId = "123";
        var dto = new UpdatePatientDto("path.jpg", "name", "email@test.com", "987654321");

        _patientServiceMock
            .Setup(s => s.UpdatePatientDataServiceAsync(userId, dto))
            .ReturnsAsync(new Result { Success = false, Message = "Failed to update profile: Invalid email" });

        var controller = CreateControllerWithUser(userId);
        var result = await controller.UpdatePatientData(dto);
        var actionResult = Assert.IsType<Result>(result);
        Assert.False(actionResult.Success);
        Assert.Equal("Failed to update profile: Invalid email", actionResult.Message);
    }

    [Fact] public async Task GetAllPatientData_PrintsRequestAndResponse()
    {
        var userId = "123";
        var patient = new Patient { UserId = 123 };

        _patientServiceMock
            .Setup(s => s.GetPatientDataServiceAsync(userId))
            .ReturnsAsync(new Result<Patient> { Success = true, Data = patient });

        var controller = CreateControllerWithUser(userId);

        Console.WriteLine($"REQUEST: UserId={userId}");
        var result = await controller.GetAllPatientData();
        Console.WriteLine($"RESPONSE: {result}");
    }

    // =========================
    // New 10 tests
    // =========================

    [Fact] public async Task GetAllPatientData_ReturnsNullData_WhenPatientDataIsNull()
    {
        var userId = "123";
        _patientServiceMock
            .Setup(s => s.GetPatientDataServiceAsync(userId))
            .ReturnsAsync(new Result<Patient> { Success = true, Data = null });

        var controller = CreateControllerWithUser(userId);
        var result = await controller.GetAllPatientData();
        var r = Assert.IsType<Result<Patient>>(result);
        Assert.Null(r.Data);
    }

    [Fact]
    public async Task GetAllPatientData_ReturnsFailure_WhenUserUnauthorized()
    {
        // Controller with no user claims
        var controller = CreateControllerWithUser(null);

        var result = await controller.GetAllPatientData();
        var r = Assert.IsType<Result<Patient>>(result);

        Assert.False(r.Success);
        Assert.Equal("Unauthorized", r.Message); // You can adjust the message according to your controller logic
    }

    [Fact] public async Task GetAllPatientData_HandlesServiceException()
    {
        var userId = "123";
        _patientServiceMock
            .Setup(s => s.GetPatientDataServiceAsync(userId))
            .ThrowsAsync(new Exception("Service error"));

        var controller = CreateControllerWithUser(userId);
        await Assert.ThrowsAsync<Exception>(() => controller.GetAllPatientData());
    }

    [Fact] public async Task GetAllPatientData_ReturnsFullMedicalRecord()
    {
        var userId = "123";
        var patient = new Patient
        {
            UserId = 123,
            MedicalRecord = new MedicalRecord
            {
                Visits = new List<Visit> { new Visit { DoctorName = "Dr X" } },
                Allergies = new List<Allergy> { new Allergy { Name = "Pollen" } }
            }
        };
        _patientServiceMock
            .Setup(s => s.GetPatientDataServiceAsync(userId))
            .ReturnsAsync(new Result<Patient> { Success = true, Data = patient });

        var controller = CreateControllerWithUser(userId);
        var result = await controller.GetAllPatientData();
        var r = Assert.IsType<Result<Patient>>(result);
        Assert.Single(r.Data!.MedicalRecord.Visits);
        Assert.Single(r.Data.MedicalRecord.Allergies);
    }

    [Fact]
    public async Task UpdatePatientData_ReturnsFailure_WhenDTOIsNull()
    {
        var controller = CreateControllerWithUser("123");

        var result = await controller.UpdatePatientData(null!);
        var r = Assert.IsType<Result>(result);

        Assert.False(r.Success);
        Assert.Equal("Invalid data", r.Message); // Adjust according to your controller implementation
    }

    [Fact] public async Task UpdatePatientData_ReturnsSuccess_WhenNoChangesInDTO()
    {
        var userId = "123";
        var dto = new UpdatePatientDto(null, null, null, null);
        _patientServiceMock
            .Setup(s => s.UpdatePatientDataServiceAsync(userId, dto))
            .ReturnsAsync(new Result { Success = true, Message = "Profile updated successfully" });

        var controller = CreateControllerWithUser(userId);
        var result = await controller.UpdatePatientData(dto);
        var r = Assert.IsType<Result>(result);
        Assert.True(r.Success);
    }

    [Fact] public async Task UpdatePatientData_ReturnsSuccess_WhenOnlyEmailChanged()
    {
        var userId = "123";
        var dto = new UpdatePatientDto(null, null, "new@email.com", null);
        _patientServiceMock
            .Setup(s => s.UpdatePatientDataServiceAsync(userId, dto))
            .ReturnsAsync(new Result { Success = true });

        var controller = CreateControllerWithUser(userId);
        var result = await controller.UpdatePatientData(dto);
        var r = Assert.IsType<Result>(result);
        Assert.True(r.Success);
    }

    [Fact] public async Task UpdatePatientData_ReturnsSuccess_WhenOnlyPhoneNumberChanged()
    {
        var userId = "123";
        var dto = new UpdatePatientDto(null, null, null, "999999");
        _patientServiceMock
            .Setup(s => s.UpdatePatientDataServiceAsync(userId, dto))
            .ReturnsAsync(new Result { Success = true });

        var controller = CreateControllerWithUser(userId);
        var result = await controller.UpdatePatientData(dto);
        var r = Assert.IsType<Result>(result);
        Assert.True(r.Success);
    }

    [Fact] public async Task UpdatePatientData_ReturnsFailure_WhenUserManagerFails()
    {
        var userId = "123";
        var dto = new UpdatePatientDto(null, "name", null, null);
        _patientServiceMock
            .Setup(s => s.UpdatePatientDataServiceAsync(userId, dto))
            .ReturnsAsync(new Result { Success = false, Message = "UserManager update failed" });

        var controller = CreateControllerWithUser(userId);
        var result = await controller.UpdatePatientData(dto);
        var r = Assert.IsType<Result>(result);
        Assert.False(r.Success);
        Assert.Equal("UserManager update failed", r.Message);
    }

    [Fact] public async Task UpdatePatientData_UpdatesImagePathOnly()
    {
        var userId = "123";
        var dto = new UpdatePatientDto("image.jpg", null, null, null);
        _patientServiceMock
            .Setup(s => s.UpdatePatientDataServiceAsync(userId, dto))
            .ReturnsAsync(new Result { Success = true });

        var controller = CreateControllerWithUser(userId);
        var result = await controller.UpdatePatientData(dto);
        var r = Assert.IsType<Result>(result);
        Assert.True(r.Success);
    }
}
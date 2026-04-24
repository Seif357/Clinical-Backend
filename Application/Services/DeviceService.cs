using Application.Dto.AuthDto;
using Application.Dto.Device_management;
using Application.DTOs;
using Application.Interfaces;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class DeviceService(
    IDeviceRepository deviceRepository,
    IRefreshTokenRepository refreshTokenRepository,
    AppDbContext context) : IDeviceService
{
    public async Task<Result> GetUserDevicesAsync(int userId, int currentDeviceId)
    {
        var devices = await deviceRepository.GetUserDevicesAsync(userId);
        var dtos = devices.Select(d => new DeviceDto
        {
            Id              = d.Id,
            DeviceName      = d.DeviceName,
            UserAgent       = d.UserAgent,
            IpAddress       = d.IpAddress,
            LastSeenAt      = d.LastSeenAt,
            IsCurrentDevice = d.Id == currentDeviceId
        }).ToList();

        return new Result { Success = true, Data = dtos };
    }

    public async Task<Result> RenameDeviceAsync(int userId, RenameDeviceDto dto)
    {
        var device = await deviceRepository.GetByIdAsync(dto.DeviceId);
        if (device is null || device.UserId != userId)
            return new Result { Success = false, Message = "Device not found." };

        device.DeviceName = dto.DeviceName;
        await context.SaveChangesAsync();
        return new Result { Success = true, Message = "Device renamed." };
    }

    public async Task<Result> LogoutDeviceAsync(int userId, LogoutDeviceDto dto)
    {
        var device = await deviceRepository.GetByIdAsync(dto.DeviceId);
        if (device is null || device.UserId != userId)
            return new Result { Success = false, Message = "Device not found." };

        // Revoke all tokens tied to this device
        var tokens = await context.RefreshTokens
            .Where(rt => rt.DeviceId == dto.DeviceId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt      = DateTime.UtcNow;
            token.ReasonRevoked  = "Logged out by user from device manager";
        }

        await deviceRepository.DeleteAsync(dto.DeviceId, userId);
        await context.SaveChangesAsync();
        return new Result { Success = true, Message = "Device logged out successfully." };
    }
}
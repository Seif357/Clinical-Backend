using Application.Dto.Device_management;
using Application.DTOs;

namespace Application.Interfaces;

public interface IDeviceService
{
    Task<Result> GetUserDevicesAsync(int userId, int currentDeviceId);
    Task<Result> RenameDeviceAsync(int userId, RenameDeviceDto dto);
    Task<Result> LogoutDeviceAsync(int userId, LogoutDeviceDto dto);
}
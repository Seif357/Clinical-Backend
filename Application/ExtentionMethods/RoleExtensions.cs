using Domain.Models;

namespace Application.ExtentionMethods;

public static class RoleExtensions
{
    public static bool IsAdmin(this string role)
    {
        return Enum.TryParse<Role>(role, out var r) && r == Role.Admin;
    }

    public static bool IsDoctor(this string role)
    {
        return Enum.TryParse<Role>(role, out var r) && r == Role.Doctor;
    }

    public static bool IsPatient(this string role)
    {
        return Enum.TryParse<Role>(role, out var r) && r == Role.Patient;
    }
}
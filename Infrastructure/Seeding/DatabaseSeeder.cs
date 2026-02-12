using Domain.Models;
using Domain.Models.Auth;
using Infrastructure.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore.Storage;
using System.Numerics;

namespace Infrastructure.Seeding;

public class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole<int>> roleManager)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        transaction.GetDbTransaction();
        // Seed roles
        string[] roleNames = { "Admin", "Patient", "Doctor" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(
                    new IdentityRole<int>
                    {
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    });
            }
        }
        // Seed admin user
        if (await userManager.FindByNameAsync("admin") == null)
        {
            var adminUser = new AppUser
            {
                UserName = "admin",
                Email = "admin@gmail.com",
                PhoneNumber = "01022003571",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                throw new Exception(string.Join("\n", result.Errors.Select(e => e.Description)));
            }
        }
        if(await userManager.FindByNameAsync("doctor")==null)
        {
            var appUser = new AppUser
            {
                UserName = "doctor",
                Email = "doctor@gmail.com",
                PhoneNumber = "01022003571",
                EmailConfirmed = true

            };
            var result = await userManager.CreateAsync(appUser, "Doctor@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(appUser, "Doctor");
                var userObject = userManager.FindByNameAsync("doctor");
                var doctorUser = new Doctor
                {
                    UserId = userObject.Id,
                    Gender = Gender.Male,
                    ProfessionalPracticeLicense= "Balz",
                    IssuingAuthority= "Egyptian Medical Syndicate"
                };
                var DoctorResult = await context.Doctors.AddAsync(doctorUser);
            }
            else
            {
                throw new Exception(string.Join("\n", result.Errors.Select(e => e.Description)));
            }

        }
        if (await userManager.FindByNameAsync("patient") == null)
        {
            var appUser = new AppUser
            {
                UserName = "patient",
                Email = "patient@gmail.com",
                PhoneNumber = "01022003571",
                EmailConfirmed = true

            };
            var result = await userManager.CreateAsync(appUser, "Patient@123");
            if (result.Succeeded)
            {
                var RoleResult = await userManager.AddToRoleAsync(appUser, "Patient");
                var userObject = userManager.FindByNameAsync("patient");
                    var patientUser = new Patient
                    {
                        UserId = userObject.Id,
                        DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                        BloodType = BloodType.B_Positive,
                        Gender = Gender.Male
                    };
                    var PatientResult = await context.Patients.AddAsync(patientUser);
            }
            else
            {
                throw new Exception(string.Join("\n", result.Errors.Select(e => e.Description)));
            }
        }
        await context.SaveChangesAsync();
        transaction.Commit();
    }
}
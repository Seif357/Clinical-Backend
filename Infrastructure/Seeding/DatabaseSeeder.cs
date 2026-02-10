using Domain.Models;
using Domain.Models.Auth;
using Infrastructure.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Numerics;

namespace Infrastructure.Seeding;

public class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole<int>> roleManager)
    {
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
                PhoneNumber = "01022003571"
            };
            var result = await userManager.CreateAsync(adminUser, "admin");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        if(await userManager.FindByNameAsync("doctor")==null)
        {
            var appUser = new AppUser
            {
                UserName = "doctor",
                Email = "doctor@gmail.com",
                PhoneNumber = "01022003571"
            };
            var result = await userManager.CreateAsync(appUser, "doctor");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(appUser, "Doctor");
                var doctorUser = new Doctor
                {
                    UserId =appUser.Id,
                    Gender = Gender.Male

                };
            }

        }
        if (await userManager.FindByNameAsync("patient") == null)
        {
            var appUser = new AppUser
            {
                UserName = "patient",
                Email = "patient@gmail.com",
                PhoneNumber= "01022003571"
            };
            var result = await userManager.CreateAsync(appUser, "patient");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(appUser, "Patient");
                var patientUser = new Patient
                {
                    UserId = appUser.Id,
                    DateOfBirth = DateOnly.FromDateTime(DateTime.Now),
                    BloodType = BloodType.B_Positive,
                    Gender = Gender.Male
                };
            }
        }
    }
}
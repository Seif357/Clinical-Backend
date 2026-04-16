using Application.Interfaces;
using Application.Services;
using Application.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IImageProcessingService, ImageProcessingService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddValidatorsFromAssemblyContaining<UploadImageValidator>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IPatientService,PatientService>();
        services.AddScoped<IPatientRequestService, PatientRequestService>();
        services.AddScoped<IPatientResponseService, PatientResponseService>();
        services.AddScoped<IDoctorRequestService, DoctorRequestService>();
        services.AddScoped<IDoctorResponseService, DoctorResponseService>();
        
        return services;
    }
}
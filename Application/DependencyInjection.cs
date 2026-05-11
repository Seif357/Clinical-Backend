using Application.Interfaces;
using Application.Services;
using Application.Validators;
using FluentValidation;
using Infrastructure.Services;
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

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IDoctorAdminService, DoctorAdminService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        return services;
    }
}

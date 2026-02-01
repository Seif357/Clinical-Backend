using Domain.Models;
using Domain.Models.Auth;
using Infrastructure;
using Infrastructure.Configuration;
using Infrastructure.DataAccess;
using Infrastructure.DataAccess.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IRefreshTokenRepository, RefreshTokensRepository>();


            services.AddIdentity<AppUser, IdentityRole<int>>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
                        .AddEntityFrameworkStores<AppDbContext>()
                        .AddDefaultTokenProviders();
            services.AddAuthentication(options =>
            {
                //the default jwt bearer is going to be the one we give to you
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
    //the value we are going to give to the add authentication service to use as a default
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        //some of the stuff that is going to be put in the token is stuff in the payload or in the claims(claims are some set of parameters that you want to include i the token)
        //flags to check if a token is valid or not when some one 
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            //a string that is considered a part of the claims
            //it ensures that the token that is checked made by an issuer you want
            ValidIssuer = configuration["Jwt:ValidIssuer"],
            //it ensures that the ones who should use the key are only people that you mention here
            ValidAudience = configuration["Jwt:ValidAudience"],
            ClockSkew = TimeSpan.Zero,
            //this is where it uses the secret key
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!))
        };
    });
            return services;
        }
        public static IServiceCollection AddJwtService(this IServiceCollection services, IConfiguration configuration)
        {
            var secretKey = configuration["Jwt:SecretKey"] ??
                            throw new ApplicationException("SecretKey is missing");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            services.AddSingleton(new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:ValidIssuer"],
                ValidAudience = configuration["Jwt:ValidAudience"],
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = key
            });
            return services;
        }
    }
}

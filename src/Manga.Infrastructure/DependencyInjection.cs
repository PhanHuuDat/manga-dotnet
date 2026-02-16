using Manga.Application.Common.Interfaces;
using Manga.Domain.Interfaces;
using Manga.Infrastructure.Auth;
using Manga.Infrastructure.Email;
using Manga.Infrastructure.ImageProcessing;
using Manga.Infrastructure.Persistence;
using Manga.Infrastructure.Persistence.Interceptors;
using Manga.Infrastructure.Services;
using Manga.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Manga.Infrastructure;

/// <summary>
/// Registers Infrastructure layer services: EF Core, Redis, repositories, interceptors.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core + PostgreSQL
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
            options.UseNpgsql(
                configuration.GetConnectionString("Default"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Redis
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddStackExchangeRedisCache(options =>
            options.Configuration = redisConnectionString);

        services.AddSingleton<IRedisConnectionProvider, RedisConnectionProvider>();

        // Auth services
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
        services.AddSingleton<IAuthSettings, AuthSettingsAdapter>();

        // Email service (conditional: dev logs to console, prod uses SMTP)
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        if (configuration.GetValue<bool>("Email:UseDev"))
            services.AddScoped<IEmailService, DevEmailService>();
        else
            services.AddScoped<IEmailService, SmtpEmailService>();

        // File storage
        services.Configure<FileStorageSettings>(configuration.GetSection(FileStorageSettings.SectionName));
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        // Image processing
        services.Configure<ImageProcessingSettings>(configuration.GetSection(ImageProcessingSettings.SectionName));
        services.AddSingleton<IImageProcessingService, SkiaSharpImageProcessingService>();

        // View tracking (Redis HyperLogLog)
        services.AddSingleton<IViewTrackingService, RedisViewTrackingService>();

        return services;
    }
}

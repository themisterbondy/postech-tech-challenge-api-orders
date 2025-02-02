using System.Reflection;
using Azure.Storage.Queues;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Postech.Fiap.Orders.WebApi.Common.Behavior;
using Postech.Fiap.Orders.WebApi.Common.Messaging;
using Postech.Fiap.Orders.WebApi.Features.Orders.Jobs;
using Postech.Fiap.Orders.WebApi.Features.Orders.Messaging.Queues;
using Postech.Fiap.Orders.WebApi.Features.Orders.Repositories;
using Postech.Fiap.Orders.WebApi.Features.Orders.Services;
using Postech.Fiap.Orders.WebApi.Infrastructure.Queue;
using Postech.Fiap.Orders.WebApi.Persistence;
using Quartz;
using Quartz.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Filters;

namespace Postech.Fiap.Orders.WebApi;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    private static readonly Assembly Assembly = typeof(Program).Assembly;

    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddMediatRConfiguration();
        services.AddSwaggerConfiguration();
        services.AddOpenTelemetryConfiguration();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddUseHealthChecksConfiguration(configuration);
        services.AddValidatorsFromAssembly(Assembly);

        services.AddProblemDetails();
        services.AddCarter();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("SQLConnection")));

        services.AddScoped<IOrderQueueRepository, OrderQueueRepository>();
        services.AddScoped<IOrderQueueService, OrderQueueService>();

        var storageConnectionString = configuration.GetValue<string>("AzureStorageSettings:ConnectionString");
        services.Configure<AzureQueueSettings>(configuration.GetSection("AzureQueueSettings"));
        services.AddScoped(cfg => cfg.GetService<IOptions<AzureQueueSettings>>().Value);
        services.AddSingleton(x => new QueueServiceClient(storageConnectionString));
        services.AddSingleton<IQueue, AzureQueueService>();
        services.AddSingleton<ICreateOrderCommandSubmittedQueueClient, CreateOrderCommandSubmittedQueueClient>();

        services.AddQuartzConfiguration(configuration);
        services.AddJobs();

        return services;
    }

    private static IServiceCollection AddJobs(this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            var orderJobKey = new JobKey("OrderCleanupJob");
            q.AddJob<OrderCleanupJob>(opts => opts.WithIdentity(orderJobKey));
            q.AddTrigger(opts => opts
                .ForJob(orderJobKey)
                .WithIdentity("OrderCleanupJob-trigger")
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(30)
                    .RepeatForever())
                .StartAt(DateBuilder.FutureDate(5, IntervalUnit.Minute))
            );
        });

        services.AddQuartzServer(options => { options.WaitForJobsToComplete = true; });

        return services;
    }

    private static void AddQuartzConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<QuartzOptions>(configuration.GetSection("Quartz"));
        services.AddQuartz(configure =>
        {
            var mergedSubmittedJob = configuration.GetSection(nameof(JobSettings)).Get<JobSettings>()
                .CreateOrderCommandSubmittedJob;

            configure
                .AddJob<CreateOrderCommandSubmittedJob>(new JobKey(mergedSubmittedJob.JobName))
                .AddTrigger(
                    trigger => trigger
                        .ForJob(mergedSubmittedJob.JobName)
                        .WithCronSchedule(mergedSubmittedJob.CronExpression)
                        .WithIdentity(mergedSubmittedJob.TriggerName, mergedSubmittedJob.TriggerGroup));

            configure.UseMicrosoftDependencyInjectionJobFactory();
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }

    private static void AddMediatRConfiguration(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
    }

    private static void AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Description = "PosTech MyFood API - Orders",
                Version = "v1",
                Title = "PosTech MyFood API - Orders"
            });
        });
    }

    private static void AddOpenTelemetryConfiguration(this IServiceCollection services)
    {
        var serviceName = $"{Assembly.GetName().Name}-{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}";
        services.AddOpenTelemetry()
            .ConfigureResource(resourceBuilder => resourceBuilder.AddService(serviceName!))
            .WithTracing(tracing =>
            {
                tracing.AddSource(serviceName!);
                tracing.ConfigureResource(resource => resource
                    .AddService(serviceName)
                    .AddTelemetrySdk());
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddEntityFrameworkCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();

                tracing.AddOtlpExporter();
            });
    }

    public static void AddSerilogConfiguration(this IServiceCollection services,
        WebApplicationBuilder builder, IConfiguration configuration)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var applicationName =
            $"{Assembly.GetName().Name?.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}";

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", applicationName)
            .Enrich.WithCorrelationId()
            .Enrich.WithExceptionDetails()
            .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles"))
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Host.UseSerilog(Log.Logger, true);
    }
}
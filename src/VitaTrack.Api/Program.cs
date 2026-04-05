using VitaTrack.Application;
using VitaTrack.Infrastructure;
using VitaTrack.Infrastructure.Persistence;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.UseUrls("http://0.0.0.0:5177");

// JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "supersecretkey1234567891abcdef");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

// Global Exception Handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        if (exception is ValidationException validationException)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            await context.Response.WriteAsJsonAsync(new { errors });
        }
        else if (exception is KeyNotFoundException)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { error = exception.Message });
        }
        else
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = exception?.Message ?? "An error occurred" });
        }
    });
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseHangfireDashboard();
RecurringJob.AddOrUpdate<VitaTrack.Infrastructure.Jobs.DailyValuesJob>(
    "calculate-bmr",
    job => job.UpdateBmrAsync(CancellationToken.None),
    Cron.Daily);

RecurringJob.AddOrUpdate<VitaTrack.Infrastructure.Jobs.JobsService>(
    "daily-reminders",
    job => job.SendDailyRemindersAsync(CancellationToken.None),
    Cron.Daily);

RecurringJob.AddOrUpdate<VitaTrack.Infrastructure.Jobs.JobsService>(
    "weekly-summaries",
    job => job.SendWeeklySummariesAsync(CancellationToken.None),
    Cron.Weekly);

await app.InitialiseDatabaseAsync();

app.Run();

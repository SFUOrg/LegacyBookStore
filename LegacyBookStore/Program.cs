
using LegacyBookStore.Data;
using LegacyBookStore.Interfaces;
using LegacyBookStore.Repository;
using LegacyBookStore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("BooksRateLimit", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.Headers["Retry-After"] = "1";
        await context.HttpContext.Response.WriteAsync(
            "{\"error\": \"Много запросов\"}",
            cancellationToken: token
        );
    };
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IBooksRepository, BooksRepository>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
        { Title = "LegacyBookStore", Version = "v1" }
    );
});

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "LegacyBookStore");
});

app.UseRateLimiter();

app.Use(async (context, next) =>
{
    var userIdHeader = context.Request.Headers["X-User-Id"].ToString();
    if (!string.IsNullOrEmpty(userIdHeader) && int.TryParse(userIdHeader, out var userId))
    {
        // Нет проверки существования пользователя!
        context.Items["UserId"] = userId;
    }
    await next();
});

app.MapControllers();

app.Run();

public partial class Program { } //make program visible to Integration tests


using LegacyBookStore.Data;
using LegacyBookStore.Interfaces;
using LegacyBookStore.Services;
using LegacyBookStore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

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


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IBooksRepository, BooksRepository>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddControllers();
builder.Services.AddRazorPages();

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
app.MapRazorPages(); 

app.Run();

public partial class Program { } //make program visible to Integration tests

// Program.cs
using GitHub.Services;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// הוספת שירותי הקונטרולרים
builder.Services.AddControllers();
// הוספת In-Memory Caching
builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// הגדרת GitHub Options
builder.Services.Configure<GitHubOptions>(
    builder.Configuration.GetSection(GitHubOptions.GitHub));

// הזרקת IGitHubService
builder.Services.AddScoped<IGitHubService, GitHubService>();

var app = builder.Build();

// ... הגדרות Pipeline



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

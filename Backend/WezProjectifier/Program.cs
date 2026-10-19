using Microsoft.EntityFrameworkCore;
using WezProjectifier.Models;
using WezProjectifierBLL.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<WezDbContext>(
    options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"])
    );
builder.Services.AddScoped<clsUserService>();
builder.Services.AddScoped<clsProjectService>();
builder.Services.AddScoped<clsTaskService>();
builder.Services.AddScoped<clsMemberService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

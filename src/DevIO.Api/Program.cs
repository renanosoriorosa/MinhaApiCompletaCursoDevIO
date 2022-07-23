using DevIO.Api.Configuration;
using DevIO.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<MeuDbContext>( options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddApiConfig();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ResolveDependencies();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true; 
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseApiConfig(app.Environment);

app.MapControllers();

app.UseCors(
    options => options.WithOrigins("*")
    .AllowAnyMethod()
    .AllowAnyHeader()
);

app.Run();

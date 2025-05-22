using HBYSClientApi.Data;
using HBYSClientApi.Interfaces;
using HBYSClientApi.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HBYSClientApi", Version = "v1" });
});

builder.Services.AddSingleton<IDbContext, DbContext>();
builder.Services.AddScoped<IFaturaService, FaturaService>();

var app = builder.Build();
app.UseCors("AllowAllOrigins");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HBYSClientApi v1");
    });
}
app.MapControllers();
app.UseHttpsRedirection();

app.Run();
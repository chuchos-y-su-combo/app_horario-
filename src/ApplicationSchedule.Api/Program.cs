using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Application.Services;
using ApplicationSchedule.Infrastructure.Services;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

builder.Services.AddScoped<IAsignaturaService, AsignaturaService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<IAsignacionService, AsignacionService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendLocal", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    
    context.Database.EnsureCreated();
    
    // context.Database.Migrate();
}

app.UseSwagger(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
});

app.UseSwaggerUI();

app.UseCors("FrontendLocal");

app.UseAuthorization();

app.MapControllers();

app.Run();
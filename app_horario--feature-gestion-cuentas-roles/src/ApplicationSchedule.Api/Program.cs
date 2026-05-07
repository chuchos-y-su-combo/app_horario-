using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Application.Services;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ✅ Configurar DbContext
// - Si hay connection string válida: Usar MySQL
// - Si no hay connection string o falla: Registrar sin provider (el Factory configura con InMemory)
if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            );
        });
    }
    catch
    {
        // Si MySQL no está disponible, registrar sin provider para testing
        builder.Services.AddDbContext<AppDbContext>(options => { });
    }
}
else
{
    // Para testing sin connection string: Registrar DbContext sin proveedor
    builder.Services.AddDbContext<AppDbContext>(options => { });
}

builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddControllers();

// ✅ Agregar Swagger solo si tenemos connection string válida y NO estamos en el entorno de pruebas
if (!string.IsNullOrEmpty(connectionString) && !builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

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

// ✅ Swagger solo si tenemos connection string válida y NO estamos en el entorno de pruebas
if (!string.IsNullOrEmpty(connectionString) && !app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendLocal");

app.UseAuthorization();

app.MapControllers();

app.Run();
using ApplicationSchedule.Application.Interfaces;
using ApplicationSchedule.Application.Services;
using ApplicationSchedule.Infrastructure.Data;
using ApplicationSchedule.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(connectionString);
});
var jwtSettings = builder.Configuration.GetSection("Jwt");

var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("Falta la SecretKey del Jwt");
var issuer = jwtSettings["Issuer"] ?? "ApplicationSchedule";
var audience = jwtSettings["Audience"] ?? "ApplicationScheduleClients";

var keyBytes = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

        ValidateIssuer = true,
        ValidIssuer = issuer,

        ValidateAudience = true,
        ValidAudience = audience,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});


builder.Services.AddScoped<IAsignaturaService, AsignaturaService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IProfesorService, ProfesorService>();
builder.Services.AddScoped<IAsignacionService, AsignacionService>();
builder.Services.AddScoped<ICurriculoDocenteService, CurriculoDocenteService>();
builder.Services.AddScoped<IGeneradorHorarioService, GeneradorHorarioService>();
builder.Services.AddScoped<ICalendarioSemanalService, CalendarioSemanalService>();

builder.Services.AddScoped<IHorarioExportService, HorarioExportService>();

builder.Services.AddScoped<IReporteCargaService, ReporteCargaService>();
builder.Services.AddScoped<IConflictoAsignacionService, ConflictoAsignacionService>();
builder.Services.AddScoped<IBloqueoFranjaAsignaturaService, BloqueoFranjaAsignaturaService>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ApplicationSchedule.Api",
        Version = "v1",
        Description = "API para la gestión de horarios académicos, usuarios, profesores y asignaturas."
    });
});

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

    // Compatibilidad para bases SQLite ya existentes:
    // EnsureCreated() crea la base si no existe, pero NO modifica tablas
    // si la base ya estaba creada. Por eso garantizamos aquí la tabla Req 36.
    if (context.Database.IsRelational())
    {
        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS bloqueos_franja_asignatura (
                id_bloqueo TEXT NOT NULL PRIMARY KEY,
                id_asignatura TEXT NOT NULL,
                periodo TEXT NOT NULL,
                dia INTEGER NOT NULL CHECK (dia BETWEEN 1 AND 6),
                hora_inicio TEXT NOT NULL,
                hora_fin TEXT NOT NULL,
                motivo TEXT NULL,
                fecha_creacion_utc TEXT NOT NULL,
                FOREIGN KEY (id_asignatura) REFERENCES asignaturas(id_asignatura) ON DELETE CASCADE
            );
        """);

        context.Database.ExecuteSqlRaw("""
            CREATE UNIQUE INDEX IF NOT EXISTS IX_bloqueos_franja_asignatura_unico
            ON bloqueos_franja_asignatura (id_asignatura, periodo, dia, hora_inicio, hora_fin);
        """);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ApplicationSchedule.Api v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("FrontendLocal");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
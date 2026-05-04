using Microsoft.EntityFrameworkCore;
using SmartDashboardAPI.Data;
using SmartDashboardAPI.Interface;
using SmartDashboardAPI.Repository;
using SmartDashboardAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Connection String ────────────────────────────────────────
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ── EF Core with Npgsql ──────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connStr));

// ── Dependency Injection ─────────────────────────────────────
builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();
builder.Services.AddScoped<IQueryService, QueryService>();
builder.Services.AddScoped<IDatasetService, DatasetService>();
builder.Services.AddScoped<IChartBuilderService, ChartBuilderService>();
builder.Services.AddScoped<IKpiService, KpiService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPresetService, PresetService>();

// ── CORS — allow Angular dev server ─────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});
//builder.Services.AddControllers();
// ── Controllers + JSON ───────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.SnakeCaseLower;   // snake_case to match Angular
        opts.JsonSerializerOptions.WriteIndented = true;
    });


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();

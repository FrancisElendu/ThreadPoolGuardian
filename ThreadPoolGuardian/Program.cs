using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.EntityFrameworkCore;
using ThreadPoolGuardian.Data;
using ThreadPoolGuardian.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register storage for DatabaseMetricsStorage
//builder.Services.AddScoped<IThreadPoolMetricsStorage, DatabaseMetricsStorage>();
builder.Services.AddSingleton<IThreadPoolMetricsStorage, DatabaseMetricsStorage>();

#region Optional: Add Application Insights & Composite Storage ie using both Database and App Insights - Composite pattern
//// Add Application Insights
//builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
//{
//    ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"]
//});

//// Register storage for AppInsightsMetricsStorage
//builder.Services.AddSingleton<IThreadPoolMetricsStorage, AppInsightsMetricsStorage>();

////add composite storage to send metrics to both database and App Insights  - Composite pattern
//builder.Services.AddScoped<IThreadPoolMetricsStorage>(sp =>
//{
//    var dbStorage = sp.GetRequiredService<DatabaseMetricsStorage>();
//    var aiStorage = sp.GetRequiredService<AppInsightsMetricsStorage>();
//    return new CompositeStorage(dbStorage, aiStorage);
//});
#endregion

// Register monitor service for background monitoring of ThreadPool metrics
builder.Services.AddHostedService<ThreadPoolMonitorService>();

var app = builder.Build();

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

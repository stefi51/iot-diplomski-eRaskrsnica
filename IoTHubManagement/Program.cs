using System.Text.Json.Serialization;
using IoTHubManagement.DTOs;
using IoTHubManagement.Services;
using IoTHubManagement.Settings;
using Microsoft.AspNetCore.Mvc;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<MvcJsonOptions>(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));


/*builder.Services.AddCors(
    options =>
    {
        options.AddDefaultPolicy( policy => {
                policy.WithOrigins("http://localhost:3000"
                   ).AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });
*/
builder.Services.AddCors();
builder.Logging.AddConsole();

builder.Services.AddOptions<IoTHubSettingsDto>()
    .BindConfiguration(IoTHubSettingsDto.SectionName);

builder.Services.AddOptions<CosmosDbSettings>()
    .BindConfiguration(CosmosDbSettings.SectionName);

builder.Services.AddSingleton<IIoTHubManager, IoTHubManager>();

var app = builder.Build();

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPut("devices/{deviceId}/turn-on",
    async (string deviceId, IIoTHubManager hubManager) => { await hubManager.TurnOnAsync(deviceId); });

app.MapPut("devices/{deviceId}/turn-off",
    async (string deviceId, IIoTHubManager hubManager) => { await hubManager.TurnOffAsync(deviceId); });

app.MapPost("devices/{deviceId}/messages",
    async (string deviceId, [FromBody] MessagePayloadDto payload, IIoTHubManager hubManager) =>
    {
        await hubManager.SendMessageToDevice(deviceId, payload);
    });

app.MapPut("devices/{deviceId}/telemetry-interval/{payload}",
    async (string deviceId,  int payload, IIoTHubManager hubManager) =>
    {
        await hubManager.UpdateDeviceTelemetryInterval(deviceId, payload);
    });

app.MapGet("devices", async (IIoTHubManager hubManager) => await hubManager.GetDevicesStatus());

app.MapGet("devices/{deviceId}/device-data", async (string deviceId,IIoTHubManager hubManager) => await hubManager.GetDeviceData(deviceId));

app.MapGet("devices/device-data", async ( IIoTHubManager hubManager) => await hubManager.GetAllData());

app.MapPut("devices/{deviceId}/number-lanes/{payload}",
    async (string deviceId, int payload, IIoTHubManager hubManager) =>
    {
        await hubManager.ChangeLaneConfiguration(deviceId, payload);
    });

app.MapGet("devices/{deviceId}/refined-data", async (string deviceId, IIoTHubManager hubManager) => await hubManager.GetRefinedData(deviceId));

app.MapPut("devices/{deviceId}/resolve-accident",
    async (string deviceId, IIoTHubManager hubManager) => { await hubManager.ResolveAccident(deviceId); });

app.MapGet("devices/{deviceId}/air-quality", async (string deviceId, IIoTHubManager hubManager) => await hubManager.GetAirQuality(deviceId));

app.Run();
using IoTHubManagement.Services;
using IoTHubManagement.Settings;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddConsole();

builder.Services.AddOptions<IoTHubSettingsDto>()
    .BindConfiguration(IoTHubSettingsDto.SectionName);

builder.Services.AddOptions<CosmosDbSettings>()
    .BindConfiguration(CosmosDbSettings.SectionName);

builder.Services.AddSingleton<IIoTHubManager, IoTHubManager>();

var app = builder.Build();

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
    async (string deviceId, [FromBody] string payload, IIoTHubManager hubManager) =>
    {
        await hubManager.SendMessageToDevice(deviceId, payload);
    });


app.MapPut("devices/{deviceId}/telemetry-interval",
    async (string deviceId, [FromBody] int payload, IIoTHubManager hubManager) =>
    {
        await hubManager.UpdateDeviceTelemetryInterval(deviceId, payload);
    });

app.MapGet("devices", async (IIoTHubManager hubManager) => await hubManager.GetDevicesStatus());

app.MapGet("devices/{deviceId}/device-data", async (string deviceId,IIoTHubManager hubManager) => await hubManager.GetDeviceData(deviceId));

app.Run();
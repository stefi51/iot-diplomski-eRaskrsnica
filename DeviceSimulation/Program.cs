using DeviceSimulation.DTOs;
using DeviceSimulation.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<DeviceOneSettingsDto>()
    .BindConfiguration(DeviceOneSettingsDto.SectionName);

builder.Services.AddOptions<DeviceTwoSettingsDto>()
    .BindConfiguration(DeviceTwoSettingsDto.SectionName);

builder.Services.AddOptions<DeviceThirdSettingsDto>()
    .BindConfiguration(DeviceThirdSettingsDto.SectionName);

builder.Services.AddOptions<Device4ThSettingsDto>()
    .BindConfiguration(Device4ThSettingsDto.SectionName);

builder.Services.AddSingleton<IDeviceSimulation, DeviceSimulation.Services.DeviceSimulation>();
builder.Logging.AddConsole();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPut("/devices/{deviceId}/turn-on", async (string deviceId, IDeviceSimulation deviceSimulation) =>
{
    await deviceSimulation.StartDeviceAsync(deviceId);
});

app.MapPut("/devices/{deviceId}/turn-off", async (string deviceId, IDeviceSimulation deviceSimulation) =>
{
    await deviceSimulation.StopDeviceAsync(deviceId);
});

app.MapGet("/devices", async (IDeviceSimulation deviceSimulation) => await deviceSimulation.GetDevicesAsync());

app.MapPut("/devices/{deviceId}/report-accident", async (string deviceId, IDeviceSimulation deviceSimulation) =>
{
    await deviceSimulation.ReportCarAccident(deviceId);
});

app.Run();

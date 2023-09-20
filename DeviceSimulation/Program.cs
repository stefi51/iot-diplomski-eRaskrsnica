using DeviceSimulation.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<DeviceSimulation.Devices.DeviceOneSettingsDto>()
    .BindConfiguration(DeviceSimulation.Devices.DeviceOneSettingsDto.DeviceId);

builder.Services.AddOptions<DeviceSimulation.Devices.DeviceTwoSettingsDto>()
    .BindConfiguration(DeviceSimulation.Devices.DeviceTwoSettingsDto.DeviceId);

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

app.MapGet("/devices", async (IDeviceSimulation deviceSimulation) =>
{
    return await deviceSimulation.GetDevicesAsync();
});

app.Run();

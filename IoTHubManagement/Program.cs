using IoTHubManagement.Services;
using IoTHubManagement.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddConsole();

builder.Services.AddOptions<IoTHubSettingsDto>()
    .BindConfiguration(IoTHubSettingsDto.SectionName);

builder.Services.AddSingleton<IIoTHubManager, IoTHubManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPut("devices/{deviceId}/turn-on", async (string deviceId, IIoTHubManager hubManager) =>
{
    await hubManager.TurnOnAsync(deviceId);
});

app.MapPut("devices/{deviceId}/turn-off", async (string deviceId, IIoTHubManager hubManager) =>
{
    await hubManager.TurnOffAsync(deviceId);
});

app.Run();
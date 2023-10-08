using DeviceSimulation.DTOs;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;

namespace DeviceSimulation.Devices;

public abstract class Device
{
    public string DeviceId { get; init; }
    public bool IsActive { get; set; }
    protected DeviceClient DeviceClient { get; init; }

    protected Device(IOptions<BaseSettingsDto> options)
    {
        DeviceId = options.Value.DeviceId;
        DeviceClient = DeviceClient.CreateFromConnectionString(options.Value.ConnectionString, options.Value.TransportType);
    }

    public abstract void StartDeviceAsync();

    public abstract void StopDeviceAsync();

    public abstract void ReportCarAccident();
}
using DeviceSimulation.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace DeviceSimulation.DTOs;

public abstract class Device
{
    public string DeviceId { get; set; }
    public bool IsActive { get; set; }
    protected DeviceClient DeviceClient { get; init; }

    public Device(IOptions<BaseSettingsDto> options)
    {     
        DeviceClient = DeviceClient.CreateFromConnectionString(options.Value.ConnectionString, options.Value.TransportType);
        
        StartDeviceAsync();
    }

    public abstract void  StartDeviceAsync();

    public abstract void StopDeviceAsync();

}

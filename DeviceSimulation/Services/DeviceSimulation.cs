using DeviceSimulation.Devices;
using DeviceSimulation.DTOs;
using Microsoft.Extensions.Options;

namespace DeviceSimulation.Services;

public class DeviceSimulation : IDeviceSimulation
{
    private readonly List<Device> devices;

    private readonly ILogger _logger;
    // todo refactor
    public DeviceSimulation(ILogger<DeviceSimulation> logger, IOptions<DeviceOneSettingsDto> device1, IOptions<DeviceTwoSettingsDto> device2)
    {
        _logger = logger;

        devices = new List<Device>()
        {
            new Camera(device1, logger)
        };
    }

    public async Task<List<Device>> GetDevicesAsync()
    {
        return devices;
    }

    public async Task StartDeviceAsync(string deviceId)
    {
        var device = (from d in devices
                     where d.DeviceId == deviceId
                     select d).SingleOrDefault();

        if(device is null)
        {
            throw new Exception($"Device not found.{deviceId}");
        }

        device.IsActive = true;    
        device.StartDeviceAsync();

        _logger.LogInformation($"Device: {deviceId} started.");      
    }

    public async Task StopDeviceAsync(string deviceId)
    {
        var device = (from d in devices
                      where d.DeviceId == deviceId
                      select d).SingleOrDefault();

        if (device is null)
        {
            throw new Exception($"Device not found.{deviceId}");
        }

       // device.IsActive = false;
       device.StopDeviceAsync();
        _logger.LogInformation($"Device: {deviceId} stopped.");
    }
}

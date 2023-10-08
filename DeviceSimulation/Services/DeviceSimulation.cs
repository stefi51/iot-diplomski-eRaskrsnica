using DeviceSimulation.Devices;
using DeviceSimulation.DTOs;
using Microsoft.Extensions.Options;

namespace DeviceSimulation.Services;

public class DeviceSimulation : IDeviceSimulation
{
    private readonly List<Device> _devices;

    private readonly ILogger _logger;

    public DeviceSimulation(ILogger<DeviceSimulation> logger,
        IOptions<DeviceOneSettingsDto> deviceSettings1,
        IOptions<DeviceTwoSettingsDto> deviceSettings2)
    {
        _logger = logger;

        _devices = new List<Device>
        {
            new IntersectionDevice(deviceSettings1, logger),
            new IntersectionDevice(deviceSettings2, logger)
        };
    }

    public async Task<List<Device>> GetDevicesAsync()
    {
        return _devices;
    }

    public Task ReportCarAccident(string deviceId)
    {
        var device = (from d in _devices
            where d.DeviceId == deviceId
            select d).SingleOrDefault();

        if (device is null)
        {
            throw new Exception($"Device not found.{deviceId}");
        }
        _logger.LogInformation($"Device: {deviceId} ReportedCarAccident:{DateTime.UtcNow}");
        device.ReportCarAccident();
        return Task.CompletedTask;
    }

    public async Task StartDeviceAsync(string deviceId)
    {
        var device = (from d in _devices
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
        var device = (from d in _devices
                      where d.DeviceId == deviceId
                      select d).SingleOrDefault();

        if (device is null)
        {
            throw new Exception($"Device not found.{deviceId}");
        }

        device.StopDeviceAsync();
        _logger.LogInformation($"Device: {deviceId} stopped.");
    }
}

using DeviceSimulation.Devices;

namespace DeviceSimulation.Services;

public interface IDeviceSimulation
{
    public Task StartDeviceAsync(string deviceId);
    public Task StopDeviceAsync(string deviceId);

    public Task<List<Device>> GetDevicesAsync();
}

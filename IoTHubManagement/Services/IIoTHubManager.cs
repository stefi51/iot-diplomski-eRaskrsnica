using IoTHubManagement.DTOs;

namespace IoTHubManagement.Services;

public interface IIoTHubManager
{
    public Task TurnOnAsync(string deviceId);

    public Task TurnOffAsync(string deviceId);

    public Task SendMessageToDevice(string deviceId, string payload);
    public Task UpdateDeviceTelemetryInterval(string deviceId, int value);

    public Task<List<DeviceStatusDto>> GetDevicesStatus();

    public Task<List<DeviceDataDto>> GetDeviceData(string deviceId);
}
using IoTHubManagement.DTOs;

namespace IoTHubManagement.Services;

public interface IIoTHubManager
{
    public Task TurnOnAsync(string deviceId);

    public Task TurnOffAsync(string deviceId);

    public Task SendMessageToDevice(string deviceId, MessagePayloadDto payload);
    public Task UpdateDeviceTelemetryInterval(string deviceId, int value);

    public Task<List<DeviceStatusDto>> GetDevicesStatus();

    public Task<List<DeviceDataDto>> GetDeviceData(string deviceId);

    public Task<List<DeviceDataDto>> GetAllData();

    public Task ChangeLaneConfiguration(string deviceId, int value);

    public Task<List<RefinedDataDTO>> GetRefinedData(string deviceId);
    public Task ResolveAccident(string deviceId);
    public Task<AirQualityDto> GetAirQuality(string deviceId);
}
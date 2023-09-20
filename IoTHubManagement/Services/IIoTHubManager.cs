namespace IoTHubManagement.Services;

public interface IIoTHubManager
{
    public Task TurnOnAsync(string deviceId);

    public Task TurnOffAsync(string deviceId);
}

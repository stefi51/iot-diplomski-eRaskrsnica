using System.Text;
using IoTHubManagement.DTOs;
using IoTHubManagement.Settings;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Options;

namespace IoTHubManagement.Services;

public class IoTHubManager : IIoTHubManager
{
    private readonly ILogger _logger;

    private readonly ServiceClient _serviceClient;

    private readonly RegistryManager _registryManager;

    public IoTHubManager(ILogger<IoTHubManager> logger, IOptions<IoTHubSettingsDto> settings)
    {
        _logger = logger;
        _serviceClient = ServiceClient.CreateFromConnectionString(settings.Value.ConnectionString);
        _registryManager = RegistryManager.CreateFromConnectionString(settings.Value.ConnectionString);
    }

    public async Task TurnOffAsync(string deviceId)
    {
        var methodInvocation = new CloudToDeviceMethod("StopDevice")
        {
            ResponseTimeout = TimeSpan.FromSeconds(30)
        };
        methodInvocation.SetPayloadJson("10");

        var response = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);

        _logger.LogInformation($"Response status: {response.Status}, payload:\n\t{response.GetPayloadAsJson()}");
        _logger.LogInformation($"{deviceId} stopped.");
    }

    public async Task SendMessageToDevice(string deviceId, string payload)
    {
        using var message = new Message(Encoding.ASCII.GetBytes(payload))
        {
            // An acknowledgment is sent on delivery success or failure.
            Ack = DeliveryAcknowledgement.Full
        };
        try
        {
            await _serviceClient.SendAsync(deviceId, message);
            _logger.LogInformation($"Sent payload to {deviceId}.");
            message.Dispose();
        }
        catch (Exception e)
        {
            _logger.LogError($"Unexpected error, will need to reinitialize the client: {e}");
        }
    }

    public async Task TurnOnAsync(string deviceId)
    {
        var methodInvocation = new CloudToDeviceMethod("StartDevice")
        {
            ResponseTimeout = TimeSpan.FromSeconds(30)
        };
        methodInvocation.SetPayloadJson("10");

        var response = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);

        _logger.LogInformation($"Response status: {response.Status}, payload:\n\t{response.GetPayloadAsJson()}");
        _logger.LogInformation($"{deviceId} started.");
    }

    public async Task UpdateDeviceTelemetryInterval(string deviceId, int value)
    {
        var deviceTwin = await _registryManager.GetTwinAsync(deviceId);

        deviceTwin.Properties.Desired["telemetryInterval"] = value;

        await _registryManager.UpdateTwinAsync(deviceId, deviceTwin, deviceTwin.ETag);
    }

    public async Task<List<DeviceStatusDto>> GetDevicesStatus()
    {
        var list= new List<DeviceStatusDto>();
        var query = _registryManager.CreateQuery("SELECT * FROM devices WHERE devices.connectionState= 'Connected'", 100);
        while (query.HasMoreResults)
        {
            var page = await query.GetNextAsTwinAsync();
            foreach (var twin in page)
            {
                list.Add(new()
                {
                    DeviceId = twin.DeviceId,
                    IsActive = twin.Properties.Reported["IsActive"],
                    TelemetryInterval = twin.Properties.Reported["telemetryInterval"]
                });

            }
        }
        return list;
    }
}
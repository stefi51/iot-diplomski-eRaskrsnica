using IoTHubManagement.Settings;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Options;
using Microsoft.Rest;

namespace IoTHubManagement.Services;

public class IoTHubManager : IIoTHubManager
{
    private readonly ILogger _logger;

    private readonly ServiceClient _serviceClient;

    public IoTHubManager(ILogger<IoTHubManager> logger, IOptions<IoTHubSettingsDto> settings)
    {
        _logger = logger;
        _serviceClient= ServiceClient.CreateFromConnectionString(settings.Value.ConnectionString);

    }
    public async Task TurnOffAsync(string deviceId)
    {
        var methodInvocation = new CloudToDeviceMethod("StopDevice")
        {
            ResponseTimeout = TimeSpan.FromSeconds(30),
        };
        methodInvocation.SetPayloadJson("10");

        CloudToDeviceMethodResult response = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);

        _logger.LogInformation($"Response status: {response.Status}, payload:\n\t{response.GetPayloadAsJson()}");
        _logger.LogInformation($"{deviceId} stopped.");
    }

    public async Task TurnOnAsync(string deviceId)
    {
        var methodInvocation = new CloudToDeviceMethod("StartDevice")
        {
            ResponseTimeout = TimeSpan.FromSeconds(30),
        };
        methodInvocation.SetPayloadJson("10");

        CloudToDeviceMethodResult response = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);

        _logger.LogInformation($"Response status: {response.Status}, payload:\n\t{response.GetPayloadAsJson()}");
        _logger.LogInformation($"{deviceId} started.");
    }
}

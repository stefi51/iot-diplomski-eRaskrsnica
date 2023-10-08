using System.Text;
using IoTHubManagement.DTOs;
using IoTHubManagement.Settings;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Options;

namespace IoTHubManagement.Services;

public class IoTHubManager : IIoTHubManager
{
    private readonly ILogger _logger;

    private readonly ServiceClient _serviceClient;

    private readonly RegistryManager _registryManager;

    private readonly CosmosClient _cosmosClient;

    public IoTHubManager(ILogger<IoTHubManager> logger, IOptions<IoTHubSettingsDto> settings, IOptions<CosmosDbSettings> cosmosDbo)
    {
        _logger = logger;
        _serviceClient = ServiceClient.CreateFromConnectionString(settings.Value.ConnectionString);
        _registryManager = RegistryManager.CreateFromConnectionString(settings.Value.ConnectionString);
        _cosmosClient = new CosmosClient(cosmosDbo.Value.ConnectionString);
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

    public async Task SendMessageToDevice(string deviceId, MessagePayloadDto reqDto)
    {
        using var message = new Message(Encoding.ASCII.GetBytes(reqDto.Payload))
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
                    TelemetryInterval = twin.Properties.Reported["telemetryInterval"],
                    NumberOfLanes = twin.Properties.Reported["NumberOfLanes"]

                });

            }
        }
        return list;
    }

    public async Task<List<DeviceDataDto>> GetDeviceData(string deviceId)
    {
        var db = _cosmosClient.GetDatabase("device-data");

        var container = db.GetContainer("DeviceData");

        return container.GetItemLinqQueryable<DeviceDataDto>(true)
            .Where(d => d.DeviceId== deviceId)
            .OrderByDescending(x=> x.Body.TimeStamp)
            .ToList();
        
    }

    public async Task<List<DeviceDataDto>> GetAllData()
    {
        var db = _cosmosClient.GetDatabase("device-data");

        var container = db.GetContainer("DeviceData");

        return container.GetItemLinqQueryable<DeviceDataDto>(true)
            .OrderByDescending(x=> x.Body.TimeStamp).ToList();
    }

    public async Task ChangeLaneConfiguration(string deviceId, int value)
    {
        var deviceTwin = await _registryManager.GetTwinAsync(deviceId);

        deviceTwin.Properties.Desired["NumberOfLanes"] = value;

        await _registryManager.UpdateTwinAsync(deviceId, deviceTwin, deviceTwin.ETag);
    }

    public async Task<List<RefinedDataDTO>> GetRefinedData(string deviceId)
    {
        var db = _cosmosClient.GetDatabase("device-data");

        var container = db.GetContainer("refined-data");

        return container.GetItemLinqQueryable<RefinedDataDTO>(true)
            .Where(d => d.DeviceId == deviceId)
            .OrderByDescending(x => x.RefinedDate)
            .ToList();
      
    }

    public async Task ResolveAccident(string deviceId)
    {
        var methodInvocation = new CloudToDeviceMethod("ResolveAccident")
        {
            ResponseTimeout = TimeSpan.FromSeconds(30)
        };
        methodInvocation.SetPayloadJson("false");

        var response = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);

        _logger.LogInformation($"Response status: {response.Status}, payload:\n\t{response.GetPayloadAsJson()}");
        _logger.LogInformation($"{deviceId} Accident resolved.");
    }

    public async Task<AirQualityDto > GetAirQuality(string deviceId)
    {

        var db = _cosmosClient.GetDatabase("device-data");

        var container = db.GetContainer("refined-data");

       var data=container.GetItemLinqQueryable<RefinedDataDTO>(true)
            .Where(d => d.DeviceId == deviceId)
            .OrderByDescending(x => x.RefinedDate)
            .ToList();

       var last = data.FirstOrDefault();

       return new AirQualityDto(deviceId, last?.AirQuality);

    }
}
using DeviceSimulation.DTOs;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace DeviceSimulation.Devices;

public class IntersectionDevice : Device
{
    CancellationTokenSource _cancelTokenSource;
    CancellationToken _token;
    private readonly ILogger _logger;
    private TimeSpan TelemetryInterval { get; set; }

    private DeviceSensorDataDto _sensorData;

    public IntersectionDevice(IOptions<BaseSettingsDto> options,
        ILogger logger) : base(options)
    {

        // Invoke direct method on device.
        DeviceClient.SetMethodHandlerAsync("ResolveAccident", ResolveCarAccident, null);
        
        DeviceClient.SetMethodHandlerAsync("StopDevice", StopDeviceFromCloud, null);

        DeviceClient.SetMethodHandlerAsync("StartDevice", StartDeviceFromCloud, null);

        DeviceClient.SetDesiredPropertyUpdateCallbackAsync(TwinUpdateCallback, null);

        DeviceClient.SetReceiveMessageHandlerAsync(ReceiveC2DMessage, null);

        _logger = logger;

        StartDeviceAsync();

    }

    // Async method to send simulated telemetry.
    // Device to cloud messages
    public async Task SimulateDeviceSensors()
    {

        try
        {
            while (IsActive)
            {
                if (_token.IsCancellationRequested)
                    _token.ThrowIfCancellationRequested();

                //initial data
                double minTemperature = 20;
                var rand = new Random();
                var currentTemp = minTemperature + rand.NextDouble() * 15;
                var averageSpeed = (rand.NextDouble() * 100) / _sensorData.NumberOfLanes;
                var vehiclePerHour = (rand.NextDouble() * 1000) * _sensorData.NumberOfLanes;

                _sensorData.Temperature = currentTemp;
                _sensorData.Humidity = 60 + rand.NextDouble() * 20;
                _sensorData.AirQualityIndex = (int)((_sensorData.NumberOfLanes * currentTemp) / (rand.NextDouble() * 2));
                _sensorData.AverageSpeedPerLane = averageSpeed;
                _sensorData.VehiclePerHour = (int)vehiclePerHour;
                _sensorData.TimeStamp = DateTime.Now;

                // Create JSON message.
                string messageBody = JsonSerializer.Serialize(_sensorData);

                using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                };

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
               // message.Properties.Add("IsTrafficJam", _sensorData.IsTrafficJamActive ? "true" : "false");

                // Send the telemetry message.
                await DeviceClient.SendEventAsync(message);

                Console.WriteLine($"{DateTime.Now} >DeviceId:{DeviceId} Sending message: {messageBody}");

                await Task.Delay(TelemetryInterval, _token);
            }

        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("canceled1");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("canceled");
        } 
    }

    public async override void StartDeviceAsync()
    {
        await SetUpMetaData();

        //stops previous iteration
        _cancelTokenSource?.Cancel();
        _cancelTokenSource = new CancellationTokenSource();
        _token = _cancelTokenSource.Token;

        SimulateDeviceSensors();
    }

    public async override void StopDeviceAsync()
    {
        this.IsActive = false;
        var twinProperties = new TwinCollection();
        twinProperties["IsActive"] = this.IsActive;
        await DeviceClient.UpdateReportedPropertiesAsync(twinProperties);
        _cancelTokenSource?.Cancel();
    }

    /// <summary>
    /// Stops devices on cloud request.
    /// </summary>
    /// <param name="methodRequest"></param>
    /// <param name="userContext"></param>
    /// <returns></returns>
    private Task<MethodResponse> StopDeviceFromCloud(MethodRequest methodRequest, object userContext)
    {
        _logger.LogInformation($"Stop device: {DeviceId} request from cloud.");

        StopDeviceAsync();

        string result = $"{{\"result\":\"Executed direct method: {methodRequest.Name}\"}}";

        return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
    }


    /// <summary>
    /// Starts device on cloud request.
    /// </summary>
    /// <param name="methodRequest"></param>
    /// <param name="userContext"></param>
    /// <returns></returns>
    private Task<MethodResponse> StartDeviceFromCloud(MethodRequest methodRequest, object userContext)
    {
        _logger.LogInformation($"Start device: {DeviceId} request from cloud.");

        StartDeviceAsync();
        string result = $"{{\"result\":\"Executed direct method: {methodRequest.Name}\"}}";
        return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));

    }

    private Task<MethodResponse> ResolveCarAccident(MethodRequest methodRequest, object userContext)
    {
        string data = Encoding.UTF8.GetString(methodRequest.Data);

        if (bool.TryParse(data, out bool carAccident))
        {
            _sensorData.ReportedAccident = carAccident;
            string result = $"{{\"result\":\"Executed direct method: {methodRequest.Name}\"}}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }
        else
        {
            // Acknowlege the direct method call with a 400 error message.
            string result = "{\"result\":\"Invalid parameter\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
        }
    }


    private static Task<MethodResponse> SetTelemetryInterval(MethodRequest methodRequest, object userContext)
    {
        string data = Encoding.UTF8.GetString(methodRequest.Data);

        if (int.TryParse(data, out int telemetryIntervalInSeconds))
        {
            var s_telemetryInterval = TimeSpan.FromSeconds(telemetryIntervalInSeconds);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Telemetry interval set to {s_telemetryInterval}");
            Console.ResetColor();

            // Acknowlege the direct method call with a 200 success message.
            string result = $"{{\"result\":\"Executed direct method: {methodRequest.Name}\"}}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }
        else
        {
            // Acknowlege the direct method call with a 400 error message.
            string result = "{\"result\":\"Invalid parameter\"}";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
        }
    }



    // Initial device setup.
    private async Task SetUpMetaData()
    {
        Twin twin = await DeviceClient.GetTwinAsync();
        _logger.LogInformation($"\t DeviceId:{DeviceId}." +
                               $"Initial twin value received::{twin.ToJson()}");

        var isActive = twin.Properties.Desired["IsActive"];
        var telemetryInterval = twin.Properties.Desired["telemetryInterval"];
        var numberOfLanes = (int)twin.Properties.Desired["NumberOfLanes"];

        if (telemetryInterval is not null)
        {
            TelemetryInterval = TimeSpan.FromSeconds((double)telemetryInterval);
        }
        else
        {
            TelemetryInterval = TimeSpan.FromSeconds((double)10);
        }

        if (isActive is not null)
        {
            this.IsActive = isActive;
        }
        else
        {
            this.IsActive = false;
        }

        //initial data
        _sensorData = new DeviceSensorDataDto()
        {
            NumberOfLanes = numberOfLanes,
        };

        var twinProperties = new TwinCollection();
        twinProperties["IsActive"] = this.IsActive;
        twinProperties["telemetryInterval"] = telemetryInterval;
        twinProperties["NumberOfLanes"] = numberOfLanes;
        await DeviceClient.UpdateReportedPropertiesAsync(twinProperties);
    }

    //Update device data twin from cloud.
    private async Task TwinUpdateCallback(TwinCollection tw, object userContext)
    {
        _logger.LogInformation($"\t DeviceId:{DeviceId}." +
                               $"Updated twin value received:{tw.ToJson()}");


        var isActive = tw["IsActive"];
        var telemetryInterval = tw["telemetryInterval"];
        var numOfLanes = tw["NumberOfLanes"];

        if (telemetryInterval is not null)
        {
            TelemetryInterval = TimeSpan.FromSeconds((double)telemetryInterval);
        }
        else
        {
            TelemetryInterval = TimeSpan.FromSeconds((double)10);
        }

        if (isActive is not null)
        {
            this.IsActive = isActive;
        }
        else
        {
            this.IsActive = false;
        }

        if (numOfLanes is not null)
        {
            this._sensorData.NumberOfLanes = (int)numOfLanes;
        }
        else
        {
            this._sensorData.NumberOfLanes = 1;
        }

        var twinProperties = new TwinCollection();
        twinProperties["IsActive"] = this.IsActive;
        twinProperties["telemetryInterval"] = telemetryInterval;
        twinProperties["NumberOfLanes"] = this._sensorData.NumberOfLanes;
        await DeviceClient.UpdateReportedPropertiesAsync(twinProperties);

    }

    private async Task ReceiveC2DMessage(Message message, object _)
    {
        try
        {
            string messageData = Encoding.ASCII.GetString(message.GetBytes());

            var formattedMessage = new StringBuilder($"Received message: [{messageData}]\n");

            //print C2D message
            _logger.LogInformation($"DeviceID: {DeviceId}. Timestamp:{DateTime.Now} {formattedMessage}");

            // remove message from queue
            await DeviceClient.CompleteAsync(message);

        }
        finally
        {
            message.Dispose();
        }
    }

    public override void ReportCarAccident()
    {
        _sensorData.ReportedAccident = true;
    }
}

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
    CancellationToken _token ;
    private readonly ILogger _logger;


    // Traffic data
    private int  VehiclePerHour { get; set; }
    private Dictionary<int,double> AverageSpeedPerLane { get; set; }
    private double Temperature { get; set; }
    private double AirQuality { get; set; }
    private int NumberOfLanes { get; set; }
    private bool IsTrafficJamActive { get; set; }
    private TimeSpan TelemetryInterval { get; set; }


    public IntersectionDevice(IOptions<BaseSettingsDto> options,
        ILogger logger) : base(options)
    {
       
        //TODO
        DeviceClient.SetMethodHandlerAsync("DecreaseTrafficFlow", SetTelemetryInterval, null);

         // TODO
        DeviceClient.SetMethodHandlerAsync("IncreaseTrafficFlow", SetTelemetryInterval, null);

        // Invoke direct method on device.
        DeviceClient.SetMethodHandlerAsync("StopDevice", StopDeviceFromCloud, null);

        DeviceClient.SetMethodHandlerAsync("StartDevice", StartDeviceFromCloud, null);


        DeviceClient.SetDesiredPropertyUpdateCallbackAsync(TwinUpdateCallback, null);

        DeviceClient.SetReceiveMessageHandlerAsync(ReceiveC2DMessage, null);

        _logger = logger;
    }

    //TODO simulirati data

    // Async method to send simulated telemetry.
    public async Task SendDeviceToCloudMessagesAsync()
    {
        // Initial telemetry values.
        double minTemperature = 20;
        double minHumidity = 60;
        var s_telemetryInterval = TimeSpan.FromSeconds(10); // Seconds

        var rand = new Random();

        try
        {
            while (IsActive)
            {
                if (_token.IsCancellationRequested)
                    _token.ThrowIfCancellationRequested();

                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                // Create JSON message.
                string messageBody = JsonSerializer.Serialize(
                    new
                    {
                        temperature = currentTemperature,
                        humidity = currentHumidity,
                    });
                using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                };

                // Add a custom application property to the message.
                // An IoT hub can filter on these properties without access to the message body.
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                // Send the telemetry message.
                await DeviceClient.SendEventAsync(message);
                Console.WriteLine($"{DateTime.Now} > Sending message: {messageBody}");

                await Task.Delay(s_telemetryInterval, _token);
            }

        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("canceled1");
        }
        catch (TaskCanceledException) {
            Console.WriteLine("canceled");
        } // User canceled
    }

    public async override void StartDeviceAsync()
    {
        await SetUpMetaData();

        //stops previous iteration
        _cancelTokenSource?.Cancel();
        _cancelTokenSource = new CancellationTokenSource();
        _token = _cancelTokenSource.Token;

        SendDeviceToCloudMessagesAsync();
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


    // Handle the direct method call.
    private static Task<MethodResponse> SetTelemetryInterval(MethodRequest methodRequest, object userContext)
    {
        string data = Encoding.UTF8.GetString(methodRequest.Data);

        // Check the payload is a single integer value.
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

    private async Task SetUpMetaData()
    {
       
        Twin twin = await DeviceClient.GetTwinAsync();
        _logger.LogInformation("\tInitial twin value received:");
        _logger.LogInformation($"\t{twin.ToJson()}");


        var isActive = twin.Properties.Desired["IsActive"];
        var telemetryInterval = twin.Properties.Desired["telemetryInterval"];

        if (telemetryInterval is not null)
        {
            TelemetryInterval= TimeSpan.FromSeconds((double)telemetryInterval);
        }

        if (isActive is not null)
        {
            this.IsActive = isActive;
        }
        else
        {
            this.IsActive = false;
        }

        var twinProperties = new TwinCollection();
        twinProperties["IsActive"] = this.IsActive;
        twinProperties["telemetryInterval"] = telemetryInterval;
        await DeviceClient.UpdateReportedPropertiesAsync(twinProperties);
    }

    private async Task TwinUpdateCallback(TwinCollection tw, object userContext)
    {
      
        _logger.LogInformation("\t Updated twin value received:");
        _logger.LogInformation($"\t{tw.ToJson()}"); 


        var isActive = tw["IsActive"];
        var telemetryInterval = tw["telemetryInterval"];

        if (telemetryInterval is not null)
        {
            TelemetryInterval = TimeSpan.FromSeconds((double)telemetryInterval);
        }

        if (isActive is not null)
        {
            this.IsActive = isActive;
        }
        else
        {
            this.IsActive = false;
        }

        var twinProperties = new TwinCollection();
        twinProperties["IsActive"] = this.IsActive;
        twinProperties["telemetryInterval"] = telemetryInterval;
        await DeviceClient.UpdateReportedPropertiesAsync(twinProperties);
      
    }

    private async Task ReceiveC2DMessage(Message message, object _)
    {
        try
        {
            string messageData = Encoding.ASCII.GetString(message.GetBytes());
            var formattedMessage = new StringBuilder($"Received message: [{messageData}]\n");
            _logger.LogInformation($"Message received: {formattedMessage}");

            // remove message from queue
            await DeviceClient.CompleteAsync(message);

        }
        finally
        {
            message.Dispose();
        }


        throw new NotImplementedException();
    }

}

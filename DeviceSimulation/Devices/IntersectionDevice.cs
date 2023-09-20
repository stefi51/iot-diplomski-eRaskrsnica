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

    public IntersectionDevice(IOptions<BaseSettingsDto> options, ILogger logger) : base(options)
    {
        DeviceClient.SetMethodHandlerAsync("SetTelemetryInterval", SetTelemetryInterval, null);
        DeviceClient.SetMethodHandlerAsync("StopDevice", StopDeviceCloud, null);
        DeviceClient.SetMethodHandlerAsync("StartDevice", StartDevice, null);
        _logger = logger;
    }

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
        Twin twin = await DeviceClient.GetTwinAsync();
       _logger.LogInformation("\tInitial twin value received:");
        _logger.LogInformation($"\t{twin.ToJson()}");
     

        var isActive = twin.Properties.Desired["IsActive"];

        if( isActive is not null)
        {
            this.IsActive = isActive;
        }
        else
        {
            this.IsActive = false;
        }

        var twinProperties = new TwinCollection();
        twinProperties["IsActive"] = this.IsActive;
        await DeviceClient.UpdateReportedPropertiesAsync(twinProperties);
        _cancelTokenSource?.Cancel();
        _cancelTokenSource = new CancellationTokenSource();
        _token = _cancelTokenSource.Token;

        SendDeviceToCloudMessagesAsync();
    }

    public override void StopDeviceAsync()
    {
        this.IsActive = false;
        _cancelTokenSource?.Cancel();
    }

    private Task<MethodResponse> StopDeviceCloud(MethodRequest methodRequest, object userContext)
    {
        _logger.LogInformation("Stop from cloud");
        StopDeviceAsync();
        // Acknowlege the direct method call with a 200 success message.
        string result = $"{{\"result\":\"Executed direct method: {methodRequest.Name}\"}}";
        return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
     
    }


    private Task<MethodResponse> StartDevice(MethodRequest methodRequest, object userContext)
    {
        StartDeviceAsync();
        // Acknowlege the direct method call with a 200 success message.
        _logger.LogInformation("Start from cloud");
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

}

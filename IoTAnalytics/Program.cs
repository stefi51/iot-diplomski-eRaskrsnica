using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs.Consumer;
using IoTAnalytics.DTOs;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace IoTAnalytics
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            await ProcessMessagesFromDevicesAsync(cts.Token);
        }

        private static async Task ProcessMessagesFromDevicesAsync(CancellationToken ct)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true);

            var config = builder.Build();

            var iotHubConnectionString = config["IoTHub:ConnectionString"];

            await using var consumer = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, iotHubConnectionString);

            var cosmosDbConnectionString = config["CosmosDb:ConnectionString"];
            var _cosmosClient = new CosmosClient(cosmosDbConnectionString);

            var db = _cosmosClient.GetDatabase("device-data");

            var container = db.GetContainer("refined-data");
            var dateTimeNow = (DateTime.UtcNow).AddSeconds(-30);

            try
            {
                await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync(ct))
                {

                   var deviceId = partitionEvent.Data.SystemProperties.GetValueOrDefault("iothub-connection-device-id");

                    BodyDto? bodyDto = JsonSerializer.Deserialize<BodyDto?>(partitionEvent.Data.Data);

                    if (partitionEvent.Data.EnqueuedTime.UtcDateTime > dateTimeNow)
                    {
                        string data = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
                        Console.WriteLine($"Processed.DeviceId:{deviceId}.Message body: {data}");

                        var airQuality = DetermineAirQuality(bodyDto?.AirQualityIndex);

                        var isRushHour = IsRushHour(bodyDto?.TimeStamp);

                        var intersectionState = DetermineState(bodyDto, isRushHour);

                        var refinedData = new RefinedDataDto(Guid.NewGuid().ToString(), (string)deviceId,
                            intersectionState, airQuality, DateTime.UtcNow, bodyDto);

                        await container.CreateItemAsync<RefinedDataDto>(refinedData);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        private static IntersectionState DetermineState(BodyDto? bodyDto, bool isRushHour)
        {
            if (bodyDto.ReportedAccident && bodyDto.AverageSpeedPerLane < 10)
                return IntersectionState.Blocked;

            if (isRushHour)
                return IntersectionState.RushHour;

            if (bodyDto.AverageSpeedPerLane < 14 || bodyDto.ReportedAccident)
                return IntersectionState.TrafficJam;

            return IntersectionState.Recommended;
        }

        private static bool IsRushHour(DateTime? time)
        {
            if (time?.Hour is >= 7 and <= 10 || time?.Hour is >= 17 and <= 20)
                return true;
            return false;
        }

       private static AirQuality DetermineAirQuality(double? index)
        {
            switch (index)
            {
                case var i when(i > 0 && i <= 50):
                    return AirQuality.Good;
                case var i when (i > 51 && i <= 100):
                    return AirQuality.Moderate;

                case var i when (i > 101 && i <= 150):
                    return AirQuality.UnhealthyForSensitiveGroups;

                case var i when (i > 151 && i <= 200):
                    return AirQuality.Unhealthy;

                case var i when (i > 201 && i <= 300):
                    return AirQuality.VeryUnhealthy;

                default:
                    return AirQuality.Hazardous;

            }
        }
    }
}


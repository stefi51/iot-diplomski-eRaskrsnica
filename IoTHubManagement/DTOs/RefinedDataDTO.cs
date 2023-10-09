namespace IoTHubManagement.DTOs;

public class RefinedDataDTO
{
    public string id { get; set; }
    public string DeviceId { get; set; }
    public IntersectionState IntersectionState { get; set; }
    public AirQuality AirQuality { get; set; }
    public DateTime RefinedDate { get; set; }
    public BodyDto rawData { get; set; }
}

public enum IntersectionState
{
    TrafficJam,
    Blocked,
    RushHour,
    Recommended
}

public enum AirQuality
{
    Good,
    Moderate,
    UnhealthyForSensitiveGroups,
    Unhealthy,
    VeryUnhealthy,
    Hazardous
}


public record AirQualityDto(
    string deviceId,
    AirQuality? airQuality,
    double Temperature,
    double AirQualityIndex,
    DateTime RefinedDate,
    DateTime RawDate
    );
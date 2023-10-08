namespace IoTAnalytics.DTOs;

public record RefinedDataDto(
    string id,
    string DeviceId,
    IntersectionState IntersectionState,
    AirQuality AirQuality,
    DateTime RefinedDate,
    BodyDto rawData
    );

public record BodyDto(
    int VehiclePerHour,
    double AverageSpeedPerLane,
    double Temperature,
    double AirQualityIndex,
    int NumberOfLanes,
    DateTime TimeStamp,
    bool ReportedAccident = false
);

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
namespace IoTHubManagement.DTOs;

public record DeviceDataDto
(
    string DeviceId,
    BodyDto Body
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
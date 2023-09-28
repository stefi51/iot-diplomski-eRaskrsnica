using System.Security.Cryptography.X509Certificates;

namespace IoTHubManagement.DTOs;

public record DeviceDataDto
(
    string DeviceId,
    BodyDto Body
);

public record BodyDto(
    double temperature,
    double humidity
    );
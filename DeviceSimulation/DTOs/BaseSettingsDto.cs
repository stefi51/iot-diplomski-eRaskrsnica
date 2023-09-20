using Microsoft.Azure.Devices.Client;

namespace DeviceSimulation.DTOs;

public abstract class BaseSettingsDto
{
    public string DeviceId { get; init; }
    public string ConnectionString { get; init; }
    public TransportType TransportType { get; init; }
}

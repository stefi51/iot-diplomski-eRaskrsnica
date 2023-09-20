using Microsoft.Azure.Devices.Client;

namespace DeviceSimulation.Devices;

public abstract class BaseSettingsDto
{
    public string ConnectionString { get; init; }
    public TransportType TransportType { get; init; }
}

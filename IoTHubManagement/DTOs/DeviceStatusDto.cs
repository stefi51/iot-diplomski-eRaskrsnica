namespace IoTHubManagement.DTOs
{
    public class DeviceStatusDto
    {
        public string DeviceId { get; set; }

        public bool IsActive { get; set; }

        public int TelemetryInterval { get; set; }

        public int NumberOfLanes { get; set; }
    }
}

namespace DeviceSimulation.DTOs
{
    public class DeviceSensorDataDto
    {
        // Traffic data
        public int VehiclePerHour { get; set; }
        public double AverageSpeedPerLane { get; set; }
        public double Temperature { get; set; }
        public double AirQualityIndex { get; set; }
        public int NumberOfLanes { get; set; }
        public bool IsTrafficJamActive { get; set; }

    }
}

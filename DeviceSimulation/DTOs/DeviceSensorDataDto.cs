namespace DeviceSimulation.DTOs
{
    public class DeviceSensorDataDto
    {
        // Traffic data
        public int VehiclePerHour { get; set; }
        public double AverageSpeedPerLane { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double AirQualityIndex { get; set; }
        public int NumberOfLanes { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool ReportedAccident { get; set; }

    }
}

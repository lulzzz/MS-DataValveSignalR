using System;
using System.Collections.Generic;

namespace FuseThru.Common
{
    public interface IDeviceData
    {
        string DeviceId { get; set; }
        StreamSensorData StreamSensorData { get; set; }
    }

    public class DeviceData : IDeviceData
    {
        public string DeviceId { get; set; }
        public StreamSensorData StreamSensorData { get; set; }
    }

    public class StreamSensorData
    {
        public string SensorName { get; set; }
        public SensorData SensorData { get; set; }
    }

    public class SensorData
    {
        public List<string> SensorReading { get; set; }
        public DateTime SensorReadingTime { get; set; }
    }
}

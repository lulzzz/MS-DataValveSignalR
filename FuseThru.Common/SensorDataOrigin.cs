using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FuseThru.Common
{
    public static class SensorDataOriginFactory
    {
        public static async Task StartAsync(string dataSourceName, Action<IDeviceData> onNewData)
        {
            if (string.IsNullOrWhiteSpace(dataSourceName)) throw new ArgumentNullException("dataSourceName");

            switch (dataSourceName.ToLowerInvariant())
            {
                case "eventhub":
                {
                    await new EventHubDeviceDataOrigin(onNewData).StartAsync();
                }
                    break;

                case "random":
                {
                    await new RandomSensorDataOrigin(onNewData).StartAsync();
                }
                    break;

                default:
                    throw new ArgumentException("Unknown data source");
            }
        }
    }

    public class EventHubDeviceDataOrigin
    {
        private Action<IDeviceData> _onNewData;

        public EventHubDeviceDataOrigin(Action<IDeviceData> onNewData)
        {
            this._onNewData = onNewData;
        }

        public async Task StartAsync()
        {
            var ehName = AzureHelper.FromConfiguration("eventHubName");
            var ehNamespace = AzureHelper.FromConfiguration("eventHubNamespace");
            var ehKeyName = AzureHelper.FromConfiguration("eventHubKeyName");
            var ehKey = AzureHelper.FromConfiguration("eventHubKey");
            var ehConsumerGroup = AzureHelper.FromConfiguration("consumerGroupName") ?? "$Default";

            var eventHubConnection =
                AzureHelper.ServiceBusConnectionString(ehName, ehNamespace, ehKeyName, ehKey);

            var stoName = AzureHelper.FromConfiguration("storageName");
            var stoKey = AzureHelper.FromConfiguration("storageKey");

            var storageConnection = AzureHelper.StorageConnectionString(stoName, stoKey);

            Trace.TraceInformation("Connecting to {0}/{1}/{2}, storing in {3}", eventHubConnection, ehName, ehConsumerGroup, storageConnection);

            var factory = new DeviceDataProcessorFactory(item =>
            {
                Trace.TraceInformation("From EH: {0}", item.DeviceId);
                this._onNewData(item);
            });

            await
                AzureHelper.AttachProcessorForHub("fusethru", eventHubConnection, storageConnection, ehName,
                    ehConsumerGroup, factory);
        }
    }

    public class RandomSensorDataOrigin
    {
        public RandomSensorDataOrigin(Action<IDeviceData> onNewData, int seed = 13, int millBetweenData = 1000, int numbDevices = 20 )
        {
            this._random = new Random(seed);
            this._timeBetweenReadings = TimeSpan.FromMilliseconds(millBetweenData);
            this._numDevices = numbDevices;
            this.Cancel = new CancellationToken();
            this._onData = onNewData;
        }

        public CancellationToken Cancel;

        private Random _random;
        private TimeSpan _timeBetweenReadings;
        private int _numDevices;
        private Dictionary<int, DeviceData> _previousData = new Dictionary<int, DeviceData>();
        private Action<IDeviceData> _onData;

        public async Task StartAsync()
        {
            while (!this.Cancel.IsCancellationRequested)
            {
                var data = GenerateData();
                this._onData(data);
                await Task.Delay(_timeBetweenReadings);
            }
        }

        private DeviceData GenerateData()
        {
            DeviceData cur;
            var device = _random.Next(1, _numDevices);

            cur = new DeviceData()
            {
                DeviceId = device.ToString(),
                StreamSensorData = new StreamSensorData()
                {
                    SensorName = device.ToString(),
                    SensorData = new SensorData()
                    {
                        SensorReadingTime = DateTime.Now,
                        SensorReading = new List<string>()
                            {
                                "Yup " + device
                            }
                    }
                }
            };

            Trace.TraceInformation("Generated {0}", device);
            return cur;
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using FuseThru.Common;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace FuseThru.Common
{
    public class AzureHelper
    {
        private static ConcurrentDictionary<string, string> _ConfigurationEntries = new ConcurrentDictionary<string, string>();

        public static string FromConfiguration(string name)
        {
            return _ConfigurationEntries.GetOrAdd(name, x => CloudConfigurationManager.GetSetting(x) ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User));
        }

        public static string ServiceBusConnectionString(string serviceBusName, string serviceBusNamespace, string sharedAccessKeyName, string sharedAccessKey, bool entityPath = false)
        {
            if (entityPath)
            {
                return string.Format("Endpoint=sb://{0}.servicebus.windows.net/;SharedAccessKeyName={1};SharedAccessKey={2};EntityPath={3}",
                serviceBusNamespace,
                sharedAccessKeyName,
                sharedAccessKey,
                serviceBusName);
            }
            else
            {
                return string.Format("Endpoint=sb://{0}.servicebus.windows.net/;SharedAccessKeyName={1};SharedAccessKey={2}",
                serviceBusNamespace,
                sharedAccessKeyName,
                sharedAccessKey);
            }
        }

        public static string StorageConnectionString(string storageName, string storageKey)
        {
            return string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                storageName, storageKey);
        }

        public static T DeserializeMessage<T>(EventData data, JsonSerializerSettings settings = null)
        {
            var dataString = Encoding.UTF8.GetString(data.GetBytes());
            Trace.TraceInformation("Attempting to deserialize '{0}'", dataString);

            return settings == null
                ? JsonConvert.DeserializeObject<T>(dataString)
                : JsonConvert.DeserializeObject<T>(dataString, settings);
        }

        public static async Task<EventProcessorHost> AttachProcessorForHub(
            string processorName, 
            string eventHubConnection, 
            string storageConnection, 
            string eventHubName, 
            string consumerGroup, 
            IEventProcessorFactory factory)
        {
            var eventProcessorHost = new EventProcessorHost(processorName, eventHubName, consumerGroup,
                eventHubConnection, storageConnection);
            await eventProcessorHost.RegisterEventProcessorFactoryAsync(factory);

            return eventProcessorHost;
        }
    }

    public class DeviceDataProcessorFactory : IEventProcessorFactory
    {
        public DeviceDataProcessorFactory(Action<IDeviceData> onItem)
        {
            this._onItem = onItem;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new DeviceDataProcessor(this._onItem);
        }

        private Action<IDeviceData> _onItem;
    }

    public class DeviceDataProcessor : IEventProcessor
    {
        private Action<IDeviceData> _onItem;
        public const int MessagesBetweenCheckpoints = 100;
        private int untilCheckpoint = MessagesBetweenCheckpoints;

        public DeviceDataProcessor(Action<IDeviceData> onItem)
        {
            this._onItem = onItem;
        }

        public Task OpenAsync(PartitionContext context)
        {
            Trace.TraceInformation("Opening RouteItemProcessor");
            return Task.FromResult(false);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var message in messages)
            {
                var routeItem = AzureHelper.DeserializeMessage<DeviceData>(message);

                try
                {
                    this._onItem(routeItem);
                }
                catch (Exception up)
                {
                    Trace.TraceError("Failed to process {0}: {1}", routeItem, up);
                    throw;
                }

                this.untilCheckpoint--;

                if (this.untilCheckpoint == 0)
                {
                    await context.CheckpointAsync();
                    this.untilCheckpoint = MessagesBetweenCheckpoints;
                }
            }
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.TraceInformation("Closing RouteItemProcessor: {0}", reason);
            return Task.FromResult(false);
        }
    }
}

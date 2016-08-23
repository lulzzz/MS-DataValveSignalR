using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FuseThru.Common;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace SampleDataPump
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteToEventHub();

            Console.ReadKey();
        }

        private static void WriteToEventHub()
        {
            var ehName = AzureHelper.FromConfiguration("eventHubName");
            var ehNamespace = AzureHelper.FromConfiguration("eventHubNamespace");
            var ehKeyName = AzureHelper.FromConfiguration("eventHubKeyName");
            var ehKey = AzureHelper.FromConfiguration("eventHubKey");

            var eventHubConnection =
                AzureHelper.ServiceBusConnectionString(ehName, ehNamespace, ehKeyName, ehKey, true);

            Console.WriteLine(eventHubConnection);

            var eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnection);

            new RandomSensorDataOrigin(dt =>
            {
                Console.WriteLine("Sending {0}", dt.DeviceId);
                var data = new EventData(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dt)));
                eventHubClient.Send(data);
            }).StartAsync().Wait();
        }
    }
}

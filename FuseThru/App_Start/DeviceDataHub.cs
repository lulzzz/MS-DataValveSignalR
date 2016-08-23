using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace FuseThru.App_Start
{
    public class DeviceDataHub : Hub
    {
        public static IHubContext Hub()
        {
            return GlobalHost.ConnectionManager.GetHubContext<DeviceDataHub>();
        }

        public static void Send(IHubContext hub, string deviceId)
        {
            hub.Clients.All.newDeviceReading(deviceId);
        }

        public void Send(string deviceId)
        {
            Clients.All.newDeviceReading(deviceId);
        }
    }
}
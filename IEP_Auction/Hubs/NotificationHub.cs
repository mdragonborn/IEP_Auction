using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Web.Script.Serialization;

namespace IEP_Auction.Hubs
{
    public class NotificationHub : Hub
    {
        public IHubContext context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
        public void NotifyAll(object update, string alertType)
        {
            context.Clients.All.displayNotification(new JavaScriptSerializer().Serialize(update), alertType);
        }

    }
}
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SmartFleet.Core.Data;
using SmartFleet.Data;
using SmartFleet.Data.Dbcontextccope.Implementations;

namespace SmartFLEET.Web.Hubs
{
    public static class SignalRHubManager
    {
        public static IHubCallerConnectionContext<dynamic> Clients { get; set; }
        public static Dictionary<string, string> Connections = new Dictionary<string, string>();
        public static HubCallerContext Context { get; set; }
        public static IGroupManager Group { get; set; }
        public static IDbContextScopeFactory DbContextScopeFactory = new DbContextScopeFactory();
    }
}

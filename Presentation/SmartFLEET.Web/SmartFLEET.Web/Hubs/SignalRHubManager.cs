﻿using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SmartFleet.Core.Data;
using SmartFleet.Data.Dbcontextccope.Implementations;

namespace SmartFLEET.Web.Hubs
{
    /// <summary>
    /// 
    /// </summary>
    public static class SignalRHubManager
    {
        /// <summary>
        /// 
        /// </summary>
        public static IHubCallerConnectionContext<dynamic> Clients { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<string, string> Connections = new Dictionary<string, string>();
        /// <summary>
        /// 
        /// </summary>
        public static HubCallerContext Context { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static IGroupManager Group { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static IDbContextScopeFactory DbContextScopeFactory = new DbContextScopeFactory();
    }
}

using System.Net;

namespace SmartFleet.Core
{
    public interface ICommonServer
    {
        /// <summary>
        /// Teltonika
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        void StartTeltonikaListner(IPAddress ipAddress, int port);
    }
}
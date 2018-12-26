using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using SmartFleet.Core.Contracts.Commands;

namespace SmartFleet.Core.Protocols.Teltonika
{
    public interface ITeltonikaReciever
    {
        Task<string> GetIMEI(string dataReceived, NetworkStream stream);
        void ParseAvlData(byte[] buffer, List<CreateTeltonikaGps> gpsResult, string imei, NetworkStream stream);
        Task DecodeAndSendData(byte[] buffer, List<CreateTeltonikaGps> gpsResult, string imei, NetworkStream stream,
            TcpClient client);
        Task SendDecodedData(List<CreateTeltonikaGps> gpsResult, string imei);
    }
}
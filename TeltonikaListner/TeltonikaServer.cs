using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using SmartFleet.Core.Contracts.Commands;
using SmartFleet.Core.Helpers;
using SmartFleet.Core.Protocols;
using SmartFleet.Core.ReverseGeoCoding;

namespace TeltonikaListner
{
    public class TeltonikaServer
    {
        private readonly IBusControl _bus;
        private readonly Task<ISendEndpoint> _endpoint;
        private readonly ReverseGeoCodingService _reverseGeoCodingService;
        private TcpListener _listener;

        public TeltonikaServer(IBusControl bus, ReverseGeoCodingService reverseGeoCodingService)
        {
            _bus = bus;
            _reverseGeoCodingService = reverseGeoCodingService;
            _endpoint = _bus.GetSendEndpoint(new Uri( "rabbitmq://zcckffbw:QKVVIKHQgsx_QQ8qbxeb1Dl-E9jsKlSJ@eagle.rmq.cloudamqp.com/zcckffbw/Teltonika.endpoint"));

        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, 34400);
            _listener.Start();
            while (true) // Add your exit flag here
            {
                var client = _listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(ThreadProc, client);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        // ReSharper disable once TooManyDeclarations
        private async void  ThreadProc(object state)
        {
            try
            {
                var client = (TcpClient)state;
                byte[] buffer = new byte[client.ReceiveBufferSize];
                NetworkStream stream = ((TcpClient)state).GetStream();
                int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize) - 2;
                string imei = Encoding.ASCII.GetString(buffer, 2, bytesRead);
                if (Commonhelper.IsValidImei(imei))
                    await ParseAvlData(client, stream, imei);
            }
            catch (InvalidCastException e)
            {
                Trace.TraceWarning(e.Message);
            }
            catch (Exception e)
            {
                Trace.TraceWarning(e.Message);
                //throw;
            }
        }

        // ReSharper disable once MethodTooLong
        private async Task ParseAvlData(TcpClient client, NetworkStream stream, string imei)
        {
            Trace.TraceInformation("IMEI received : " + imei);
            Trace.TraceInformation("--------------------------------------------");
            Byte[] b = { 0x01 };
            await stream.WriteAsync(b, 0, 1);
            var command = new CreateBoxCommand();
            command.Imei = imei;
            await _endpoint.Result.Send(command);
            while (true)
            {
                stream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];
                await stream.ReadAsync(buffer, 0, client.ReceiveBufferSize);
                List<byte> list = new List<byte>();
                foreach (var b1 in buffer.Skip(9).Take(1)) list.Add(b1);
                int dataCount = Convert.ToInt32(list[0]);
                var gpsResult = await ParsseAvlData(imei, buffer);
                var bytes = Convert.ToByte(dataCount);
               if(client.Connected)
                   await stream.WriteAsync(new byte[] { 0x00, 0x00, 0x00, bytes }, 0, 4);
                if (!gpsResult.Any() && imei.Any()) continue;
                foreach (var gpSdata in gpsResult)
                    if (_bus != null)
                        await _bus.Publish(gpSdata);
                // break;
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private async Task<List<CreateTeltonikaGps>> ParsseAvlData(string imei, byte[] buffer)
        {
            List<CreateTeltonikaGps> gpsResult = new List<CreateTeltonikaGps>();
            var parser = new DevicesParser();
            gpsResult.AddRange(parser.Decode(new List<byte>(buffer), imei));
            await GeoReverseCodeGpsData(gpsResult);
            LogAvlData(gpsResult);
            return gpsResult;
        }

        private  async Task GeoReverseCodeGpsData(List<CreateTeltonikaGps> gpsRessult)
        {
            foreach (var gpSdata in gpsRessult)
                gpSdata.Address = await _reverseGeoCodingService.ReverseGoecode(gpSdata.Lat, gpSdata.Long);

        }
        private static void LogAvlData(List<CreateTeltonikaGps> gpsResult)
        {
            foreach (var gpsData in gpsResult.OrderBy(x => x.Timestamp))
            {
                Trace.TraceInformation("Date:" + gpsData.Timestamp + " Latitude: " + gpsData.Lat + " Longitude" +
                                       gpsData.Long + " Speed :" + gpsData.Speed + "Direction: " + gpsData.Direction);
                Trace.TraceInformation("--------------------------------------------");
                foreach (var io in gpsData.IoElements_1B)
                    Trace.TraceInformation("Propriété IO (1 byte) : " + (TNIoProperty) io.Key + " Valeur:" + io.Value);
                foreach (var io in gpsData.IoElements_2B)
                    Trace.TraceInformation("Propriété IO (2 byte) : " + (TNIoProperty) io.Key + " Valeur:" + io.Value);
                foreach (var io in gpsData.IoElements_4B)
                    Trace.TraceInformation("Propriété IO (4 byte) : " + (TNIoProperty) io.Key + " Valeur:" + io.Value);
                foreach (var io in gpsData.IoElements_8B)
                    Trace.TraceInformation("Propriété IO (8 byte) : " + (TNIoProperty) io.Key + " Valeur:" + io.Value);
                Trace.TraceInformation("--------------------------------------------");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.AzureServiceBusTransport;
using Microsoft.ServiceBus;
using Microsoft.WindowsAzure.ServiceRuntime;
using Serilog;
using Serilog.Core;
using SmartFleet.Core.Protocols.NewBox;
using SmartFleet.Core.Protocols.Tk1003;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SmartFleet.Core.Contracts.Commands;
using SmartFleet.Core.Protocols;

namespace SmartFleet.TcpWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);
        private static IBusControl _bus;
        private TcpListener _listener;
        private static Logger _log;
        private static bool arduinoBox = false;
        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable once MethodNameNotMeaningful
        public override void Run()
        {
            InitLog();

            try
            {
                if (_listener == null)
                    StartTcpListner();
                InitBus();
                _bus.Start();
                while (true) // Add your exit flag here
                {
                    var client = _listener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(ThreadProc, client);
                }
            }
            catch (Exception exception)
            {
                _bus.Stop();
                Trace.TraceError(exception.Message);
            }
            finally
            {
                _runCompleteEvent.Set();
            }
        }

        private static void InitLog()
        {
            _log = new LoggerConfiguration()
                // .WriteTo.Console()
                .WriteTo.File("tcp-worker-role.txt")
                .CreateLogger();
        }

        private void StartTcpListner()
        {
            _listener = new TcpListener(RoleEnvironment.CurrentRoleInstance
                .InstanceEndpoints["Smartfleet.server"].IPEndpoint);
            _listener.Start();
        }

        private static void InitBus()
        {
            _bus = Bus.Factory.CreateUsingAzureServiceBus(sbc =>
            {
                var serviceUri = ServiceBusEnvironment.CreateServiceUri("sb",
                    ConfigurationManager.AppSettings["AzureSbNamespace"],
                    ConfigurationManager.AppSettings["AzureSbPath"]);

                sbc.Host(serviceUri,
                    h =>
                    {
                        h.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(
                            ConfigurationManager.AppSettings["AzureSbKeyName"],
                            ConfigurationManager.AppSettings["AzureSbSharedAccessKey"],
                            //TimeSpan.FromDays(1),
                            TokenScope.Namespace);
                    });
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private static async void ThreadProc(object state)
        {
            var client = (TcpClient)state;
            byte[] buffer = new byte[client.ReceiveBufferSize];
            NetworkStream stream = ((TcpClient)state).GetStream();
            int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
            string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead) ;

            try
            {
                string imei = string.Empty;
                var codecId = -1;
                Int32.TryParse(dataReceived.Skip(8).Take(1).ToList()[0].ToString(), out codecId);
                if (codecId == 8)
                {
                    await ParseTeltonikaData(imei, dataReceived, stream, buffer);
                    client.Close();
                }
                // il s'agit du format de nouveaux boitiers créent par khaled
               else if (dataReceived.Contains(",") && !dataReceived.Contains("("))
                {
                    arduinoBox = true;
                    NewBoxParser parser = new NewBoxParser();
                    dataReceived = dataReceived.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                    try
                    {
                        var result = parser.Parse(dataReceived.Split('\r'));
                        foreach (var r in result)
                           await _bus.Publish(r);
                       
                    }
                    catch (ValidationException e)
                    {
                        _log.Error("error message :" + e.Message + " details:" + e.InnerException + " at:" + DateTime.Now);
                        //throw;
                    }


                }
                else
                {
                    // boitier GT02A
                    Tk1003Parser parser = new Tk1003Parser();
                    arduinoBox = false;
                    var result = parser.Parse(dataReceived.Split('\r'));
                    // envoyer une message de reception et une déconnexion aux clients
                    if (client.Connected)
                        await SendCommand(stream, result.FirstOrDefault().Value, client);

                    foreach (var r in result)
                    {
                        foreach (var createTk103Gpse in r.Key)
                           await _bus.Publish(createTk103Gpse);
                    }
                  
                    // client.Close();
                }
            }
            catch (Exception e)
            {
                _log.Error("error message :" + e.Message + " details:" + e.InnerException + " at:" + DateTime.Now);
                Console.WriteLine(e);
                if(!arduinoBox)
                    client.Close();
            }
            


            // Console.ReadLine();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imei"></param>
        /// <param name="dataReceived"></param>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private static async Task ParseTeltonikaData(string imei, string dataReceived, NetworkStream stream, byte[] buffer)
        {
            var gpsResult = new List<CreateTeltonikaGps>();
            while (true)
            {
                if (imei == string.Empty)
                {
                    imei = dataReceived;
                    Console.WriteLine("IMEI received : " + dataReceived);

                    Byte[] b = {0x01};
                    stream.Write(b, 0, 1);
                    var command = new CreateBoxCommand();
                    command.Imei = imei;
                    await _bus.Send(command);
                }
                else
                {
                    int dataNumber = Convert.ToInt32(buffer.Skip(9).Take(1).ToList()[0]);

                    while (dataNumber > 0)
                    {
                        var parser = new DevicesParser();
                        gpsResult.AddRange(parser.Decode(new List<byte>(buffer), imei));
                        dataNumber--;
                    }

                  await  stream.WriteAsync(new byte[] {0x00, 0x00, 0x00, 0x01}, 0, 4).ConfigureAwait(false);
                   
                }

                if (gpsResult.Count > 0)
                    foreach (var gpSdata in gpsResult)
                        await _bus.Send(gpSdata);
            }
        }


        private static async Task SendCommand(NetworkStream stream, string msg, TcpClient client)
        {
            await stream.WriteAsync(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
            client.Close();
        }
        public override bool OnStart()
        {
            // Définir le nombre maximum de connexions simultanées
            ServicePointManager.DefaultConnectionLimit = 12;

            // Pour plus d'informations sur la gestion des modifications de configuration
            // consultez la rubrique MSDN à l'adresse https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("SmartFleet.TcpWorker has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("SmartFleet.TcpWorker is stopping");

            _cancellationTokenSource.Cancel();
            _runCompleteEvent.WaitOne();
            _listener.Stop();
            _bus.StopAsync();
            base.OnStop();

            Trace.TraceInformation("SmartFleet.TcpWorker has stopped");
        }

      
    }
}

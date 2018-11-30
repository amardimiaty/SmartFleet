using System;
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

namespace SmartFleet.TcpWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);
        private static IBusControl _bus;
        private TcpListener _listener;
        private static Logger _log;
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
                {
                    StartTcpListner();
                }

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
        private static void ThreadProc(object state)
        {
            var client = ((TcpClient)state);
            byte[] buffer = new byte[client.ReceiveBufferSize];
            NetworkStream stream = ((TcpClient)state).GetStream();
            int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
            string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead) ;

            try
            {
               // _log.Information(dataReceived + " at :" +  DateTime.Now);



                if (dataReceived.Contains(","))
                {
                    // il s'agit du format de nouveaux boitiers créent par khaled
                    NewBoxParser parser =  new NewBoxParser();
                    dataReceived= dataReceived.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                    var result = parser.Parse(dataReceived.Split('\r'));
                    foreach (var r in result)
                    {
                        //   Task.Run(async () => { await SendCommand(stream, r.Value, client); });
                        _bus.Publish(r);
                    }
                }
                else
                {
                    // boitier GT02A
                    Tk1003Parser parser = new Tk1003Parser();
                    var result = parser.Parse(dataReceived.Split('\r'));
                    foreach (var r in result)
                    {
                        Task.Run(async () => { await SendCommand(stream, r.Value, client); });
                        foreach (var createTk103Gpse in r.Key)
                        {
                            _bus.Publish(createTk103Gpse);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error("error message :"+ e .Message+" details:"+ e.InnerException +  " at:" + DateTime.Now);

                Console.WriteLine(e);
                //client.Close();
            }


            // Console.ReadLine();
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

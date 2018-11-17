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
using SmartFleet.Core.Contracts.Commands;
using SmartFleet.Core.Protocols.NewBox;
using SmartFleet.Core.Protocols.Tk1003;

namespace SmartFleet.TcpWorker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private static IBusControl _bus;
        private TcpListener listener;
        private static Logger _log;
        public override void Run()
        {
            Trace.TraceInformation("SmartFleet.TcpWorker is running");
             _log = new LoggerConfiguration()
               // .WriteTo.Console()
                .WriteTo.File("tcp-worker-role.txt")
                .CreateLogger();

            //  Thread.Sleep(300000);
            try
            {
                if (listener == null)
                {
                    listener = new TcpListener(RoleEnvironment.CurrentRoleInstance
                        .InstanceEndpoints["Smartfleet.server"].IPEndpoint);
                    listener.Start();
                }

                InitBus();
                _bus.Start();

                while (true) // Add your exit flag here
                {
                    var client = listener.AcceptTcpClient();
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
                this.runCompleteEvent.Set();
            }
          
         
        }

        private static void InitBus()
        {
            _bus = Bus.Factory.CreateUsingAzureServiceBus(sbc =>
            {
                var serviceUri = ServiceBusEnvironment.CreateServiceUri("sb",
                    ConfigurationManager.AppSettings["AzureSbNamespace"],
                    ConfigurationManager.AppSettings["AzureSbPath"]);

                var host = sbc.Host(serviceUri,
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

        private static void ThreadProc(object state)
        {
            var client = ((TcpClient)state);
            byte[] buffer = new byte[client.ReceiveBufferSize];
            NetworkStream stream = ((TcpClient)state).GetStream();
            int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
            string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead) ;

            try
            {
                _log.Information(dataReceived + " at :" +  DateTime.Now);



                if (dataReceived.Contains(","))
                {
                    // il s'agit du format de nouveaux boitiers créent par khaled
                    NewBoxParser parser =  new NewBoxParser();
                    dataReceived= dataReceived.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                    var result = parser.Parse(dataReceived.Split('\r'));
                    foreach (var r in result)
                    {
                        //   Task.Run(async () => { await SendCommand(stream, r.Value, client); });
                        _bus.Publish<CreateNewBoxGps>(r);
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
                            _bus.Publish<CreateTk103Gps>(createTk103Gpse);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e+" at:" + DateTime.Now);

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

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();
            listener.Stop();
            _bus.StopAsync();
            base.OnStop();

            Trace.TraceInformation("SmartFleet.TcpWorker has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Remplacez le texte suivant par votre propre logique.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}

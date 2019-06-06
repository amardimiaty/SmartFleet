using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TeltonikaEmulator.Models;

namespace TeltonikaEmulator.TcpClient
{
    public delegate void UpdateLogDataGird(LogVm log);


    public class Client : IDisposable
    {
        private readonly int _port;
        private readonly string _server;
        private System.Net.Sockets.TcpClient _client;
        private NetworkStream _stream;
        private int minBufferSize = 8192;
        private int maxBufferSize = 15 * 1024 * 1024;
        private int _bufferSize = 4;
        public Client(String server, Int32 port)
        {
            _port = port;
            _server = server;

        }

        public event EventHandler<byte[]> OnDataReceived;
        public event EventHandler OnDisconnected;

        public void Disconnect()
        {
            _client?.Close();
            IsConnected = false;
        }


        private async Task Close()
        {
            await Task.Yield();
            if (_client != null)
            {

                _client.Close();
                //_client.Dispose();
                _client = null;
            }
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }
        private async Task CloseIfCanceled(CancellationToken token, Action onClosed = null)
        {
            if (token.IsCancellationRequested)
            {
                await Close();
                onClosed?.Invoke();
                token.ThrowIfCancellationRequested();
            }
        }
        public async Task SendAsync(byte[] data, CancellationToken token = default(CancellationToken))
        {
            try
            {
                await _stream.WriteAsync(data, 0, data.Length, token);
                await _stream.FlushAsync(token);

            }
            catch (IOException ex)
            {
                if (ex.InnerException != null && ex.InnerException is ObjectDisposedException) // for SSL streams
                {
                }
                else OnDisconnected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Thread.Sleep(500);
               // await SendAsync(data, token);
            }
        }



        public async Task<Byte[]> ReceiveAsync(CancellationToken token = default(CancellationToken))
        {
            byte[] buffer = new byte[_bufferSize];
            byte[] data = new byte[4];
            try
            {
                if (!IsConnected || IsRecieving)
                    throw new InvalidOperationException();
                IsRecieving = true;
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token);

                data = new byte[bytesRead];
                Array.Copy(buffer, data, bytesRead);
                IsRecieving = false;
                // buffer = new byte[bufferSize];
            }
            catch (ObjectDisposedException)
            {
            }
            catch (IOException ex)
            {
                var evt = OnDisconnected;
                if (ex.InnerException != null && ex.InnerException is ObjectDisposedException) { } // for SSL streams

                if (evt != null)
                    evt(this, EventArgs.Empty);
            }
            finally
            {
                IsRecieving = false;
            }
            return data;

        }
        public bool IsRecieving { get; set; }

        public bool IsConnected { get; set; }

        public async Task ConnectAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                //Connect async method
                await Close();
                cancellationToken.ThrowIfCancellationRequested();
                _client = new System.Net.Sockets.TcpClient();
                cancellationToken.ThrowIfCancellationRequested();
                await _client.ConnectAsync(_server, _port);
                await CloseIfCanceled(cancellationToken);
                // get stream and do SSL handshake if applicable

                _stream = _client.GetStream();
                await CloseIfCanceled(cancellationToken);

            }
            catch (Exception e)
            {
                CloseIfCanceled(cancellationToken).Wait();
                throw;
            }
        }
        public void CloseStream()
        {
            _stream?.Close();
        }


        public void Dispose()
        {
            ((IDisposable)_client)?.Dispose();
            _stream?.Dispose();
        }
    }
}

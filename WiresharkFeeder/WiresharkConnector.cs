// This source code file does not look like the origin any more. However, I had referenced the below source code file at first.
// https://wiki.wireshark.org/uploads/__moin_import__/attachments/CaptureSetup/Pipes/WiresharkSender.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace WiresharkFeeder
{
    public class WiresharkConnector : IDisposable
    {
        private NamedPipeServerStream WiresharkPipe;

        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    Console.WriteLine($"Wireshark connection is {(IsConnected ? "connected" : "disconnected")}");
                    OnConnectionStateChanged?.Invoke(IsConnected);
                }
            }
        }

        public event ConnectionStateChanged OnConnectionStateChanged;
        public delegate void ConnectionStateChanged(bool isConnected);

        private AutoResetEvent _eventCreatePipe;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private Task _taskPipeCreating;

        private bool disposedValue;

        public WiresharkConnector(string pipe_name)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _eventCreatePipe = new AutoResetEvent(false);

            WiresharkPipe = new NamedPipeServerStream(pipe_name, PipeDirection.Out, 1, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            _taskPipeCreating = new Task(PipeCreatingProc, _cancellationToken, TaskCreationOptions.LongRunning);
            _taskPipeCreating.Start();
        }

        private void PipeCreatingProc()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    IsConnected = false;

                    Console.WriteLine("Waiting for Wireshark to connect...");
                    WiresharkPipe.WaitForConnection();

                    IsConnected = true;
                }
                catch
                { }

                _eventCreatePipe.WaitOne();
            }
        }

        public void SendPacket(byte[] buffer, int offset, int count)
        {
            if (!IsConnected) return;

            try
            {
                WiresharkPipe?.Write(buffer, offset, count);
            }
            catch (System.IO.IOException)
            {
                // broken pipe, try to restart
                WiresharkPipe?.Disconnect();
                _eventCreatePipe.Set();
            }
            catch (Exception)
            {
                // Unknow error, not due to the pipe
                // No need to restart it
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _cancellationTokenSource.Cancel();
                    _taskPipeCreating.Wait();

                    WiresharkPipe?.Dispose(); // Why Dispose before Close?
                                              // Lets look at the Method's comment. It says to Dispose before calling Close.
                    WiresharkPipe?.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WiresharkConnector()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

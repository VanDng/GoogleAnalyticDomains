using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WiresharkFeeder;

namespace WiresharkFeeder
{
    class PCAPPacketSourceFile : IPCAPPacketSource, IDisposable
    {
        private byte[] _globalHeaderPacket;
        private byte[] _captureHeaderPacketBuffer;

        private string _filePath;

        private FileHelper _fileHelper;

        private ManualResetEventSlim _eventPacketSourcing;
        private ManualResetEventSlim _eventGlobalHeaderReading;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private Task _taskSourcing;
        private bool _isSendGlobalHeader;

        private Task _taskGlobalHeaderReading;

        public event NewPacketArrived OnNewPacketArrived;

        public PCAPPacketSourceFile(string filePath, int capturePacketLength = 262144)
        {
            _globalHeaderPacket = new byte[24];
            _captureHeaderPacketBuffer = null;

            _filePath = filePath;

            _eventPacketSourcing = new ManualResetEventSlim(true);
            _eventGlobalHeaderReading = new ManualResetEventSlim(true);

            _fileHelper = new FileHelper(_filePath, clearFileOnStartupOrStreamReset: true);
            _fileHelper.OnFileStreamIsReset += FileStreamReset;

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            
            _taskSourcing = new Task(SourcingProc, _cancellationToken, TaskCreationOptions.LongRunning);
            _taskSourcing.Start();

            _taskGlobalHeaderReading = new Task(GlobalHeaderReadingProc, _cancellationToken, TaskCreationOptions.LongRunning);
            _taskGlobalHeaderReading.Start();
        }

        private void FileStreamReset()
        {
            _eventGlobalHeaderReading.Set();
        }

        private async void GlobalHeaderReadingProc()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                _eventGlobalHeaderReading.Wait();

                // Stop sourcing until the global header is prepared
                _eventPacketSourcing.Reset();

                _isSendGlobalHeader = true;

                Console.WriteLine("Reading global header packet");

                while (!ReadGlobalHeaderPacket())
                {
                    await Task.Delay(300);
                }
                InitializeCapturePacketBuffer();

                // Continue sourcing
                _eventPacketSourcing.Set();

                _eventGlobalHeaderReading.Reset();
            }
        }

        private async void SourcingProc()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                byte[] buffer = null;
                int offset = 0;
                int count = 0;

                _eventPacketSourcing.Wait();

                bool isGlobalHeakderPacket = false; 

                //Console.WriteLine("Fetching packet");

                if (_isSendGlobalHeader)
                {
                    if (!GetGlobalHeader(ref buffer, ref offset, ref count))
                    {
                        goto tagGoTryAgain;
                    }

                    _isSendGlobalHeader = false;
                    isGlobalHeakderPacket = true;
                }
                else
                {
                    if (!GetNextCapturePacket(ref buffer, ref offset, ref count))
                    {
                        goto tagGoTryAgain;
                    }
                }
                
                if (buffer != null)
                {
                    //Console.WriteLine($"Sending packet. Type '{(isGlobalHeakderPacket ? "Global header" : "Capture packet")}'");
                    OnNewPacketArrived?.Invoke(buffer, offset, count);
                }

                tagGoTryAgain:;
#if DEBUG
                // For development purpose only, sleeping is to slow down everything so it is easier to watch the result.
                await Task.Delay(500);
#else
                await Task.Delay(5); // It fixes CPU High issue.
#endif
            }
        }

        private bool ReadGlobalHeaderPacket()
        {
            // To prevent repetitive allocation, I re-use the same buffer.
            // I use 1st byte to determine whether it is filled successfully.
            _globalHeaderPacket[0] = 0;

            if (_fileHelper.ReadBytes(_globalHeaderPacket, 0, _globalHeaderPacket.Length) == 0)
            {
                return false;
            }

            PCAPGlobalHeaderPacket globalHeaderPacket = new PCAPGlobalHeaderPacket();
            globalHeaderPacket.FromBytes(_globalHeaderPacket);

            Console.WriteLine($"PCAP Global header: Data link type '{globalHeaderPacket.datalinkType}'");
            Console.WriteLine($"PCAP Global header: Capture packet length '{globalHeaderPacket.capturePacketLength}'");

            return true;
        }

        private void InitializeCapturePacketBuffer()
        {
            Console.WriteLine("Initialize Capture Packet Buffer");

            PCAPGlobalHeaderPacket globalHeaderPacket = new PCAPGlobalHeaderPacket();
            globalHeaderPacket.FromBytes(_globalHeaderPacket);

            _captureHeaderPacketBuffer = new byte[globalHeaderPacket.capturePacketLength];
        }

        public bool GetGlobalHeader(ref byte[] buffer, ref int offset, ref int count)
        {
            buffer = _globalHeaderPacket;
            offset = 0;
            count = _globalHeaderPacket.Length;

            if (buffer[0] == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool GetNextCapturePacket(ref byte[] buffer, ref int offset, ref int count)
        {
            try
            {
                //
                // Header
                //
                if (_fileHelper.ReadBytes(_captureHeaderPacketBuffer, 0, 16) == 0)
                {
                    return false;
                }

                var captureHeaderPacket = new PCAPCaptureHeaderPacket();
                captureHeaderPacket.FromBytes(_captureHeaderPacketBuffer);

                //
                // Data
                //
                if (_fileHelper.ReadBytes(_captureHeaderPacketBuffer, 16, (int) captureHeaderPacket.incl_len) == 0)
                {
                    return false;
                }

                //
                // Return
                //
                buffer = _captureHeaderPacketBuffer;
                offset = 0;
                count = 16 + (int)captureHeaderPacket.incl_len;
            }
            catch
            {
                Console.WriteLine("Failed to read next capture packet");
            }

            return true;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _eventPacketSourcing.Set();
            _eventGlobalHeaderReading.Set();

            _taskSourcing.Wait();
            _taskGlobalHeaderReading.Wait();
        }

        public void ResendGlobalHeaderPacket()
        {
            _isSendGlobalHeader = true;
        }
    }
}

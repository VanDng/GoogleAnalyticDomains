using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using WiresharkFeeder;

namespace WiresharkFeeder
{
    class Program
    {
#if DEBUG
        private static string _pcapFilePath = @"..\..\..\..\Docker_TCPDump\tcpdump.pcap";
#else
        private static string _pcapFilePath = @"..\Docker_TCPDump\tcpdump.pcap";
#endif

        private static WiresharkConnector _wireSharkConnector;

        private static IPCAPPacketSource _packetSource;

        private static List<IDisposable> _disposableInstances;

        static void Main(string[] args)
        {
            Initialize();
            
            RegisterEvents();

            Console.Read();
            
            Dispose();
        }

        private static void Initialize()
        {
            _disposableInstances = new List<IDisposable>();

            _wireSharkConnector = new WiresharkConnector("networkanalysis");
            _disposableInstances.Add(_wireSharkConnector);

            var packetSourceInstance = new PCAPPacketSourceFile(_pcapFilePath);
            _packetSource = packetSourceInstance;
            _disposableInstances.Add(packetSourceInstance);
        }

        private static void RegisterEvents()
        {
            _wireSharkConnector.OnConnectionStateChanged += (isConnected) =>
            {
                if (isConnected)
                {
                    _packetSource.ResendGlobalHeaderPacket();
                }
            };

            _packetSource.OnNewPacketArrived += (buffer, offset, count) =>
            {
                _wireSharkConnector.SendPacket(buffer, offset, count);
            };
        }

        private static void Dispose()
        {
            foreach (var disposableInstance in _disposableInstances)
            {
                disposableInstance?.Dispose();
            }
        }
    }
}

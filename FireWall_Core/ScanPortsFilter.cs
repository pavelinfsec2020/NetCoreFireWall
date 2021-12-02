using PacketDotNet;
using System;
using System.Net;

namespace FireWall_Core
{
    internal class ScanPortsFilter
    {
        private int[] _scannedPorts;
        private PacketsSender _packetsSender;
        public ScanPortsFilter(int[] scannedPorts, IPAddress destIP)
          {
                  _scannedPorts = scannedPorts;
                  _packetsSender = new PacketsSender(destIP);
                  
          }
        private int FindPort(int port)
        {
            for (int i = 0; i < _scannedPorts.Length; i++)
            { 
                if(_scannedPorts[i] == port)
                    return i;
            }
            return -1;
        }
        private void AnalyzeSecurePorts(ushort selectedPort, Packet packet)
        {
            int port = (int)selectedPort;
            int rezIndex = FindPort(port);

            if (rezIndex == -1)
            {
                _packetsSender.TransmitPacket(packet);
            }
            else
            {
                string body = String.Format("Сканирование порта {0} от {1} " ,
                    _scannedPorts[rezIndex],
                    IpPacket.GetEncapsulated(packet).SourceAddress.ToString()
                    );
                new Incident(body).WriteToLog();
            }
        }
        public void FilterPacket(Packet packet)
        {
            var tcpPacket = TcpPacket.GetEncapsulated(packet);
            var udpPacket = UdpPacket.GetEncapsulated(packet);

            if (tcpPacket != null)
            {
                AnalyzeSecurePorts(tcpPacket.DestinationPort, packet);

            }
            else if(udpPacket != null)
            {
               AnalyzeSecurePorts(udpPacket.DestinationPort,packet);   
            }
            else
            {
                _packetsSender.TransmitPacket(packet);
            }
        }
    }
}

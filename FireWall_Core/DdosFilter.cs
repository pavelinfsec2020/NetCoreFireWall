using PacketDotNet;
using System.Net;

namespace FireWall_Core
{

    internal class DdosFilter
    {
        private readonly int _maxSynPackets;
        private List<ushort> _portsList;
        private List<int> _countOfPackets;
        private List<int> _countOfDetections;
        private ArpFilter arpFilter;
        public DdosFilter(int maxSynPackets, Dictionary<string, string> arpTable, int[] scannedPorts, IPAddress ip)
        { 
             _maxSynPackets = maxSynPackets;
           _portsList = new List<ushort>();
            _countOfPackets = new List<int>();
            _countOfDetections = new List<int>();
          arpFilter = new ArpFilter(arpTable,scannedPorts,ip);
        }
        private int FindPort(ushort port)
        {
            for (int i = 0; i < _portsList.Count; i++)
            {
                if (_portsList[i] == port)
                    return i;
            }
           return -1;
        }
        public  void FilterPacket(Packet packet)
        {
            var tcpPacket = TcpPacket.GetEncapsulated(packet);
            if (tcpPacket != null)
            {
                if (tcpPacket.Syn == true && tcpPacket.Ack == false)
                {
                    int searchRezult = FindPort(tcpPacket.DestinationPort);

                    if (searchRezult == -1)
                    {
                        _portsList.Add(tcpPacket.DestinationPort);
                        _countOfPackets.Add(0);
                        _countOfDetections.Add(0);
                    }
                    else
                    {
                        if (_countOfPackets[searchRezult] < _maxSynPackets)
                        {
                            arpFilter.FilterPacket(packet);
                            _countOfPackets[searchRezult]++;
                        }
                        else
                        {
                            if (_countOfDetections[searchRezult] == 0 )
                            {
                                string body = String.Format(
                                    "DDOS атака на порт {0} от {1}",
                                    _portsList[searchRezult],
                                    IpPacket.GetEncapsulated(packet).SourceAddress.ToString()
                                    );

                                new Incident(body).WriteToLog();
                                _countOfDetections[searchRezult]++;
                            }
                           
                        }

                    }
                }
                else
                {
                    arpFilter.FilterPacket(packet);
                }
            }
            else
            {
                arpFilter.FilterPacket(packet);
            }
        }
       
    }
}

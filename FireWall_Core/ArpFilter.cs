using PacketDotNet;
using System.Net;


namespace FireWall_Core
{
    internal class ArpFilter
    {
        private string macAddress;
        private ScanPortsFilter _portsFilter;
        private readonly Dictionary<string, string> _arpTable;
        public ArpFilter(Dictionary<string, string> arpTable, int[] scannedPorts, IPAddress ip)
        {
            _portsFilter = new ScanPortsFilter(scannedPorts,ip);
            _arpTable = arpTable;
        }
        public void FilterPacket(Packet packet)
        {
            var arpPacket = ARPPacket.GetEncapsulated(packet);
            
            if (arpPacket != null )
            {
                if (_arpTable.TryGetValue( arpPacket.SenderProtocolAddress.MapToIPv4().ToString(),  out macAddress))
                {
                    if (macAddress == arpPacket.SenderHardwareAddress.ToString())
                    {
                        _portsFilter.FilterPacket(packet);
                    }
                    else
                    {
                        string body = String.Format("arp спуфинг от {0}", arpPacket.SenderProtocolAddress.MapToIPv4());
                        new Incident(body).WriteToLog();
                    }
                }
                else
                {
                    _portsFilter.FilterPacket(packet);
                }
            }
            else
            {
                _portsFilter.FilterPacket(packet);
            }
        }
    }
}

using PacketDotNet;
using System.Net;
using System.Net.NetworkInformation;


namespace FireWall_Core
{
    internal class PacketsSender
    {
        private readonly IPAddress _destinationIP;
        private EthernetPacket _ethPacket;
       
        public PacketsSender(IPAddress destinationIP)
        { 
             _destinationIP  = destinationIP;
              _ethPacket = new EthernetPacket(
                PhysicalAddress.Parse("F0:98:9D:1C:93:F6"),
                PhysicalAddress.Parse("F0:98:9D:1C:73:F6"),
                EthernetPacketType.None
                );
        }
        public void TransmitPacket(Packet safePacket)
        {

            IPAddress sourceIp;
            var packet = IpPacket.GetEncapsulated(safePacket);
            if (packet != null) sourceIp = packet.SourceAddress;
            else sourceIp = GetCurrentIP();
            var ipPacket = new IPv4Packet(IPAddress.Parse("8.8.8.8"), IPAddress.Parse(_destinationIP.ToString()));
            ipPacket.PayloadPacket = safePacket;
            _ethPacket.PayloadPacket = ipPacket;
         
            try { FireWall._captureDevice.SendPacket(_ethPacket); }
            catch (Exception e) { }
        }
        private IPAddress GetCurrentIP()
        {
            string host = System.Net.Dns.GetHostName();
            var ip = Dns.GetHostByName(host).AddressList[0];
            return ip;
        }
    }
}

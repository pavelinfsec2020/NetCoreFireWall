using PacketDotNet;
using SharpPcap;
using System.Net;

namespace FireWall_Core
{
    public class FireWall
    {
        private CaptureDeviceList _deviceList;
        public static ICaptureDevice _captureDevice;
        private DdosFilter _ddosFilter;
        
        public FireWall(
            IPAddress receiverNodeIPv4,
            int maxSynPackets,
            int[] scannedPorts,
            Dictionary<string, string> arpTable
            )
            
        {
            _deviceList = CaptureDeviceList.Instance;
            _ddosFilter = new DdosFilter(
                maxSynPackets, 
                arpTable,
                scannedPorts,
                receiverNodeIPv4
                );
        }
        public bool IsStarted
        {
            get { return _captureDevice.Started; }
        }
        public string[] GetNetInterfacesNames()
        {
           string[] names =  new string[_deviceList.Count];
           
            for(int i = 0;i< _deviceList.Count;i++)
            {
                names[i] = _deviceList[i].Description;
            }
            return names;
        }
        public void SetNetInterface(int indexOfInterf)
        {
            _captureDevice = _deviceList[indexOfInterf];
        }
        public async void StartFireWall()
        {
            _captureDevice.OnPacketArrival += OnPacketArrival;
            _captureDevice.Open(DeviceMode.Promiscuous, 1000);
             
            await Task.Run(() =>
            {
                try
                {
                    _captureDevice.Capture();
                }
                catch (Exception e)
                { }

            });
        }
        public async void StopFireWall()
        {
            await Task.Run(() =>
            {
                try
                {
                    _captureDevice.StopCapture();
                    _captureDevice.Close();
                }
                catch (Exception e)
                { }

            });
           
        }
        private async void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
           _ddosFilter.FilterPacket(packet);
            await Task.Delay(1000);
           
        }
    }
}
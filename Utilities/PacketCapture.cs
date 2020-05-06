using System;
using System.Net;
using System.Net.Sockets;

namespace WatchPartyBot.Utilities
{
    public class PacketCapture
    {
        public void StartCapture()
        {
            Socket socket =
                new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            //has to be your external IP address
            socket.Bind(new IPEndPoint(IPAddress.Parse("192.168.X.X"), 0)); // specify IP address
            socket.ReceiveBufferSize = 2 * 1024 * 1024; // 2 megabytes
            socket.ReceiveTimeout = 500; // half a second
            byte[] incoming = BitConverter.GetBytes(1);
            byte[] outgoing = BitConverter.GetBytes(1);
            socket.IOControl(IOControlCode.ReceiveAll, incoming, outgoing);
        }
    }
}

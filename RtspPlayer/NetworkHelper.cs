using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RtspPlayer
{
    public static class NetworkHelper
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern int GetBestInterfaceEx(ref SOCKADDR_INET destAddr, out int bestIfIndex);

        [StructLayout(LayoutKind.Sequential)]
        private struct SOCKADDR_INET
        {
            public ushort si_family;
            public SOCKADDR_IN ipv4;
            public SOCKADDR_IN6 ipv6;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SOCKADDR_IN
        {
            public ushort sin_family;
            public ushort sin_port;
            public uint sin_addr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] sin_zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SOCKADDR_IN6
        {
            public ushort sin6_family;
            public ushort sin6_port;
            public uint sin6_flowinfo;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] sin6_addr;
            public uint sin6_scope_id;
        }

        public static int? GetInterfaceIndexToReach(IPAddress destination)
        {
            SOCKADDR_INET addr = new SOCKADDR_INET();
            if (destination.AddressFamily == AddressFamily.InterNetwork)
            {
                addr.si_family = 2; // AF_INET
                addr.ipv4 = new SOCKADDR_IN
                {
                    sin_family = 2,
                    sin_port = 0,
                    sin_addr = BitConverter.ToUInt32(destination.GetAddressBytes(), 0),
                    sin_zero = new byte[8]
                };
            }
            else
            {
                throw new NotSupportedException("Only IPv4 is supported in this example.");
            }

            int result = GetBestInterfaceEx(ref addr, out int index);
            return result == 0 ? index : null;
        }

        public static string GetInterfaceIpByIndex(int index)
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var props = ni.GetIPProperties();
                if (ni.Supports(NetworkInterfaceComponent.IPv4) &&
                    props.GetIPv4Properties()?.Index == index)
                {
                    var addr = props.UnicastAddresses
                        .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
                    return addr?.Address.ToString();
                }
            }
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RtspPlayer
{
    public class FailoverRouteManager : IDisposable
    {
        
        private Timer timer;
        private bool isUsingPrimary = true;
        private int failureCount = 0;
        private const int MaxFailures = 3;

        public FailoverRouteManager()
        {
            
        }
        
        public void DeleteRoute(string targetIp)
        {
            
            // Delete existing route
            ExecuteCommand($"route delete {targetIp}",out string output, out string error );

            // Add new route
            //ExecuteCommand($"route add {targetIp} mask {mask} {newGateway} if {newIfIndex}");

        }

        public void DeleteCache()
        {

            // Delete existing route
            ExecuteCommand($"arp -d *", out string output, out string error);

            // Add new route
            //ExecuteCommand($"route add {targetIp} mask {mask} {newGateway} if {newIfIndex}");

        }

        //arp -d*

        public void DisableInterface(string interfaceName)
        {
            string query = $"SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = '{interfaceName}'";

            using (var searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    obj.InvokeMethod("Disable", null);
                    Console.WriteLine($"Disabled interface: {interfaceName}");
                }
            }
        }

        public void EnableInterface(string interfaceName)
        {
            string query = $"SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID = '{interfaceName}'";

            using (var searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    obj.InvokeMethod("Enable", null);
                    Console.WriteLine($"Enabled interface: {interfaceName}");
                }
            }
        }


        public void AddOrUpdateRoute(string destination, string mask, string gateway, int metric, int interfaceIndex)
        {
            string changeCmd = $"change {destination} mask {mask} {gateway} metric {metric} if {interfaceIndex}";
            string addCmd = $"add {destination} mask {mask} {gateway} metric {metric} if {interfaceIndex}";

            if (!RunRouteCommand(changeCmd, out string changeOutput))
            {
                Console.WriteLine("Route not found or not changed. Adding new route...");
                RunRouteCommand(addCmd, out _);
            }
            else if (changeOutput.Contains("The system cannot find the file specified", StringComparison.OrdinalIgnoreCase) ||
                     changeOutput.Contains("Element not found", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Route not found. Adding new route...");
                RunRouteCommand(addCmd, out _);
            }
        }

        private bool RunRouteCommand(string arguments, out string output)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "route",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                process.Start();
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                output = stdout + stderr;

                if (!string.IsNullOrWhiteSpace(stderr) || output.Contains("Element not found", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Route command output (possible error): {output.Trim()}");
                    return false;
                }

                Console.WriteLine($"Route command success: {output.Trim()}");
                return true;
            }
            catch (Exception ex)
            {
                output = $"Exception: {ex.Message}";
                Console.WriteLine(output);
                return false;
            }
        }

        static bool IsRunAsAdmin()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private bool ExecuteCommand(string command, out string output, out string error)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            output = process.StandardOutput.ReadToEnd();
            error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return process.ExitCode == 0;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
   
    public class NetworkInterfaceInfoForRoutes
    {
        public int InterfaceIndex { get; set; }
        public string InterfaceName { get; set; }
        public string InterfaceDescription { get; set; }
        public List<IPAddress> IPAddresses { get; set; }

        public NetworkInterfaceInfoForRoutes(int interfaceIndex, string interfaceName, string interfaceDescription)
        {
            InterfaceIndex = interfaceIndex;
            InterfaceName = interfaceName;
            InterfaceDescription = interfaceDescription;
            IPAddresses = new List<IPAddress>();
        }

        public override string ToString()
        {
            return $"{InterfaceName} (Index: {InterfaceIndex}, Description: {InterfaceDescription})";
        }
    }

    public class NetworkInterfaceManager
    {
        private List<NetworkInterfaceInfoForRoutes> networkInterfaces;

        public NetworkInterfaceManager()
        {
            networkInterfaces = new List<NetworkInterfaceInfoForRoutes>();
            LoadNetworkInterfaces();
        }

        // Method to load network interfaces into the list
        private void LoadNetworkInterfaces()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface netInterface in interfaces)
            {
                var ipProperties = netInterface.GetIPProperties();
                var ipv4Props = ipProperties.GetIPv4Properties();
                var networkInfo = new NetworkInterfaceInfoForRoutes(
                    ipv4Props.Index,
                    netInterface.Name,
                    netInterface.Description
                );

                // Add all the IP addresses to the network info
                foreach (var unicastAddress in ipProperties.UnicastAddresses)
                {
                    networkInfo.IPAddresses.Add(unicastAddress.Address);
                }

                networkInterfaces.Add(networkInfo);
            }
        }

        // Method to search by IP Address
        public NetworkInterfaceInfoForRoutes GetByIpAddress(string ipAddress)
        {
            foreach (var netInterface in networkInterfaces)
            {
                foreach (var ip in netInterface.IPAddresses)
                {
                    if (ip.ToString() == ipAddress)
                    {
                        return netInterface;
                    }
                }
            }
            return null;
        }

        // Method to search by Interface Index
        public NetworkInterfaceInfoForRoutes GetByInterfaceIndex(int interfaceIndex)
        {
            foreach (var netInterface in networkInterfaces)
            {
                if (netInterface.InterfaceIndex == interfaceIndex)
                {
                    return netInterface;
                }
            }
            return null;
        }

        // Method to list all interfaces
        public void ListInterfaces()
        {
            foreach (var netInterface in networkInterfaces)
            {
                Console.WriteLine(netInterface);
                foreach (var ip in netInterface.IPAddresses)
                {
                    Console.WriteLine($"  IP Address: {ip}");
                }
                Console.WriteLine("-----------------------------------");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Management;
using System.Management.Automation;

namespace DeviceInfo.Network {
    [Cmdlet(VerbsCommon.Get, "NetAdapterInfo")]
    public class GetNetAdapterInfo : PSCmdlet {

        private readonly Dictionary<byte, string> adapt_type = new Dictionary<byte, string> {
            {0, "Ethernet 802.3" },
            {1, "Token Ring 802.5" },
            {2, "Fiber Distributed Data Interface (FDDI)"},
            {3, "Wide Area Network (WAN)" },
            {4, "LocalTalk"},
            {5, "Ethernet using DIX header format" },
            {6, "ARCNET" },
            {7, "ARCNET (878.2)"},
            {8, "ATM" },
            {9, "Wireless" },
            {10, "Infrared Wireless" },
            {11, "Bpc" },
            {12, "CoWan"},
            {13, "1394" }
        };
        private readonly Dictionary<byte, string> avaliability = new Dictionary<byte, string> {
            {1, "Other" },
            {2, "Unknown" },
            {3, "Running/Full Power" },
            {4, "Warning" },
            {5, "In Test" },
            {6, "Not Applicable" },
            {7, "Power Off" },
            {8, "Off Line" },
            {9, "Off Duty" },
            {10, "Degraded" },
            {11, "Not Installed" },
            {12, "Install Error" },
            {13, "Power Save - Unknown" },
            {14, "Power Save - Low Power Mode" },
            {15, "Power Save - Standby" },
            {16, "Power Cycle" },
            {17, "Power Save - Warning" },
            {18, "Paused" },
            {19, "Not Ready" },
            {20, "Not Configured" },
            {21, "Quiesced" }
        };
        private readonly Dictionary<byte, string> error_code = new Dictionary<byte, string> {
            { 0, "This device is working properly" },
            { 1, "This device is not configured correctly" },
            { 2, "Windows cannot load the driver for this device" },
            { 3, "The driver for this device might be corrupted, or your system may be running low on memory or other resources" },
            { 4, "This device is not working properly. One of its drivers or your registry might be corrupted" },
            { 5, "The driver for this device needs a resource that Windows cannot manage" },
            { 6, "The boot configuration for this device conflicts with other devices" },
            { 7, "Cannot filter" },
            { 8, "Driver loader for the device is missing" },
            { 9, "This device is not working properly because the controlling firmware is reporting the resources for the device incorrectly" },
            { 10, "Cannot start" },
            { 11, "Device failed" },
            { 12, "This device cannot find enough free resources that it can use" },
            { 13, "Windows cannot verify this device's resources." },
            { 14, "This device cannot work properly until you restart your computer" },
            { 15, "This device is not working properly because there is probably a re-enumeration problem" },
            { 16, "Windows cannot identify all the resources this device uses" },
            { 17, "This device is asking for an unknown resource type" },
            { 18, "Reinstall the drivers for this device" },
            { 19, "Failure using the VxD loader" },
            { 20, "Your registry might be corrupted" },
            { 21, "System failure. Try changing the driver for this device. If that does not work, see your hardware documentation. Windows is removing this device" },
            { 22, "This device is disabled" },
            { 23, "System failure: Try changing the driver for this device. If that doesn't work, see your hardware documentation" },
            { 24, "This device is not present, is not working properly, or does not have all its drivers installed" },
            { 25, "Windows is still setting up this device" },
            { 26, "Windows is still setting up this device" },
            { 27, "This device does not have valid log configuration" },
            { 28, "The drivers for this device are not installed" },
            { 29, "This device is disabled because the firmware of the device did not give it the required resources" },
            { 30, "This device is using an Interrupt Request (IRQ) resource that another device is using" },
            { 31, "This device is not working properly because Windows cannot load the drivers required for this device" }
        };

        private class NetInfo {
            public string[] Gateway { get; set; }
            public string[] IP { get; set; }
            public bool DHCP { get; set; } // DHCP Enabled/Disabled
            public string DHCPServer { get; set; }
            public string Hostname { get; set; }
            public string[] DNSServer { get; set; } // Order by priority
            public string[] Subnet { get; set; }
        }
        private sealed class Adapter : NetInfo {
            public string Type { get; set; }
            public string Avaliability { get; set; }
            public string Interface { get; set; }
            public string Name { get; set; }
            public string MAC { get; set; }
            public string Error { get; set; }
            public string Manufacture { get; set; }
            public string ConnectionName { get; set; }
            public string ConnectionStatus { get; set; }
            public bool NetEnabled { get; set; }
        }

        private string conn_status(byte status_id) {
            switch (status_id) {
                case 0:
                    return "Disconnected";
                case 1:
                    return "Connecting";
                case 2:
                    return "Connected";
                case 3:
                    return "Disconnecting";
                case 4:
                    return "Hardware Not Present";
                case 5:
                    return "Hardware Disabled";
                case 6:
                    return "Hardware Malfunction";
                case 7:
                    return "Media Disconnected";
                case 8:
                    return "Authenticating";
                case 9:
                    return "Authentication Succeeded";
                case 10:
                    return "Authentication Failed";
                case 11:
                    return "Invalid Address";
                case 12:
                    return "Credentials Required";
                default:
                    return "Other";
            }
        }
        private bool not_physical(string device_id) {
            string interf = device_id.Split('\\')[0].Trim();
            if (interf != "USB" && interf != "PCI") {
                return true;
            } else {
                return false;
            }
        }

        // Get Network connection info
        private Dictionary<byte, NetInfo> netinfo() {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");
            Dictionary<byte, NetInfo> data = new Dictionary<byte, NetInfo>(); // Use InterfaceIndex as key
            foreach (ManagementBaseObject cim_data in searcher.Get()) {
                NetInfo netinfo = new NetInfo {
                    Gateway = cim_data["DefaultIPGateway"] as string[],
                    IP = cim_data["IPAddress"] as string[],
                    DHCP = (bool) cim_data["DHCPEnabled"],
                    DHCPServer = Convert.ToString(cim_data["DHCPServer"]),
                    Hostname = Convert.ToString(cim_data["DNSHostName"]),
                    DNSServer = cim_data["DNSServerSearchOrder"] as string[],
                    Subnet = cim_data["IPSubnet"] as string[]
                };
                data.Add(Convert.ToByte(cim_data["InterfaceIndex"]), netinfo);
            }
            return data;
        }

        // Get Adapter info and combine with network connection info
        protected override void EndProcessing() {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter");
            List<Adapter> data = new List<Adapter>();
            Dictionary<byte, NetInfo> conn_info = netinfo();

            foreach (ManagementBaseObject cim_data in searcher.Get()) {
                if (not_physical(Convert.ToString(cim_data["PNPDeviceID"]))) { continue; }

                NetInfo conn = conn_info[Convert.ToByte(cim_data["InterfaceIndex"])];
                Adapter adapter = new Adapter {
                    // Adapter info
                    Type = adapt_type[Convert.ToByte(cim_data["AdapterTypeId"])],
                    Avaliability = avaliability[Convert.ToByte(cim_data["Availability"])],
                    Interface = Convert.ToString(cim_data["PNPDeviceID"]).Split('\\')[0].Trim(),
                    Name = Convert.ToString(cim_data["Name"]),
                    MAC = Convert.ToString(cim_data["MACAddress"]),
                    Error = error_code[Convert.ToByte(cim_data["ConfigManagerErrorCode"])],
                    Manufacture = Convert.ToString(cim_data["Manufacturer"]),
                    ConnectionName = Convert.ToString(cim_data["NetConnectionID"]),
                    ConnectionStatus = conn_status(Convert.ToByte(cim_data["NetConnectionStatus"])),
                    NetEnabled = (bool) cim_data["NetEnabled"],

                    // Network connection info
                    IP = conn.IP,
                    Gateway = conn.Gateway,
                    DHCP = conn.DHCP,
                    DHCPServer = conn.DHCPServer,
                    Hostname = conn.Hostname,
                    DNSServer = conn.DNSServer,
                    Subnet = conn.Subnet
                };
                data.Add(adapter);
            }
            WriteObject(data);
        }
    }
}

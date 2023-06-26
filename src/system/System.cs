using DeviceInfo.Tools;
using System;
using System.Collections.Generic;
using System.Management;
using System.Management.Automation;

namespace DeviceInfo.System {
    [Cmdlet(VerbsCommon.Get, "SystemInfo")]
    public class GetSystemInfo : PSCmdlet {

        private readonly Dictionary<byte, string> domain_role = new Dictionary<byte, string> {
            {0, "Standalone Workstation" },
            {1,"Member Workstation" },
            {2, "Standalone Server" },
            {3, "Member Server" },
            {4, "Backup Domain Controller" },
            {5, "Primary Domain Controller" }
        };

        private readonly Dictionary<byte, string> device_type = new Dictionary<byte, string> {
            {0, "Unspecified" },
            {1,"Desktop" },
            {2, "Mobile"},
            {3, "Workstation" },
            {4, "Enterprise Server" },
            {5, "SOHO Server" },
            {6, "Appliance PC" },
            {7, "Performance Server" },
            {8, "Maximum" }
        };

        private readonly Dictionary<byte, string> power_state = new Dictionary<byte, string> {
            {0, "Unknown" },
            {1, "Full Power" },
            {2, "Power Save - Low Power Mode" },
            {3, "Power Save - Standby" },
            {4, "Power Save - Unknown" },
            {5,"Power Cycle" },
            {6, "Power Off" },
            {7, "Power Save - Warning" },
            {8, "Power Save - Hibernate" },
            {9, "Power Save - Soft Off" }
        };

        private readonly Dictionary<byte, string> wakeup_type = new Dictionary<byte, string> {
            {0, "Reserved" },
            {1, "Other" },
            {2, "Unknown" },
            {3, "APM Timer" },
            {4, "Modem Ring" },
            {5, "LAN Remote" },
            {6, "Power Switch" },
            {7, "PCI PME#" },
            {8, "AC Power Restored" }
        };

        private sealed class System {
            public bool PartofDomain { get; set; }
            public string Domain { get; set; }
            public string DomainRole { get; set; }
            public string HostName { get; set; }
            public string Manufacturer { get; set; }
            public string Model { get; set; }
            public string Family { get; set; }
            public string TimeZone { get; set; }
            public bool Hypervisor { get; set; }
            public ushort LogicalCores { get; set; }
            public byte Processors { get; set; }
            public string DeviceType { get; set; }
            public string PowerState { get; set; }
            public string Username { get; set; }
            public string WakeupType { get; set; }
        };

        // Convert offset from minutes to hours
        private string timezone_conv(Int16 minutes) {
            float hours = (float) minutes / 60;
            if (hours > 0) { return $"+{hours}"; } else { return Convert.ToString(hours); }
        }

        protected override void EndProcessing() {
            List<System> data = new List<System>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");

            foreach (ManagementBaseObject cim_data in searcher.Get()) {
                System system = new System {
                    Domain = Convert.ToString(cim_data["Domain"]),
                    HostName = Convert.ToString(cim_data["DNSHostName"]),
                    Manufacturer = Convert.ToString(cim_data["Manufacturer"]),
                    Model = Convert.ToString(cim_data["Model"]),
                    Family = Convert.ToString(cim_data["SystemFamily"]),
                    PartofDomain = (bool) cim_data["PartOfDomain"],
                    DomainRole = domain_role[Convert.ToByte(cim_data["DomainRole"])],
                    TimeZone = timezone_conv(Convert.ToInt16(cim_data["CurrentTimeZone"])),
                    Hypervisor = (bool) cim_data["HypervisorPresent"],
                    LogicalCores = Convert.ToUInt16(cim_data["NumberOfLogicalProcessors"]),
                    Processors = Convert.ToByte(cim_data["NumberOfProcessors"]),
                    DeviceType = device_type[Convert.ToByte(cim_data["PCSystemType"])],
                    PowerState = power_state[Convert.ToByte(cim_data["PowerState"])],
                    Username = Convert.ToString(cim_data["UserName"]),
                    WakeupType = wakeup_type[Convert.ToByte(cim_data["WakeUpType"])]
                };
                data.Add(system);
            }
            WriteObject(data);
        }
    }

    [Cmdlet(VerbsCommon.Get, "OSInfo")]
    public class GetOSInfo : PSCmdlet {
        private sealed class OS {
            public ushort BuildNum { get; set; }
            public string SystemDrive { get; set; }
            public string LastBoot { get; set; }
            public string InstallTime { get; set; }
            public string Name { get; set; }
            public ushort OSCountryCode { get; set; }
            public string SN { get; set; }
            public string Version { get; set; }
        }

        protected override void EndProcessing() {
            List<OS> data = new List<OS>();
            Tool tools = new Tool();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");

            foreach (ManagementBaseObject cim_data in searcher.Get()) {
                OS os = new OS {
                    BuildNum = Convert.ToUInt16(cim_data["BuildNumber"]),
                    SystemDrive = Convert.ToString(cim_data["SystemDrive"]),
                    LastBoot = tools.StrToDate(Convert.ToString(cim_data["LastBootUpTime"])).ToString("yyyy/MM/dd HH:mm:ss"),
                    InstallTime = tools.StrToDate(Convert.ToString(cim_data["InstallDate"])).ToString("yyyy/MM/dd HH:mm:ss"),
                    Name = Convert.ToString(cim_data["Name"]).Split('|')[0],
                    OSCountryCode = Convert.ToUInt16(cim_data["CountryCode"]),
                    SN = Convert.ToString(cim_data["SerialNumber"]),
                    Version = Convert.ToString(cim_data["Version"])
                };
                data.Add(os);
            }
            WriteObject(data);
        }
    }
}
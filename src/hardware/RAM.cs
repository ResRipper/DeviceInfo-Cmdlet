using System;
using System.Collections.Generic;
using System.Management;
using System.Management.Automation;

namespace DeviceInfo.Hardware {

    [Cmdlet(VerbsCommon.Get, "RamInfo")]
    public class GetRamInfo : PSCmdlet {

        // RAM specifications
        private sealed class Ram {
            public string Bank { get; set; }
            public ushort Capacity { get; set; } // GB
            public ushort Speed { get; set; } // MHz
            public float Voltage { get; set; } // V
            public ushort DataWidth { get; set; }
            public string Locator { get; set; }
            public string Vendor { get; set; }
            public string PN { get; set; }
            public string SN { get; set; }
        }

        protected override void EndProcessing() {
            List<Ram> data = new List<Ram>();
            // Read from WMI
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            foreach (ManagementBaseObject cim_data in searcher.Get()) {
                Ram ram = new Ram {
                    Bank = Convert.ToString(cim_data["BankLabel"]),
                    Capacity = (ushort) (Convert.ToUInt64(cim_data["Capacity"]) / (1024 * 1024 * 1024)),
                    Speed = Convert.ToUInt16(cim_data["ConfiguredClockSpeed"]),
                    Voltage = (float) Convert.ToUInt16(cim_data["ConfiguredVoltage"]) / 1000,
                    DataWidth = Convert.ToUInt16(cim_data["DataWidth"]),
                    Locator = Convert.ToString(cim_data["DeviceLocator"]),
                    Vendor = Convert.ToString(cim_data["Manufacturer"]),
                    PN = Convert.ToString(cim_data["PartNumber"]),
                    SN = Convert.ToString(cim_data["SerialNumber"])
                };

                data.Add(ram);
            }
            WriteObject(data);
        }
    }
}

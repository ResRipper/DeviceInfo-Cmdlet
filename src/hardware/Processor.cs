using System;
using System.Collections.Generic;
using System.Management;
using System.Management.Automation;

namespace DeviceInfo.Hardware {

    [Cmdlet(VerbsCommon.Get, "ProcessorInfo")]
    public class GetProcessorInfo : PSCmdlet {

        // Processor specifications
        private sealed class Processor {
            public ushort AddressWidth { get; set; } // bit
            public float L2Cache { get; set; } // MB
            public float L3Cache { get; set; } // MB
            public string Name { get; set; }
            public float BaseClock { get; set; } // GHz
            public float Voltage { get; set; } // V
            public byte Stepping { get; set; }
        }

        protected override void EndProcessing() {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            List<Processor> data = new List<Processor>();
            foreach (ManagementBaseObject cim_data in searcher.Get()) {
                Processor processor = new Processor {
                    AddressWidth = Convert.ToUInt16(cim_data["AddressWidth"]),
                    L2Cache = (float) Convert.ToUInt16(cim_data["L2CacheSize"]) / 1024,
                    L3Cache = (float) Convert.ToUInt16(cim_data["L3CacheSize"]) / 1024,
                    Name = Convert.ToString(cim_data["Name"]),
                    BaseClock = (float) Convert.ToUInt16(cim_data["CurrentClockSpeed"]) / 1000,
                    Voltage = (float) Convert.ToUInt16(cim_data["CurrentVoltage"]) / 10,
                    Stepping = Convert.ToByte(cim_data["Stepping"])
                };
                data.Add(processor);
            }
            WriteObject(data);
        }
    }
}
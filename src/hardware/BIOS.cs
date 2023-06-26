using DeviceInfo.Tools;
using System;
using System.Collections.Generic;
using System.Management;
using System.Management.Automation;

namespace DeviceInfo.Hardware {
    [Cmdlet(VerbsCommon.Get, "BiosInfo")]
    public class GetBiosInfo : PSCmdlet {

        // BIOS Specifications
        private sealed class Bios {
            public string Version { get; set; }
            public string Manufacture { get; set; }
            public string SN { get; set; }
            public string ReleaseDate { get; set; }
        }

        protected override void EndProcessing() {
            Tool tools = new Tool();
            List<Bios> data = new List<Bios>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");

            foreach (ManagementBaseObject cim_data in searcher.Get()) {
                Bios bios = new Bios {
                    Version = Convert.ToString(cim_data["SMBIOSBIOSVersion"]),
                    Manufacture = Convert.ToString(cim_data["Manufacturer"]),
                    SN = Convert.ToString(cim_data["SerialNumber"]),
                    ReleaseDate = tools.StrToDate(Convert.ToString(cim_data["ReleaseDate"])).ToShortDateString()
                };
                data.Add(bios);
            }
            WriteObject(data);
        }
    }
}
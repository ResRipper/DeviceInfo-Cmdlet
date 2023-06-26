using DeviceInfo.Tools;
using System;
using System.Collections.Generic;
using System.Management;
using System.Management.Automation;

namespace DeviceInfo.Security {
    [Cmdlet(VerbsCommon.Get, "TpmInfo")]
    public class GetTpmInfo : PSCmdlet {
        private sealed class Tpm {
            public bool Activated { get; set; }
            public bool Enabled { get; set; }
            public bool Owned { get; set; }
            public string Manufacture { get; set; }
            public string ManufactureVer { get; set; }
            public string HighestSpec { get; set; }
        }

        protected override void EndProcessing() {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root/CIMv2/Security/MicrosoftTpm", "SELECT * FROM Win32_Tpm");
            List<Tpm> data = new List<Tpm>();
            Tool tools = new Tool();
            try {
                foreach (ManagementBaseObject cim_data in searcher.Get()) {
                    Tpm tpm = new Tpm {
                        Activated = (bool) cim_data["IsActivated_InitialValue"],
                        Enabled = (bool) cim_data["IsEnabled_InitialValue"],
                        Owned = (bool) cim_data["IsOwned_InitialValue"],
                        Manufacture = (string) cim_data["ManufacturerIdTxt"],
                        ManufactureVer = (string) cim_data["ManufacturerVersion"],
                        HighestSpec = tools.HighestNum((string) cim_data["SpecVersion"]).ToString()
                    };
                    data.Add(tpm);
                }
            } catch (ManagementException) {
                Console.WriteLine("Require administrator privilege.");
            } finally {
                WriteObject(data);
            }
        }
    }
}
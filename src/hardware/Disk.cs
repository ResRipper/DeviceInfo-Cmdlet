using System;
using System.Collections.Generic;
using System.Management;
using System.Management.Automation;

namespace DeviceInfo.Hardware {
    [Cmdlet(VerbsCommon.Get, "DiskInfo")]
    public class GetDiskInfo : PSCmdlet {
        // Storage device specifications
        private sealed class Disk {
            public string Model { get; set; }
            public string SN { get; set; }
            public string FirmwareVer { get; set; }
            public ushort Partitions { get; set; }
            public ushort Size { get; set; } // GB
            public string Interface { get; set; }
            public string MediaType { get; set; }
        }

        protected override void EndProcessing() {
            List<Disk> data = new List<Disk>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementBaseObject cim_data in searcher.Get()) {
                Disk disk = new Disk {
                    Model = Convert.ToString(cim_data["Model"]),
                    SN = Convert.ToString(cim_data["SerialNumber"]),
                    FirmwareVer = Convert.ToString(cim_data["FirmwareRevision"]),
                    Partitions = Convert.ToUInt16(cim_data["Partitions"]),
                    Size = (ushort) (Convert.ToUInt64(cim_data["Size"]) / (1024 * 1024 * 1024)),
                    Interface = Convert.ToString(cim_data["InterfaceType"]),
                    MediaType = Convert.ToString(cim_data["MediaType"])
                };
                data.Add(disk);
            }
            WriteObject(data);
        }
    }
}
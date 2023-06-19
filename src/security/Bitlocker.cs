using System;
using System.Collections.Generic;
using System.Management;
using System.Management.Automation;

namespace DeviceInfo.Security {
    [Cmdlet(VerbsCommon.Get, "BitlockerInfo")]
    public class GetBitlockerInfo : PSCmdlet {
        private readonly Dictionary<byte, string> conv_stat = new Dictionary<byte, string> {
            {0, "Decrypted" },
            {1, "Encrypted" },
            {2, "Encrypting" },
            {3, "Decrypting" },
            {4, "Encryption Paused" },
            {5, "Decryption Paused" }
        };

        private readonly Dictionary<byte, string> encrypt_method = new Dictionary<byte, string> {
            {0, "Decrypted" },
            {1, "AES 128 with Diffuser" },
            {2, "AES 256 with Diffuser" },
            {3, "AES 128" },
            {4, "AES 256" },
            {5, "Hardware Encryption" },
            {6, "XTS-AES 128" },
            {7, "XTS-AES 256 with Diffuser" }
        };

        private readonly Dictionary<byte, string> protect_stat = new Dictionary<byte, string> {
            {0, "Off" },
            {1, "On" },
            {2, "Unknown" }
        };

        private readonly Dictionary<byte, string> vol_type = new Dictionary<byte, string> {
            {0, "System"},
            {1, "Fixed Disk" },
            {2, "Removable" }
        };

        private sealed class Bitlocker {
            public string DriveLetter { get; set; }
            public string ConversionStatus { get; set; }
            public string EncryptionMethod { get; set; }
            public string ProtectionStatus { get; set; }
            public string VolumeType { get; set; }

        }

        protected override void EndProcessing() {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root/CIMV2/Security/MicrosoftVolumeEncryption", "SELECT * FROM Win32_EncryptableVolume");
            List<Bitlocker> data = new List<Bitlocker>();
            try {
                foreach (ManagementBaseObject cim_data in searcher.Get()) {
                    Bitlocker bitlocker = new Bitlocker {
                        DriveLetter = Convert.ToString(cim_data["DriveLetter"]),
                        ConversionStatus = conv_stat[Convert.ToByte(cim_data["ConversionStatus"])],
                        EncryptionMethod = encrypt_method[Convert.ToByte(cim_data["EncryptionMethod"])],
                        ProtectionStatus = protect_stat[Convert.ToByte(cim_data["ProtectionStatus"])],
                        VolumeType = vol_type[Convert.ToByte(cim_data["VolumeType"])]
                    };
                    data.Add(bitlocker);
                }
            } catch (ManagementException) {
                Console.WriteLine("Require administrator privilege.");
            } finally {
                WriteObject(data);
            }
        }
    }
}
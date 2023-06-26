// https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-gpsb/12867da0-2e4e-4a4f-9dc4-84a7f354c8d9

using Microsoft.Win32;
using System.Management.Automation;

namespace DeviceInfo.Security {
    [Cmdlet(VerbsCommon.Get, "UACInfo")]
    public class GetUACInfo : PSCmdlet {
        private class UAC {
            public bool Enabled { get; set; }
        }

        protected override void EndProcessing() {
            UAC uac = new UAC {
                Enabled = (bool) Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System").GetValue("EnableLUA")
            };
            WriteObject(uac);
        }
    }
}
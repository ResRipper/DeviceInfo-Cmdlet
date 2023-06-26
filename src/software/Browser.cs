using Microsoft.Win32;
using System.Management.Automation;

namespace DeviceInfo.Browser {

    // ------ Firefox ------
    class Firefox {
        public string Version { get; set; }
    }

    [Cmdlet(VerbsCommon.Get, "FirefoxInfo")]
    public class GetFirefoxInfo : PSCmdlet {
        protected override void EndProcessing() {
            string ver = "";
            if (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Mozilla\Mozilla Firefox") != null) {
                ver = (string) Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Mozilla\Mozilla Firefox").GetValue("CurrentVersion");
            }
            Firefox firefox = new Firefox {
                Version = ver
            };
            WriteObject(firefox);
        }
    }
}
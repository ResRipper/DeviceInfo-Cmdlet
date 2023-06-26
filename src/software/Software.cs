using Microsoft.Win32;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Threading.Tasks;

namespace DeviceInfo.Software {
    [Cmdlet(VerbsCommon.Get, "SoftwareList")]
    public class GetSoftwareList : PSCmdlet {

        private sealed class Software {
            public string Name { get; set; }
            public string Publisher { get; set; }
            public string Version { get; set; }
            public string Uninstall { get; set; }
            public string QuiteUninstall { get; set; }
            public string InstallDate { get; set; }
            public string URL { get; set; }
            public string Location { get; set; }
        }

        protected override void EndProcessing() {
            ConcurrentBag<Software> software_list = new ConcurrentBag<Software>();
            RegistryKey uninstall_list = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");

            Parallel.ForEach(uninstall_list.GetSubKeyNames(), prog_kname => {
                RegistryKey prog = uninstall_list.OpenSubKey(prog_kname);
                // Check if empty (System component)
                if (prog.GetValue("DisplayName") != null) {
                    Software software = new Software {
                        Name = (string) prog.GetValue("DisplayName"),
                        Publisher = (string) prog.GetValue("Publisher"),
                        Version = (string) prog.GetValue("DisplayVersion"),
                        Uninstall = (string) prog.GetValue("UninstallString"),
                        QuiteUninstall = (string) prog.GetValue("QuietUninstallString"),
                        InstallDate = (string) prog.GetValue("InstallDate"),
                        URL = (string) prog.GetValue("URLInfoAbout"),
                        Location = (string) prog.GetValue("InstallLocation")
                    };
                    software_list.Add(software);
                }
            });
            WriteObject(software_list);
        }
    }
}

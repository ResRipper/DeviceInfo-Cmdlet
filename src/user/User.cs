using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Management;
using System.Management.Automation;

namespace DeviceInfo.User {
    // Get Users
    [Cmdlet(VerbsCommon.Get, "UserInfo")]
    public class GetUserInfo : PSCmdlet {
        // Include system account if True
        [Parameter()]
        public bool Degraded { get; set; }

        private readonly Dictionary<ushort, string> account_type = new Dictionary<ushort, string> {
            {256, "Temporary duplicate account" },
            {512, "Normal account" },
            {2048, "Interdomain trust account" },
            {4096, "Workstation trust account" },
            {8192, "Server trust account" }
        };

        private sealed class User {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Caption { get; set; }
            public string Description { get; set; }
            public bool Disabled { get; set; }
            public string Domain { get; set; }
            public string FullName { get; set; }
            public bool Local { get; set; }
            public string SID { get; set; }
            public string Status { get; set; }
        }

        protected override void EndProcessing() {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_UserAccount");
            List<User> data = new List<User>();
            foreach (ManagementBaseObject cim_data in searcher.Get()) {
                if (Degraded || (Convert.ToString(cim_data["Status"]) == "OK")) {
                    User user = new User {
                        Type = account_type[Convert.ToUInt16(cim_data["AccountType"])],
                        Caption = Convert.ToString(cim_data["Caption"]),
                        Description = Convert.ToString(cim_data["Description"]),
                        Disabled = (bool) cim_data["Disabled"],
                        Domain = Convert.ToString(cim_data["Domain"]),
                        FullName = Convert.ToString(cim_data["FullName"]),
                        Local = (bool) cim_data["LocalAccount"],
                        SID = Convert.ToString(cim_data["SID"]),
                        Status = Convert.ToString(cim_data["Status"]),
                        Name = Convert.ToString(cim_data["Name"])
                    };
                    data.Add(user);
                }
            }
            WriteObject(data);
        }
    }

    // List local administrators' name
    [Cmdlet(VerbsCommon.Get, "LocalAdmin")]
    public class GetLocalAdmin : PSCmdlet {
        protected override void EndProcessing() {
            List<string> username = new List<string>();
            DirectoryEntry local_directory = new DirectoryEntry("WinNT://" + Environment.MachineName);
            DirectoryEntry admin_grp = local_directory.Children.Find("administrators", "group");
            foreach (object user_obj in (IEnumerable) admin_grp.Invoke("members", null)) {
                DirectoryEntry user = new DirectoryEntry(user_obj);
                username.Add(user.Name);
            }
            WriteObject(username);
        }
    }
}

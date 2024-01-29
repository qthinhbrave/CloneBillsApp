using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CloneBillsApp
{
    [RunInstaller(true)]
    public partial class InstallerEng : System.Configuration.Install.Installer
    {
        public InstallerEng()
        {
            InitializeComponent();
        }
        
        public override void Install(IDictionary stateSaver)
        {
            // This gets the named parameters passed in from your custom action
            string folder = Context.Parameters["TARGETDIR"];

            // This gets the "Authenticated Users" group, no matter what it's called
            SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);

            // Create the rules
            FileSystemAccessRule writerule = new FileSystemAccessRule(sid, FileSystemRights.Write, AccessControlType.Allow);
            
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                // Get your file's ACL
                DirectorySecurity fsecurity = Directory.GetAccessControl(folder);

                // Add the new rule to the ACL
                fsecurity.AddAccessRule(writerule);

                // Set the ACL back to the file
                Directory.SetAccessControl(folder, fsecurity);
            }
            
            GrantAccess(folder);
            base.Install(stateSaver);
        }
        /*

        public override void Commit(System.Collections.IDictionary savedState)
        {
            base.Commit(savedState);
            System.Diagnostics.Process.Start(Context.Parameters["TARGETDIR"].ToString() + "your main program.exe");
            //Remove temp files
            base.Dispose();
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            File.Delete(Context.Parameters["TARGETDIR"].ToString() + "your main program.InstallState");
            //Remove temp files
            base.Dispose();
        }
        */
        private static void GrantAccess(string file)
        {
            bool exists = System.IO.Directory.Exists(file);
            if (!exists)
            {
                DirectoryInfo di = System.IO.Directory.CreateDirectory(file);
                Console.WriteLine("The Folder is created Sucessfully");
            }
            else
            {
                Console.WriteLine("The Folder already exists");
            }
            DirectoryInfo dInfo = new DirectoryInfo(file);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    FileSystemRights.FullControl,
                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                    PropagationFlags.NoPropagateInherit,
                    AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);

        }
        
    }
}

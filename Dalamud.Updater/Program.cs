using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dalamud.Updater
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] strArgs = Environment.GetCommandLineArgs();
            if (!IsAdministrator())
            {
                // Restart and run as admin
                var exeName = Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                startInfo.Verb = "runas";
                if (strArgs.Length >= 2 && strArgs[1].Equals("-startup"))
                {
                    startInfo.Arguments = "-startup";
                }
                startInfo.WorkingDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
                Process.Start(startInfo);
                Application.Exit();
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form form = new FormMain();
            if (strArgs.Length >= 2 && strArgs[1].Equals("-startup"))
            {
                form.Opacity = 0;
                form.ShowInTaskbar = false;
                form.Show();
                form.Visible = false;
                form.Opacity = 1;
                form.ShowInTaskbar = true;
            } else
            {
                form.Show();
            }
            Application.Run();
        }

        static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

    }
}

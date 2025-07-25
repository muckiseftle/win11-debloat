using System;
using System.Security.Principal;
using System.Windows.Forms;

namespace DebloatWin11
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            if (!IsAdministrator())
            {
                MessageBox.Show("Dieses Programm muss als Administrator ausgeführt werden.");
                return;
            }
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}

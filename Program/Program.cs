using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Windows.Forms;

namespace DHLabel
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string[] args = Environment.GetCommandLineArgs();
            SingleInstanceController controller = new SingleInstanceController();
            controller.Run(args);
        }
    }

    public class SingleInstanceController : WindowsFormsApplicationBase
    {
        public SingleInstanceController()
        {
            IsSingleInstance = true;

            StartupNextInstance += this_StartupNextInstance;
        }

        void this_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
        {
            // e.CommandLine enthält im Gegensatz zu Environment.GetCommandLineArgs()
            // NICHT den Pfad der Exe, sondern nur die eigentlichen Argumente.
            if (MainForm is Form1 form && e.CommandLine.Count > 0)
            {
                form.LoadFile(e.CommandLine[0]);
            }
        }

        protected override void OnCreateMainForm()
        {
            string[] args = Environment.GetCommandLineArgs();
            MainForm = new Form1(args);
        }
    }
}

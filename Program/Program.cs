using System;
using System.Threading;
using System.Windows.Forms;

namespace DHLabel
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        static Mutex mutex = new Mutex(true, "{F79E3061-F8FE-46DD-BE4C-69F85A675116}");
        [STAThread]
        static void Main(string[] args)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1(args));
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show(Properties.Resources.AlreadyRunning, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

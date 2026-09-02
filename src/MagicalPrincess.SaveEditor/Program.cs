using System;
using System.Windows.Forms;
using MagicalPrincess.SaveEditor.Core;
using MagicalPrincess.SaveEditor.UI;

namespace MagicalPrincess.SaveEditor
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--headless")
            {
                HeadlessTest.Run(args.Length > 1 ? args[1] : null);
                return;
            }
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
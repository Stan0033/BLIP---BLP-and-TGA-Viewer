using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BLIP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //base.OnStartup(e);

            // Retrieve command-line arguments
            string[] args = Environment.GetCommandLineArgs();

            // Check if a file path is provided as an argument
            if (args.Length > 1)
            {
                if (File.Exists(args[1]))
                {
                    string filePath = args[1];

                    // Open the MainWindow and pass the file path to it
                    MainWindow mainWindow = new MainWindow(filePath);
                    mainWindow.Show();
                }
            }
            else
            {
                // Start without file if no file path is provided
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }
}

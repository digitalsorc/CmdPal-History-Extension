using System;
using System.Runtime.InteropServices;
using System.Threading;
using CmdPalHistoryExtension.CmdPal;

namespace CmdPalHistoryExtension
{
    /// <summary>
    /// Entry point for the CmdPal History Extension
    /// This handles COM activation for the extension
    /// </summary>
    internal class Program
    {
        // NOTE: The GUID 12345678-1234-1234-1234-123456789012 used in Package.appxmanifest
        // is a placeholder and MUST be replaced with a unique GUID before production deployment.
        // Generate a new GUID with: [guid]::NewGuid() in PowerShell or guidgen.exe

        [STAThread]
        static int Main(string[] args)
        {
            // Check if we're being registered as a COM server
            if (args.Length > 0 && args[0] == "-RegisterProcessAsComServer")
            {
                return RegisterAsComServer();
            }

            // Otherwise, run as standalone for testing
            Console.WriteLine("CmdPal History Extension");
            Console.WriteLine("This extension should be launched by CmdPal, not run directly.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return 0;
        }

        private static int RegisterAsComServer()
        {
            try
            {
                // Create the command provider
                var provider = new HistoryCommandProvider();
                provider.Initialize();

                // Keep the process alive
                using (var waitHandle = new ManualResetEvent(false))
                {
                    Console.WriteLine("CmdPal History Extension COM server started.");
                    Console.WriteLine("Press Ctrl+C to stop.");
                    
                    Console.CancelKeyPress += (sender, e) =>
                    {
                        e.Cancel = true;
                        waitHandle.Set();
                    };

                    waitHandle.WaitOne();
                }

                provider.Dispose();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting COM server: {ex.Message}");
                return 1;
            }
        }
    }
}

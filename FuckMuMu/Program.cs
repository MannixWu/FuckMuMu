using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace FuckMuMuApp
{
    internal static class Program
    {
        private static readonly string[] MuMuProcessNames = new string[]
        {
            "MuMuNxMain",
            "MuMuNxDevice",
            "MuMuVMMHeadless",
            "MuMuVMMSVC"
        };

        private static Process muMuProcess;
        private static ManualResetEvent exitEvent;

        private static int Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--watch")
            {
                return RunWatcherMode(args);
            }

            return RunLauncherMode(args);
        }

        private static int RunLauncherMode(string[] args)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string muMuPath = args.Length > 0 && !string.IsNullOrWhiteSpace(args[0])
                ? args[0]
                : Path.Combine(baseDirectory, "MuMuNxDevice.exe");
            string muMuArgs = args.Length > 1 ? args[1] : string.Empty;

            if (!File.Exists(muMuPath))
            {
                Console.Error.WriteLine("MuMu executable not found: " + muMuPath);
                return 1;
            }

            exitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += OnCancelKeyPress;

            Console.WriteLine("Starting MuMu: " + muMuPath + " " + muMuArgs);
            muMuProcess = StartProcess(muMuPath, muMuArgs);

            if (muMuProcess == null)
            {
                Console.Error.WriteLine("Failed to start MuMu.");
                return 1;
            }

            if (!StartWatcher())
            {
                Console.Error.WriteLine("Failed to start watcher mode.");
                return 1;
            }

            Console.WriteLine("MuMu started with PID " + muMuProcess.Id);
            Console.WriteLine("Launcher is running. Press Ctrl+C to exit.");
            exitEvent.WaitOne();
            return 0;
        }

        private static int RunWatcherMode(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: Launcher.exe --watch <launcherPid>");
                return 1;
            }

            int parentPid;
            if (!int.TryParse(args[1], out parentPid))
            {
                Console.Error.WriteLine("Invalid launcher PID: " + args[1]);
                return 1;
            }

            Console.WriteLine("Watcher started. Monitoring launcher PID " + parentPid + ".");

            try
            {
                Process parent = Process.GetProcessById(parentPid);
                parent.WaitForExit();
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Launcher process was not found; continuing cleanup.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Watcher error while monitoring launcher: " + ex.Message);
            }

            KillMuMuProcesses();
            Console.WriteLine("Watcher finished cleanup.");
            return 0;
        }

        private static bool StartWatcher()
        {
            try
            {
                string executablePath = Process.GetCurrentProcess().MainModule.FileName;
                string arguments = "--watch " + Process.GetCurrentProcess().Id;

                ProcessStartInfo startInfo = new ProcessStartInfo(executablePath, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(executablePath) ?? AppDomain.CurrentDomain.BaseDirectory
                };

                Process watcher = Process.Start(startInfo);
                return watcher != null;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to start watcher: " + ex.Message);
                return false;
            }
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            exitEvent.Set();
        }

        private static Process StartProcess(string fileName, string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(fileName, arguments)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(fileName) ?? AppDomain.CurrentDomain.BaseDirectory
                };

                return Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return null;
            }
        }

        private static void KillMuMuProcesses()
        {
            foreach (string name in MuMuProcessNames)
            {
                Process[] processes = Process.GetProcessesByName(name);
                foreach (Process process in processes)
                {
                    try
                    {
                        Console.WriteLine("Killing process " + name + " (PID " + process.Id + ")");
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("Failed to kill " + name + " (PID " + process.Id + "): " + ex.Message);
                    }
                }
            }
        }
    }
}

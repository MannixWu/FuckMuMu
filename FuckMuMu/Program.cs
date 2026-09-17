using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace FuckMuMuApp
{
    internal static class Program
    {
        private static readonly string[] MuMuProcessNames = new string[]
        {
            "MuMuNxMain",
            "MuMuNxDevice",
            "MuMuNxService",
            "MuMuVMMHeadless",
            "MuMuVMMSVC"
        };

        private static ManualResetEvent exitEvent;
        private static NotifyIcon trayIcon;

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
            Process muMuProcess = StartProcess(muMuPath, muMuArgs);

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

            SetupTrayIcon();
            Application.Run();
            return 0;
        }

        private static void SetupTrayIcon()
        {
            trayIcon = new NotifyIcon();

            try
            {
                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                trayIcon.Icon = Icon.ExtractAssociatedIcon(exePath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to load icon: " + ex.Message);
            }

            trayIcon.Text = "FuckMuMu - 运行中";
            trayIcon.Visible = true;

            MenuItem exitItem = new MenuItem("退出", OnExit);
            trayIcon.ContextMenu = new ContextMenu(new MenuItem[] { exitItem });

            Console.WriteLine("Launcher is running. Check system tray for icon.");
        }

        private static void OnExit(object sender, EventArgs e)
        {
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
                trayIcon = null;
            }
            exitEvent.Set();
            Application.Exit();
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

            Console.WriteLine("Watcher started. Monitoring launcher PID " + parentPid + " and MuMuNxDevice.");

            Process launcher;
            try
            {
                launcher = Process.GetProcessById(parentPid);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Launcher process was not found; continuing cleanup.");
                KillMuMuProcesses();
                Thread.Sleep(3000);
                KillMuMuProcesses();
                Console.WriteLine("Watcher finished cleanup.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Watcher error while getting launcher: " + ex.Message);
                KillMuMuProcesses();
                Thread.Sleep(3000);
                KillMuMuProcesses();
                Console.WriteLine("Watcher finished cleanup.");
                return 0;
            }

            DateTime graceEnd = DateTime.UtcNow.AddSeconds(10);
            bool muMuFound = false;

            while (!launcher.HasExited)
            {
                if (!muMuFound && DateTime.UtcNow >= graceEnd)
                {
                    Process[] procs = Process.GetProcessesByName("MuMuNxDevice");
                    if (procs.Length > 0)
                    {
                        foreach (Process p in procs) p.Dispose();
                        muMuFound = true;
                        Console.WriteLine("MuMuNxDevice detected, now monitoring both processes.");
                    }
                }

                if (muMuFound)
                {
                    Process[] current = Process.GetProcessesByName("MuMuNxDevice");
                    if (current.Length == 0)
                    {
                        Console.WriteLine("MuMuNxDevice has exited.");
                        foreach (Process p in current) p.Dispose();
                        break;
                    }
                    foreach (Process p in current) p.Dispose();
                }

                Thread.Sleep(1000);
            }

            try { launcher.Dispose(); } catch { }

            KillMuMuProcesses();
            Thread.Sleep(3000);
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
            Application.Exit();
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

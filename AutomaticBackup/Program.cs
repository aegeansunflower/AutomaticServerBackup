using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        try
        {
            string[] configLines = File.ReadAllLines("config.txt");
            if (configLines.Length < 2)
            {
                Console.WriteLine("Config file must have 2 lines: server folder path and start script name.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            string serverFolder = configLines[0].Trim();
            string startBat = Path.Combine(serverFolder, configLines[1].Trim());

            if (!Directory.Exists(serverFolder) || !File.Exists(startBat))
            {
                Console.WriteLine("Server folder or start script not found.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            var javaProcesses = Process.GetProcessesByName("java")
                .Where(p => !p.HasExited).ToArray();

            if (javaProcesses.Length > 0)
            {
                try
                {
                    foreach (var proc in javaProcesses)
                    {
                        if (proc.MainModule.FileName.StartsWith(serverFolder, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Please stop the server manually, then press any key to continue...");
                            Console.ReadKey();
                            break;
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Cannot detect server process properly. Make sure the server is stopped before backup.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }

            string backupRoot = Path.Combine(serverFolder, "Backups");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string backupPath = Path.Combine(backupRoot, timestamp);
            Directory.CreateDirectory(backupPath);

            string[] worlds = { "world", "world_nether", "world_end" };
            foreach (var world in worlds)
            {
                string source = Path.Combine(serverFolder, world);
                string dest = Path.Combine(backupPath, world);
                if (Directory.Exists(source))
                    CopyAll(new DirectoryInfo(source), new DirectoryInfo(dest));
                else
                    Console.WriteLine($"{world} not found. Skipping.");
            }

            Console.WriteLine("Backup complete. Starting server...");
            Process serverProcess = new Process();
            serverProcess.StartInfo.FileName = startBat;
            serverProcess.StartInfo.WorkingDirectory = serverFolder;
            serverProcess.StartInfo.UseShellExecute = true;
            serverProcess.Start();

            Console.WriteLine("Server restarted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {
        Directory.CreateDirectory(target.FullName);
        foreach (FileInfo file in source.GetFiles())
            file.CopyTo(Path.Combine(target.FullName, file.Name), true);
        foreach (DirectoryInfo dir in source.GetDirectories())
            CopyAll(dir, target.CreateSubdirectory(dir.Name));
    }
}

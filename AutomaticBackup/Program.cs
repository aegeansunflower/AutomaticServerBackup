using System;
using System.Diagnostics;
using System.IO;

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
                Console.ReadKey();
                return;
            }

            string serverFolder = configLines[0].Trim();
            string startBat = Path.Combine(serverFolder, configLines[1].Trim());

            if (!Directory.Exists(serverFolder) || !File.Exists(startBat))
            {
                Console.WriteLine("Server folder or start script not found.");
                Console.ReadKey();
                return;
            }

            bool serverRunning = false;
            try
            {
                foreach (var proc in Process.GetProcessesByName("java"))
                {
                    try
                    {
                        if (!proc.HasExited)
                            serverRunning = true;
                    }
                    catch { }
                }
            }
            catch { }

            if (serverRunning)
            {
                Console.WriteLine("Server appears to be running. Please stop it manually, then press any key to continue...");
                Console.ReadKey();
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
            Process.Start(new ProcessStartInfo
            {
                FileName = startBat,
                WorkingDirectory = serverFolder,
                UseShellExecute = true
            });

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

//oopsie
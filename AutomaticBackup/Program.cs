using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main()
    {
        try
        {
            string configFile = "config.txt";
            if (!File.Exists(configFile))
            {
                Console.WriteLine("Config file not found: " + configFile);
                return;
            }

            string[] configLines = File.ReadAllLines(configFile);
            if (configLines.Length < 2)
            {
                Console.WriteLine("Config file must have 2 lines: folder path and start script.");
                return;
            }

            string serverFolder = configLines[0].Trim();
            string startBatName = configLines[1].Trim();
            string startBat = Path.Combine(serverFolder, startBatName);

            if (!Directory.Exists(serverFolder))
            {
                Console.WriteLine("Server folder not found: " + serverFolder);
                return;
            }

            if (!File.Exists(startBat))
            {
                Console.WriteLine("Start script not found: " + startBat);
                return;
            }

            Console.WriteLine("Please stop the server manually before backup for safety.");
            Console.WriteLine("Press any key once the server is stopped...");
            Console.ReadKey();

            string backupRoot = Path.Combine(serverFolder, "Backups");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string backupPath = Path.Combine(backupRoot, timestamp);
            Directory.CreateDirectory(backupPath);

            string[] worlds = { "world", "world_nether", "world_end" };

            foreach (var world in worlds)
            {
                string sourcePath = Path.Combine(serverFolder, world);
                string destPath = Path.Combine(backupPath, world);

                if (Directory.Exists(sourcePath))
                    CopyAll(new DirectoryInfo(sourcePath), new DirectoryInfo(destPath));
                else
                    Console.WriteLine($"Skipping: {world} not found.");
            }

            Console.WriteLine("Backup complete! Restarting server...");

            Process.Start(new ProcessStartInfo
            {
                FileName = startBat,
                WorkingDirectory = serverFolder,
                UseShellExecute = true
            });

            Console.WriteLine("Server restarted.");
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

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

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

            Process server = new Process();
            server.StartInfo.FileName = startBat;
            server.StartInfo.WorkingDirectory = serverFolder;
            server.StartInfo.UseShellExecute = false;
            server.StartInfo.RedirectStandardInput = true;
            server.StartInfo.RedirectStandardOutput = true;
            server.StartInfo.RedirectStandardError = true;
            server.OutputDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            server.ErrorDataReceived += (s, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            server.Start();
            server.BeginOutputReadLine();
            server.BeginErrorReadLine();

            Console.WriteLine("Server started. Waiting 5 seconds...");
            Thread.Sleep(5000);

            Console.WriteLine("Stopping server for backup...");
            server.StandardInput.WriteLine("stop");
            server.WaitForExit();

            Console.WriteLine("Creating backup...");
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
            }

            Console.WriteLine("Backup complete. Restarting server...");

            server = new Process();
            server.StartInfo.FileName = startBat;
            server.StartInfo.WorkingDirectory = serverFolder;
            server.StartInfo.UseShellExecute = true;
            server.Start();

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

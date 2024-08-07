using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

class Underfetch
{
    static void Main(string[] args)
    {
        Console.WriteLine("underfetch - the worse neofetch");
        Console.WriteLine("==============================");

        DisplayLogo();
        DisplaySystemInfo();
    }

    static void DisplayLogo()
    {
        string[] logo = {
            "   _   _           _           __      _       _     ",
            "  | | | |_ __   __| | ___ _ __|  _ \\  | |_ ___| |__  ",
            "  | | | | '_ \\ / _` |/ _ \\ '__| | | | | __/ __| '_ \\ ",
            "  | |_| | | | | (_| |  __/ |  | |_| | | || (__| | | |",
            "   \\___/|_| |_|\\__,_|\\___|_|  |____/   \\__\\___|_| |_|",
            ""
        };

        Console.ForegroundColor = ConsoleColor.Cyan;
        foreach (string line in logo)
        {
            Console.WriteLine(line);
        }
        Console.ResetColor();
    }

    static void DisplaySystemInfo()
    {
        Console.WriteLine($"OS: {RuntimeInformation.OSDescription}");
        DisplayHostname();
        DisplayKernelVersion();
        DisplayCPUInfo();
        DisplayMemoryUsage();
        DisplayDiskUsage();
        DisplayUptime();
    }

    static void DisplayHostname()
    {
        try
        {
            string hostname = System.Net.Dns.GetHostName();
            Console.WriteLine($"Hostname: {hostname}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hostname: Unknown (Error: {ex.Message})");
        }
    }

    static void DisplayKernelVersion()
    {
        try
        {
            string kernelVersion = File.ReadAllText("/proc/version").Split(' ')[2];
            Console.WriteLine($"Kernel: {kernelVersion}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Kernel: Unknown (Error: {ex.Message})");
        }
    }

    static void DisplayCPUInfo()
    {
        try
        {
            string cpuInfo = File.ReadAllLines("/proc/cpuinfo")
                .FirstOrDefault(line => line.StartsWith("model name"))
                ?.Split(':')
                .Last()
                .Trim();

            int cpuCount = Environment.ProcessorCount;

            Console.WriteLine($"CPU: {cpuInfo} ({cpuCount} cores)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CPU: Unknown (Error: {ex.Message})");
        }
    }

    static void DisplayMemoryUsage()
    {
        try
        {
            string[] memInfo = File.ReadAllLines("/proc/meminfo");
            long totalMem = ParseMemInfo(memInfo, "MemTotal:") / 1024;
            long freeMem = ParseMemInfo(memInfo, "MemAvailable:") / 1024;
            long usedMem = totalMem - freeMem;

            Console.WriteLine($"Memory: {usedMem} MB / {totalMem} MB");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Memory: Unknown (Error: {ex.Message})");
        }
    }

    static void DisplayDiskUsage()
    {
        try
        {
            var driveInfo = new DriveInfo("/");
            long totalSize = driveInfo.TotalSize / (1024 * 1024 * 1024);
            long freeSpace = driveInfo.AvailableFreeSpace / (1024 * 1024 * 1024);
            long usedSpace = totalSize - freeSpace;

            Console.WriteLine($"Disk (/): {usedSpace} GB / {totalSize} GB");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Disk (/): Unknown (Error: {ex.Message})");
        }
    }

    static void DisplayUptime()
    {
        try
        {
            string uptime = File.ReadAllText("/proc/uptime").Split('.')[0];
            int seconds = int.Parse(uptime);
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            string formattedUptime = $"{t.Days}d {t.Hours}h {t.Minutes}m";

            Console.WriteLine($"Uptime: {formattedUptime}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Uptime: Unknown (Error: {ex.Message})");
        }
    }

    static long ParseMemInfo(string[] memInfo, string key)
    {
        return long.Parse(memInfo.First(line => line.StartsWith(key))
            .Split(':')
            .Last()
            .Trim()
            .Split(' ')
            .First());
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
        try
        {
            Console.WriteLine($"OS: {RuntimeInformation.OSDescription}");
            DisplayHostname();
            DisplayKernelVersion();
            DisplayCPUInfo();
            DisplayMemoryUsage();
            DisplayDiskUsage();
            DisplayUptime();
            DisplayNetworkInfo();
            DisplayUserInfo();
            DisplayProcesses();
        }
        catch (Exception ex)
        {
            LogError(ex);
        }
    }

    static void DisplayHostname()
    {
        string hostname = GetSystemInfo(() => System.Net.Dns.GetHostName(), "Hostname");
        Console.WriteLine($"Hostname: {hostname}");
    }

    static void DisplayKernelVersion()
    {
        string kernelVersion = GetSystemInfo(() => File.ReadAllText("/proc/version").Split(' ')[2], "Kernel");
        Console.WriteLine($"Kernel: {kernelVersion}");
    }

    static void DisplayCPUInfo()
    {
        string cpuInfo = GetSystemInfo(() =>
        {
            string info = File.ReadAllLines("/proc/cpuinfo")
                .FirstOrDefault(line => line.StartsWith("model name"))
                ?.Split(':')
                .Last()
                .Trim();

            int cpuCount = Environment.ProcessorCount;
            return $"{info} ({cpuCount} cores)";
        }, "CPU");

        Console.WriteLine($"CPU: {cpuInfo}");
    }

    static void DisplayMemoryUsage()
    {
        string memoryUsage = GetSystemInfo(() =>
        {
            string[] memInfo = File.ReadAllLines("/proc/meminfo");
            long totalMem = ParseMemInfo(memInfo, "MemTotal:") / 1024;
            long freeMem = ParseMemInfo(memInfo, "MemAvailable:") / 1024;
            long usedMem = totalMem - freeMem;

            return $"{usedMem} MB / {totalMem} MB";
        }, "Memory");

        Console.WriteLine($"Memory: {memoryUsage}");
    }

    static void DisplayDiskUsage()
    {
        string diskUsage = GetSystemInfo(() =>
        {
            var driveInfo = new DriveInfo("/");
            long totalSize = driveInfo.TotalSize / (1024 * 1024 * 1024);
            long freeSpace = driveInfo.AvailableFreeSpace / (1024 * 1024 * 1024);
            long usedSpace = totalSize - freeSpace;

            return $"{usedSpace} GB / {totalSize} GB";
        }, "Disk (/)");

        Console.WriteLine($"Disk (/): {diskUsage}");
    }

    static void DisplayUptime()
    {
        string uptime = GetSystemInfo(() =>
        {
            string uptimeSeconds = File.ReadAllText("/proc/uptime").Split('.')[0];
            int seconds = int.Parse(uptimeSeconds);
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return $"{t.Days}d {t.Hours}h {t.Minutes}m";
        }, "Uptime");

        Console.WriteLine($"Uptime: {uptime}");
    }

    static void DisplayNetworkInfo()
    {
        string networkInfo = GetSystemInfo(() =>
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            List<string> ipAddresses = new List<string>();

            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    var properties = networkInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation addr in properties.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            ipAddresses.Add(addr.Address.ToString());
                        }
                    }
                }
            }

            return string.Join(", ", ipAddresses);
        }, "IP Addresses");

        Console.WriteLine($"IP Addresses: {networkInfo}");
    }

    static void DisplayUserInfo()
    {
        string userInfo = GetSystemInfo(() =>
        {
            string userName = Environment.UserName;
            string userDomainName = Environment.UserDomainName;
            return $"{userName}@{userDomainName}";
        }, "User");

        Console.WriteLine($"User: {userInfo}");
    }

    static void DisplayProcesses()
    {
        string processes = GetSystemInfo(() =>
        {
            Process[] runningProcesses = Process.GetProcesses();
            return $"Running Processes: {runningProcesses.Length}";
        }, "Processes");

        Console.WriteLine(processes);
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

    static string GetSystemInfo(Func<string> infoGetter, string infoName)
    {
        try
        {
            return infoGetter();
        }
        catch (Exception ex)
        {
            LogError(ex, infoName);
            return "Unknown";
        }
    }

    static void LogError(Exception ex, string infoName = null)
    {
        string errorMessage = infoName == null ? ex.Message : $"{infoName}: {ex.Message}";
        Console.WriteLine($"Error: {errorMessage}");
    }
}
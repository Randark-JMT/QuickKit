using System.Collections.Generic;
using System.Linq;
using System.Management;
using QuickKit.Models;

namespace QuickKit.Services;

public static class DeviceInfoService
{
    public static IReadOnlyList<CpuInfo> GetCpuInfo()
    {
        var list = new List<CpuInfo>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, MaxClockSpeed FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString()?.Trim() ?? "—";
                var cores = obj["NumberOfCores"]?.ToString() ?? "—";
                var mhz = obj["MaxClockSpeed"]?.ToString();
                var mhzStr = "—";
                if (!string.IsNullOrEmpty(mhz) && uint.TryParse(mhz, out var k))
                    mhzStr = $"{k} MHz";
                list.Add(new CpuInfo { Name = name, Cores = cores, MaxSpeedMhz = mhzStr });
            }
        }
        catch { /* 忽略 WMI 错误 */ }
        return list;
    }

    public static MemoryInfo? GetMemoryInfo()
    {
        try
        {
            ulong total = 0, free = 0;
            using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
            {
                foreach (var obj in searcher.Get())
                {
                    if (obj["TotalPhysicalMemory"] != null && ulong.TryParse(obj["TotalPhysicalMemory"]?.ToString(), out var t))
                        total = t;
                    break;
                }
            }
            using (var searcher = new ManagementObjectSearcher("SELECT FreePhysicalMemory FROM Win32_OperatingSystem"))
            {
                foreach (var obj in searcher.Get())
                {
                    if (obj["FreePhysicalMemory"] != null && ulong.TryParse(obj["FreePhysicalMemory"]?.ToString(), out var f))
                        free = f * 1024; // KB -> bytes
                    break;
                }
            }
            return new MemoryInfo
            {
                TotalGb = total > 0 ? $"{total / (1024.0 * 1024 * 1024):F2} GB" : "—",
                AvailableGb = free > 0 ? $"{free / (1024.0 * 1024 * 1024):F2} GB" : "—"
            };
        }
        catch { return null; }
    }

    public static IReadOnlyList<GpuInfo> GetGpuInfo()
    {
        var list = new List<GpuInfo>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, DriverVersion, AdapterRAM FROM Win32_VideoController");
            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString()?.Trim() ?? "—";
                var driver = obj["DriverVersion"]?.ToString() ?? "—";
                var ramGb = "—";
                if (obj["AdapterRAM"] != null)
                {
                    var val = obj["AdapterRAM"];
                    if (val is ulong u && u > 0 && u < 0x1_0000_0000_0000) // 合理范围
                        ramGb = $"{u / (1024.0 * 1024 * 1024):F2} GB";
                }
                list.Add(new GpuInfo { Name = name, DriverVersion = driver, AdapterRamGb = ramGb });
            }
        }
        catch { }
        return list;
    }

    public static IReadOnlyList<DiskInfo> GetDiskInfo()
    {
        var list = new List<DiskInfo>();
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT DeviceID, VolumeName, Size, FreeSpace, FileSystem FROM Win32_LogicalDisk WHERE DriveType=3");
            foreach (var obj in searcher.Get())
            {
                var id = obj["DeviceID"]?.ToString() ?? "";
                var label = obj["VolumeName"]?.ToString()?.Trim() ?? "";
                var size = obj["Size"] != null && ulong.TryParse(obj["Size"]?.ToString(), out var s) ? s : 0UL;
                var free = obj["FreeSpace"] != null && ulong.TryParse(obj["FreeSpace"]?.ToString(), out var f) ? f : 0UL;
                var fs = obj["FileSystem"]?.ToString() ?? "—";
                list.Add(new DiskInfo
                {
                    DriveLetter = id,
                    VolumeLabel = string.IsNullOrEmpty(label) ? "—" : label,
                    TotalGb = size > 0 ? $"{size / (1024.0 * 1024 * 1024):F2} GB" : "—",
                    FreeGb = free > 0 ? $"{free / (1024.0 * 1024 * 1024):F2} GB" : "—",
                    FileSystem = fs
                });
            }
        }
        catch { }
        return list.OrderBy(d => d.DriveLetter).ToList();
    }
}

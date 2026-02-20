namespace QuickKit.Models;

/// <summary>CPU 信息</summary>
public sealed class CpuInfo
{
    public string Name { get; init; } = "";
    public string Cores { get; init; } = "";
    public string MaxSpeedMhz { get; init; } = "";
}

/// <summary>内存信息</summary>
public sealed class MemoryInfo
{
    public string TotalGb { get; init; } = "";
    public string AvailableGb { get; init; } = "";
}

/// <summary>显卡信息</summary>
public sealed class GpuInfo
{
    public string Name { get; init; } = "";
    public string DriverVersion { get; init; } = "";
    public string AdapterRamGb { get; init; } = "";
}

/// <summary>磁盘/分区信息</summary>
public sealed class DiskInfo
{
    public string DriveLetter { get; init; } = "";
    public string VolumeLabel { get; init; } = "";
    public string TotalGb { get; init; } = "";
    public string FreeGb { get; init; } = "";
    public string FileSystem { get; init; } = "";
}

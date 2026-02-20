namespace QuickKit.Models;

/// <summary>
/// 表示一个已安装的 Java 发行版。
/// </summary>
public class JavaDistribution
{
    /// <summary>显示名称，如 "Eclipse Temurin 21"</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>版本号，如 "21.0.1"</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>发行商/供应商，如 "Eclipse Adoptium"</summary>
    public string Vendor { get; set; } = string.Empty;

    /// <summary>安装路径</summary>
    public string HomePath { get; set; } = string.Empty;

    /// <summary>是否为当前 JAVA_HOME 指向的版本</summary>
    public bool IsActive { get; set; }

    /// <summary>运行时名称（如 java-runtime, jdk 等），用于区分 JDK/JRE</summary>
    public string RuntimeName { get; set; } = string.Empty;
}

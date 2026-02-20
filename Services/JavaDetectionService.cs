using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using QuickKit.Models;

namespace QuickKit.Services;

/// <summary>
/// 检测系统中已安装的 Java 发行版及当前生效的 JAVA_HOME。
/// </summary>
public static class JavaDetectionService
{
    private static readonly string[] CommonJavaRoots =
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Eclipse Adoptium"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Eclipse Foundation"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Amazon Corretto"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "BellSoft"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Zulu"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Semeru"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "jbr-17"), // JetBrains runtime
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java"),
    };

    /// <summary>
    /// 获取当前生效的 JAVA_HOME 路径（用户 + 系统，系统优先）。
    /// </summary>
    public static string? GetActiveJavaHome()
    {
        var machine = Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.Machine);
        var user = Environment.GetEnvironmentVariable("JAVA_HOME", EnvironmentVariableTarget.User);
        return !string.IsNullOrWhiteSpace(machine) ? machine : user;
    }

    /// <summary>
    /// 枚举所有检测到的 Java 发行版，并标记当前生效的版本。
    /// </summary>
    public static IReadOnlyList<JavaDistribution> DiscoverDistributions()
    {
        var activeHome = GetActiveJavaHome();
        var normalizedActive = activeHome != null ? Path.GetFullPath(activeHome.Trim()) : null;
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var list = new List<JavaDistribution>();

        void TryAdd(string homePath)
        {
            if (string.IsNullOrWhiteSpace(homePath) || !Directory.Exists(homePath))
                return;
            var full = Path.GetFullPath(homePath);
            if (!set.Add(full))
                return;

            var dist = TryCreateDistribution(full);
            if (dist != null)
            {
                dist.IsActive = string.Equals(full, normalizedActive, StringComparison.OrdinalIgnoreCase);
                list.Add(dist);
            }
        }

        // 1. 当前 JAVA_HOME
        if (!string.IsNullOrWhiteSpace(activeHome))
            TryAdd(activeHome);

        // 2. 常见安装根目录下的子目录
        foreach (var root in CommonJavaRoots)
        {
            if (!Directory.Exists(root))
                continue;
            try
            {
                foreach (var dir in Directory.EnumerateDirectories(root))
                    TryAdd(dir);
            }
            catch
            {
                // 忽略权限等错误
            }
        }

        // 3. PATH 中的 java.exe 所在目录的“上级 JAVA_HOME”（例如 bin 的父目录）
        try
        {
            var pathDirs = (Environment.GetEnvironmentVariable("PATH") ?? "")
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var dir in pathDirs)
            {
                var javaExe = Path.Combine(dir.Trim(), "java.exe");
                if (!File.Exists(javaExe))
                    continue;
                var candidateHome = Directory.GetParent(dir)?.FullName;
                if (!string.IsNullOrEmpty(candidateHome))
                    TryAdd(candidateHome);
            }
        }
        catch
        {
            // ignore
        }

        return list.OrderByDescending(x => x.IsActive).ThenBy(x => x.DisplayName).ToList();
    }

    private static JavaDistribution? TryCreateDistribution(string homePath)
    {
        var binPath = Path.Combine(homePath, "bin", "java.exe");
        if (!File.Exists(binPath))
            return null;

        var (version, vendor, runtimeName) = GetVersionInfo(binPath);
        var displayName = string.IsNullOrWhiteSpace(vendor)
            ? $"Java {version}"
            : $"{vendor} {version}";

        return new JavaDistribution
        {
            DisplayName = displayName,
            Version = version,
            Vendor = vendor ?? "",
            HomePath = homePath,
            RuntimeName = runtimeName ?? ""
        };
    }

    private static (string version, string? vendor, string? runtimeName) GetVersionInfo(string javaExePath)
    {
        try
        {
            var psi = new ProcessStartInfo(javaExePath)
            {
                Arguments = "-version",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p == null)
                return ("未知", null, null);

            var err = p.StandardError.ReadToEnd();
            p.WaitForExit(3000);

            // 常见格式: "openjdk version "21.0.1" ..." 或 "java version "1.8.0_401""
            var versionMatch = Regex.Match(err, @"(?:version|openjdk version)\s+""([^""]+)""", RegexOptions.IgnoreCase);
            var version = versionMatch.Success ? versionMatch.Groups[1].Value : "未知";

            string? vendor = null;
            if (err.IndexOf("Eclipse Adoptium", StringComparison.OrdinalIgnoreCase) >= 0) vendor = "Eclipse Temurin";
            else if (err.IndexOf("AdoptOpenJDK", StringComparison.OrdinalIgnoreCase) >= 0) vendor = "AdoptOpenJDK";
            else if (err.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) >= 0) vendor = "Microsoft";
            else if (err.IndexOf("Amazon", StringComparison.OrdinalIgnoreCase) >= 0) vendor = "Amazon Corretto";
            else if (err.IndexOf("Zulu", StringComparison.OrdinalIgnoreCase) >= 0) vendor = "Zulu";
            else if (err.IndexOf("BellSoft", StringComparison.OrdinalIgnoreCase) >= 0) vendor = "BellSoft Liberica";
            else if (err.IndexOf("Semeru", StringComparison.OrdinalIgnoreCase) >= 0) vendor = "IBM Semeru";
            else if (err.IndexOf("Oracle", StringComparison.OrdinalIgnoreCase) >= 0) vendor = "Oracle";
            else if (err.IndexOf("JetBrains", StringComparison.OrdinalIgnoreCase) >= 0) vendor = "JetBrains";

            string? runtimeName = null;
            var rtMatch = Regex.Match(err, @"([\w-]+)\s+version", RegexOptions.IgnoreCase);
            if (rtMatch.Success)
                runtimeName = rtMatch.Groups[1].Value;

            return (version, vendor, runtimeName);
        }
        catch
        {
            return ("未知", null, null);
        }
    }
}

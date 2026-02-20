# QuickKit CMake 配置与构建脚本（无需 Make：优先 Ninja，可选 VS 生成器）
# 用法: .\cmake-build.ps1 [configure|build|release|publish|clean|restore]
# 无 Ninja 时可用: winget install Ninja-build.Ninja  或 脚本会改用 Visual Studio 生成器

param(
    [Parameter(Position = 0)]
    [ValidateSet("configure", "build", "release", "publish", "clean", "restore", "")]
    [string]$Target = "configure"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$BuildDir = Join-Path $ProjectRoot "build"

function Test-Ninja {
    try {
        $null = Get-Command ninja -ErrorAction Stop
        return $true
    } catch {
        return $false
    }
}

function Invoke-Configure {
    if (Test-Ninja) {
        Write-Host "Configuring CMake (Ninja) ..." -ForegroundColor Cyan
        & cmake -G Ninja -B $BuildDir -S $ProjectRoot
    } else {
        Write-Host "Ninja not found. Install with: winget install Ninja-build.Ninja" -ForegroundColor Yellow
        Write-Host "Trying Visual Studio generator (no Ninja required) ..." -ForegroundColor Cyan
        $found = $false
        foreach ($gen in "Visual Studio 17 2022", "Visual Studio 16 2019") {
            if (Test-Path $BuildDir) { Remove-Item -Recurse -Force $BuildDir }
            & cmake -G $gen -A x64 -B $BuildDir -S $ProjectRoot 2>&1 | Out-Null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Configured with $gen (x64)." -ForegroundColor Green
                $found = $true
                break
            }
        }
        if (-not $found) {
            if (Test-Path $BuildDir) { Remove-Item -Recurse -Force $BuildDir -ErrorAction SilentlyContinue }
            Write-Host "VS generator failed. Install Ninja: winget install Ninja-build.Ninja" -ForegroundColor Red
            exit 1
        }
        return
    }
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "Configure done. Run .\cmake-build.ps1 build to compile." -ForegroundColor Green
}

function Invoke-Build {
    if (-not (Test-Path (Join-Path $BuildDir "build.ninja"))) {
        Write-Host "尚未配置，先执行 configure ..." -ForegroundColor Yellow
        Invoke-Configure
    }
    Write-Host "编译 (Debug x64) ..." -ForegroundColor Cyan
    & cmake --build $BuildDir --target quickkit-build
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "编译完成。" -ForegroundColor Green
}

function Invoke-Release {
    if (-not (Test-Path (Join-Path $BuildDir "build.ninja"))) { Invoke-Configure }
    Write-Host "编译 (Release x64) ..." -ForegroundColor Cyan
    & cmake --build $BuildDir --target release
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "完成。" -ForegroundColor Green
}

function Invoke-Publish {
    if (-not (Test-Path (Join-Path $BuildDir "build.ninja"))) { Invoke-Configure }
    Write-Host "发布 (win-x64-unpacked) ..." -ForegroundColor Cyan
    & cmake --build $BuildDir --target publish
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    $publishPath = Join-Path $ProjectRoot "bin\Release\net8.0-windows10.0.19041.0\win-x64\publish"
    Write-Host "完成。输出目录: $publishPath" -ForegroundColor Green
}

function Invoke-Clean {
    if (Test-Path $BuildDir) {
        Write-Host "清理 CMake 构建目录与 dotnet ..." -ForegroundColor Cyan
        & cmake --build $BuildDir --target quickkit-clean 2>$null
        Remove-Item -Recurse -Force $BuildDir -ErrorAction SilentlyContinue
    }
    & dotnet clean (Join-Path $ProjectRoot "QuickKit.csproj") -p:Platform=x64 2>$null
    Write-Host "清理完成。" -ForegroundColor Green
}

function Invoke-Restore {
    if (-not (Test-Path (Join-Path $BuildDir "build.ninja"))) { Invoke-Configure }
    & cmake --build $BuildDir --target restore
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "还原完成。" -ForegroundColor Green
}

switch ($Target) {
    "configure" { Invoke-Configure }
    "build"      { Invoke-Build }
    "release"    { Invoke-Release }
    "publish"    { Invoke-Publish }
    "clean"      { Invoke-Clean }
    "restore"    { Invoke-Restore }
    ""           { Invoke-Configure }
}

# QuickKit 使用 CMake 构建（无需 Make）

## 1. 安装 Ninja（推荐，一条命令）

```powershell
winget install Ninja-build.Ninja
```

安装后**重新开一个终端**，再执行下面的步骤。

## 2. 配置与构建

在项目根目录 `D:\_Code\QuickKit` 下：

| 命令 | 说明 |
|------|------|
| `.\cmake-build.ps1` 或 `.\cmake-build.ps1 configure` | 配置 CMake（生成 Ninja 或 VS 构建文件） |
| `.\cmake-build.ps1 build` | Debug x64 编译 |
| `.\cmake-build.ps1 release` | Release x64 编译 |
| `.\cmake-build.ps1 publish` | 发布为传统 x64 exe（win-x64-unpacked） |
| `.\cmake-build.ps1 clean` | 清理 |
| `.\cmake-build.ps1 restore` | 还原 NuGet 包 |

首次使用建议：

```powershell
.\cmake-build.ps1
.\cmake-build.ps1 build
# publish
.\cmake-build.ps1 publish
```

## 3. 没有安装 Ninja 时

- 脚本会提示：`winget install Ninja-build.Ninja`
- 若已安装 Visual Studio，脚本会尝试使用 **Visual Studio 生成器**，无需 Ninja 也能完成配置；之后用 `.\cmake-build.ps1 build` 等即可（由 CMake 调用 MSBuild）。

## 4. 配置是否有问题

- 配置成功：会看到 `Configure done` 或 `Configured with Visual Studio ...`，且项目下出现 `build` 目录。
- 若报错「未检测到 Ninja」且 VS 生成器也失败：请先执行 `winget install Ninja-build.Ninja`，关闭并重新打开终端后再运行 `.\cmake-build.ps1`。

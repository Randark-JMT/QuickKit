# QuickKit: 使用 Ninja 生成构建（无需 Make）
# 首次运行: .\cmake-build.ps1 configure  或  .\cmake-build.ps1
# 若无 Ninja，脚本会提示: winget install Ninja-build.Ninja
cmake_minimum_required(VERSION 3.16)
project(QuickKit NONE)

set(CSPROJ "${CMAKE_CURRENT_SOURCE_DIR}/QuickKit.csproj")

# 默认目标：Debug 编译 (x64)（避免使用保留名 all）
add_custom_target(quickkit-build
  COMMAND ${CMAKE_COMMAND} -E echo "Building QuickKit (Debug x64)..."
  COMMAND dotnet build "${CSPROJ}" -c Debug -p:Platform=x64
  WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
  COMMENT "dotnet build -c Debug -p:Platform=x64"
)

# Release 编译
add_custom_target(release
  COMMAND ${CMAKE_COMMAND} -E echo "Building QuickKit (Release x64)..."
  COMMAND dotnet build "${CSPROJ}" -c Release -p:Platform=x64
  WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
  COMMENT "dotnet build -c Release -p:Platform=x64"
)

# 发布为传统 x64 exe（自包含、无需 MSIX）
add_custom_target(publish
  COMMAND ${CMAKE_COMMAND} -E echo "Publishing QuickKit (win-x64-unpacked)..."
  COMMAND dotnet publish "${CSPROJ}" -c Release -p:PublishProfile=win-x64-unpacked -p:Platform=x64
  WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
  COMMENT "dotnet publish -c Release -p:PublishProfile=win-x64-unpacked"
)

# 清理（避免使用保留名 clean）
add_custom_target(quickkit-clean
  COMMAND dotnet clean "${CSPROJ}" -p:Platform=x64
  WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
  COMMENT "dotnet clean"
)

# 还原包
add_custom_target(restore
  COMMAND dotnet restore "${CSPROJ}"
  WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
  COMMENT "dotnet restore"
)

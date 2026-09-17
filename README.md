# FuckMuMu

MuMu 模拟器进程管理工具。启动 MuMu 并在退出时自动清理所有残留进程，附带系统托盘图标支持右键退出。

## 功能

- **Launcher 模式**：启动 MuMu 模拟器，系统托盘显示图标，右键可退出
- **Watcher 模式**：监听 launcher 和 MuMuNxDevice 进程，任一退出即触发清理
- **双轮清理**：第一轮 kill 后等待 3 秒再扫一轮，确保 MuMuNxMain 等残留进程也被清除
- **托盘图标**：系统托盘显示像素风图标，右键菜单提供退出选项

## 目录结构

- `FuckMuMu\FuckMuMu.csproj`
- `FuckMuMu\Program.cs`
- `build.cmd`
- `icon.ico` — 托盘及程序图标（多尺寸：16/32/48/64/128/256）

## 构建

如果当前系统没有 .NET SDK，可直接运行：

```bat
build.cmd
```

这会生成：

- `FuckMuMu\bin\FuckMuMu.exe`
- `FuckMuMu\bin\icon.ico`

如果安装了 .NET SDK，也可以使用：

```bat
dotnet build FuckMuMu\FuckMuMu.csproj -c Release
```

## 运行方式

将以下文件放在 MuMu 的 shell 目录下（如 `D:\Program Files\Netease\MuMu\nx_device\15.0\shell\`）：

- `FuckMuMu.exe`
- `icon.ico`

然后运行：

```bat
FuckMuMu.exe
```

如果你想使用自定义 MuMu 启动路径：

```bat
FuckMuMu.exe "D:\Program Files\Netease\MuMu\nx_device\15.0\shell\MuMuNxDevice.exe"
```

### 托盘图标

程序启动后会在系统托盘显示图标。右键点击托盘图标可选择"退出"，退出时会清理所有 MuMu 相关进程。

### watcher 模式

同一个程序也支持 watcher 模式（由 launcher 自动启动，无需手动调用）：

```bat
FuckMuMu.exe --watch <launcherPid>
```

Watcher 会同时监听 launcher 进程和 MuMuNxDevice 进程，任一退出即触发清理流程。

## 清理的进程

退出时会关闭以下进程：

- `MuMuNxMain`
- `MuMuNxDevice`
- `MuMuNxService`
- `MuMuVMMHeadless`
- `MuMuVMMSVC`

如果实际进程名不同，请修改 `FuckMuMu\Program.cs` 中的 `MuMuProcessNames` 列表。

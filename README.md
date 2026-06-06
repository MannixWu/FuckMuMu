# FuckMuMu

本项目包含一个控制台程序：

- `FuckMuMu`：启动 MuMu 模拟器并保持当前程序运行；当 `FuckMuMu` 退出时，会由同一个程序的 watcher 模式负责关闭 MuMu 相关进程。

## 目录结构

- `FuckMuMu\FuckMuMu.csproj`
- `FuckMuMu\Program.cs`
- `build.cmd`

## 构建

如果当前系统没有 .NET SDK，可直接运行：

```bat
build.cmd
```

这会生成：

- `FuckMuMu\bin\FuckMuMu.exe`

如果安装了 .NET SDK，也可以使用：

```bat
dotnet build FuckMuMu\FuckMuMu.csproj -c Release
```

## 运行方式

将 `FuckMuMu.exe` 放在 `C:\Program Files\Netease\MuMuPlayer\nx_device\12.0\shell\` 同级目录下后运行：

```bat
FuckMuMu\bin\FuckMuMu.exe
```

如果你想使用自定义 MuMu 启动路径：

```bat
FuckMuMu\bin\FuckMuMu.exe "C:\Program Files\Netease\MuMuPlayer\nx_device\12.0\shell\MuMuNxDevice.exe"
```

### watcher 模式

同一个程序也支持 watcher 模式：

```bat
FuckMuMu\bin\FuckMuMu.exe --watch <launcherPid>
```

这个模式由 FuckMuMu 启动，并在 launcher 进程退出后负责清理 MuMu 进程。

## 注意

- 当前程序会关闭以下进程名：`MuMuNxMain`, `MuMuNxDevice`, `MuMuVMMHeadless`, `MuMuVMMSVC`。
- 如果实际进程名不同，请修改 `FuckMuMu\Program.cs` 中的 `MuMuProcessNames` 列表。

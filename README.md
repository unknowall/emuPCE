## **ePceCD - A lightweight PC Engine CD / TurboGrafx-16 CD Emulator**  

![License](https://img.shields.io/badge/license-MIT-blue) ![GitHub Release](https://img.shields.io/github/v/release/unknowall/emuPCE?label=Release) ![Language](https://img.shields.io/github/languages/top/unknowall/emuPCE) ![Build Status](https://img.shields.io/badge/build-passing-brightgreen)

<details>
<summary><h3> 🌐 English Version</h3></summary>
  
A lightweight emulator for retro gaming enthusiasts, supporting CD-ROM games and classic ROM formats with modern graphical enhancements to revive the pixel art charm of your childhood memories!

## Key Features 🎮
- **Instant Save/Load**: Save and load game progress anytime.
- **Multiple Renderer Support**: Dynamically switch between D2D/D3D/OpenGL/Vulkan renderers to adapt to different hardware configurations.
- **ReShade Integration**: Post-processing effects (xBR/JINC upscaling) available for OpenGL/Vulkan/D3D pipelines.
- **Resolution Scaling**: Enhanced visual experience through xBR/JINC algorithms.
- **Memory Tools**: Provides memory editing and search capabilities.
- **Cheat Code Support**: Unlock hidden content or adjust game difficulty via cheat codes.

> <b>Note:</b> This English documentation became available from Beta version 0.0.2 onwards.

_图3：ePceCD 主界面展示_<br>
![epcecd1](https://github.com/user-attachments/assets/95e6e618-cde2-42c0-8db5-6d04de6f4385)

_图2：使用ReShade_<br>
![epcecd2](https://github.com/user-attachments/assets/d978b804-fdfa-49a9-a625-fc813acca6ba)

### How to Use 🛠️

#### 1. Setting Up CD BIOS 🔑
> **Note**: BIOS files are required only for CD-ROM format games, not for standard ROMs.  
> **Legal Disclaimer**: Due to copyright restrictions, the emulator does not include BIOS files. You must obtain them legally from your own console.

- Example BIOS files: `Super CD-ROM System (Japan) (v3.0).pce` or `BIOS.pce`
- Place the file in the emulator's `bios` folder:
```
/ePceCD
├── bios/
│   └── Super CD-ROM System (Japan) (v3.0).pce
├── saves/
└── ePceCD.exe
```

#### 2. Using ReShade 🎨
- Available in OpenGL/Vulkan rendering modes
- > D3D requires separate ReShade installation
- Press the **Home key** to open the ReShade settings interface
- Load preset shader files (multiple options available)

#### 4. Control Settings ⌨️🎮
- Keyboard mapping configuration is done through the File menu
- Gamepads are plug-and-play compatible

## Frequently Asked Questions ❓

### Q: Why can't I start a CD-ROM game?
A: Please verify:
1. BIOS file is correctly configured
2. Game image format is valid (e.g., `.bin/.cue` or `.img/.cue`)

### Q: How do I get more ReShade shaders?
A: Download shader files from [ReShade Official Website](https://reshade.me/) and place them in the `reshade/` folder:
```
/ePceCD
├── reshade/
│   └── Put shaders here
├── saves/
└── ePceCD.exe
```

### Q: How to improve visual quality?
A: Press F11 multiple times. Combine with Home key to enable ReShade enhancements.

### Q: How to fix audio sync issues?
A: Try adjusting audio buffer size or switching audio output devices.

### Q: Does it support all PC Engine/TurboGrafx-16 ROM formats?
A: Yes, all ROM formats are supported. CD-ROM requires proper BIOS configuration.

### Q: Is cross-platform support available?
A: Currently Windows-only. Linux/macOS support planned via .NET MAUI or Avalonia frameworks.

## Compilation Guide

1. Project built with .NET 8.0 framework
2. SDL definitions already included in code - place SDL2 DLL in output directory
3. OpenGL: Install OpenGL.NET NuGet package (.NET 4.7 framework with compatibility issues)  
   Or manually add OpenGL.dll for .NET 8.0 builds
5. Vulkan: Use vk NuGet package or manual vk.dll dependency
6. For versions below .NET 8.0, modify project file manually

## How to Contribute 🤝
We welcome code submissions, issue reports, and documentation improvements!
- **Submit Issues**: Report bugs or suggest features at [Issues](https://github.com/unknowall/emuPCE/issues)
- **Pull Requests**: Fork this project and submit PRs
- **Localization**: Help translate UI text and documentation

# Downloads 📥

- **Lite Version (1.51 MB)**: Core functionality only, ideal for quick testing
- **Full Version (8.02 MB)**: Includes all features including ReShade integration
- **ControllerDB Database**: Optional download for enhanced controller recognition

[Download Latest Version](https://github.com/unknowall/emuPCE/releases)

</details>

为怀旧游戏爱好者打造的轻量级 PCE 模拟器，支持 CD-ROM 游戏与经典 ROM 格式，搭配现代画质增强技术，唤醒童年记忆中的像素魅力！

## 主要功能 🎮
- **即时存档/读档**: 随时保存和加载游戏进度。
- **多渲染器支持**: 动态切换 D2D、D3D、OpenGL、Vulkan 渲染器，适配不同硬件配置。
- **ReShade 集成**: D3D、OpenGL、Vulkan 支持 ReShade 后处理效果，增强画质。
- **分辨率调节**: 支持通过xBR,JINC提升视觉体验。
- **内存工具**: 提供内存编辑和搜索功能。
- **金手指支持**: 开启作弊功能，解锁隐藏内容或调整游戏难度。

<b>the english version is available starting from Beta 0.0.2 </b>

**项目已同步至 Gitee 以及 Gitcode 国内用户可优先访问以加速下载。镜像仓库自动同步更新，确保内容一致**

_图3：ePceCD 主界面展示_<br>
![epcecd1](https://github.com/user-attachments/assets/95e6e618-cde2-42c0-8db5-6d04de6f4385)

_图2：使用ReShade_<br>
![epcecd2](https://github.com/user-attachments/assets/d978b804-fdfa-49a9-a625-fc813acca6ba)

### 如何使用 🛠️

#### 1. 设置 CD BIOS 🔑
> **注意**: 只有CD-ROM格式的游戏需要BIOS，普通ROM无需BIOS。<br>
> **注意**: 由于法律限制，模拟器不附带 BIOS 文件，请自行获取合法 BIOS。
- 比如从你的主机中提取 BIOS 文件（如： Super CD-ROM System (Japan) (v3.0).pce 或 BIOS.pce）
- 将文件放入模拟器的 `bios` 文件夹中：
- /ePceCD
- ├── bios/
- │ └── Super CD-ROM System (Japan) (v3.0).pce
- ├── saves/
- └── ePceCD.exe

#### 2. 使用 ReShade 🎨
- ReShade 在 OpenGL、Vulkan 渲染模式下可用
- >D3D需额外安装reShade。
- 按 **Home 键** 打开 ReShade 设置界面。
- 可加载预设的 Shader 文件（已有多款可供选择）。
  
#### 4. 控制设置 ⌨️🎮
- 键盘设置在文件菜单中完成。
- 手柄无需额外设置，即插即用。
  
## 常见问题 ❓

### Q: 为什么无法启动光盘游戏？
A: 请确保：
1. 已正确设置 BIOS 文件。
2. 游戏镜像文件格式正确（如 `.bin/.cue` 或 `.img/.cue`）。

### Q: 如何获取更多 ReShade Shader？
A: 访问 [ReShade 官方网站](https://reshade.me/) 下载 Shader 文件，并将其放入 `reshade/` 文件夹中。
- /ePceCD
- ├── reshade/
- │ └── 放在这里
- ├── saves/
- └── ePceCD.exe

### Q: 如何获得更好的画面
A: 多按几下F11，建议配合home键选择ReShade增强画质

### Q: 如何解决音效不同步的问题？
A: 尝试调整音频缓冲区大小，或更换音频输出设备。

### Q: 是否支持 PC Engine / TurboGrafx-16 的所有ROM格式？
A: 是的，支持所有ROM格式，CD-ROM 需使用正确的BIOS。

### Q: 是否支持跨平台？
A: 目前仅支持 Windows，未来计划通过 .NET MAUI 或 Avalonia 实现 Linux/macOS 支持。

## 如何编译
1. 项目是.net 8.0 框架
2. SDL 声明文件已经在代码中包含，把SDL2的DLL放到生成目录中即可
3. OpenGL 可以安装 OpenGL.NET NuGet包(.net 4.7 框架，存在兼容性问题)<br>
   或手动添加依赖项使用 OpenGL.dll (.net 8.0 编译)
5. Vulkan 使用 vk NuGet包，或手动添加依赖项使用 vk.dll
6. 如果使用低于 .net 8.0 框架，可手动修改项目文件

## 如何贡献 🤝
欢迎为 ePceCD 提交代码、报告问题或改进文档！以下是参与方式：
- **提交 Issue**: 在 [Issues](https://github.com/unknowall/emuPCE/issues) 页面报告问题或提出建议。
- **提交 PR**: Fork 本项目并提交 Pull Request。
- **翻译支持**: 如果你熟悉其他语言，欢迎帮助翻译 README 或 UI 文本。

# 下载 📥

- **轻量版 (1.51 MB)**: 仅包含核心功能，适合快速体验。
- **完整版 (8.02 MB)**: 包含所有功能（如 ReShade 集成）。
- **ControllerDB 数据库**: 可选下载，自动识别更多手柄外设。

[点击这里下载最新版本](https://github.com/unknowall/emuPCE/releases)


# PosePad

[English](README.md) | [日本語](README.ja-JP.md)

PosePad 是一个适用于最终幻想 XIV 的 Dalamud 插件，用来把常用姿势，例如站姿 1、2、3，坐姿 1、2、3，以及情感动作、表情动作整理成一个直观的按钮面板，方便在 GPose、截图、演出和角色扮演场景中配合 Mod 快速使用。很多动作 Mod 都依赖这些姿势，而每次手动输入编号会非常麻烦，PosePad 的目标就是把这件事变得更直接。现在它也支持可选的 Penumbra 联动，可以把当前启用的动作 Mod 读取到单独的“Mod 动作”页签里直接点用。

## 主要功能

- 常用姿势按钮面板，便于快速切换站姿、坐姿、躺姿等常见动作
- 直接读取游戏内数据的情感动作页签
- 单独拆分的表情页签
- 情感动作与表情的收藏功能
- 可选的 Penumbra 联动，可把当前启用的动作 Mod 读取到“Mod 动作”页签
- 搜索与过滤
- 最近使用情感动作
- 一键取消动作，恢复角色默认状态
- 适配拍照模式的窗口显示与自动打开选项
- 支持英文、简体中文、日文界面

## 命令

- `/posepad`
- `/posspad`

## 当前范围

当前版本包含：

- 本地动作按钮面板
- 本地配置保存
- 在拍照模式下更安全的 timeline 动作播放
- 可选的 Penumbra 联动，可读取当前启用的情感动作 Mod 并提供一键使用入口

当前版本不包含：

- 所有 Penumbra 姿势变体的稳定细分识别
- 对 Brio 的运行时依赖

## 本地开发

当前仓库按下面这套环境对齐：

- `Dalamud.NET.Sdk 15.0.0`
- Dalamud API `15`
- 默认开发 hooks 路径：`$(AppData)\XIVLauncherCN\addon\Hooks\dev`

构建命令：

```powershell
dotnet build
dotnet build -c Release
```

Release 打包产物输出位置：

```text
bin/Release/PosePad/latest.zip
```

## Dalamud 开发加载方式

1. 输入 `/xlsettings`
2. 进入 `Experimental`
3. 添加 `bin/Debug/PosePad.dll` 的完整路径
4. 输入 `/xlplugins`
5. 在开发插件列表中启用 PosePad

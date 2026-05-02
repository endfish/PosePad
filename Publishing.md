# PosePad 发布说明

这份文档只讲实际可执行的 Dalamud 发布路线，重点是“如何让玩家通过额外仓库安装你的插件”，以及“以后多个插件怎么统一维护”。

## 一、两种发布路线

Dalamud 插件通常有两种发布方式：

1. 提交到官方仓库 `goatcorp/DalamudPluginsD17`
2. 自己维护一个 `custom repository`

参考资料：

- Dalamud custom repositories  
  https://dalamud.dev/plugin-publishing/custom-repositories/
- Dalamud plugin metadata  
  https://dalamud.dev/plugin-development/plugin-metadata/
- Dalamud project layout  
  https://dalamud.dev/plugin-development/project-layout/
- DalamudPluginsD17  
  https://github.com/goatcorp/DalamudPluginsD17
- DalamudPackager  
  https://github.com/goatcorp/DalamudPackager

## 二、官方仓库发布

如果你想进卫月默认官方插件仓库，路线是：

1. 保持插件源码仓库公开
2. 用 `Release` 构建
3. 按官方 SDK 规范维护项目结构
4. 准备插件图标
5. 向 `DalamudPluginsD17` 提交 manifest
6. 等待审核

官方仓库 README 明确提到：

- 新插件通常建议先提交到 `testing/live/<plugin>/manifest.toml`
- D17 manifest 里会包含：
  - `repository`
  - `commit`
  - `owners`
  - `maintainers`
  - `project_path`
  - `changelog`
- 插件图片目录里需要 `icon.png`

这条路线适合以后你想正式公开分发、并走审核流程的时候使用。

## 三、自建额外仓库发布

如果你想让玩家先通过“额外仓库”安装，这是最快的路线。

官方文档确认：

- 额外仓库地址本质上是一个公开可访问的 JSON URL
- 这个 URL 返回一个数组
- 数组里的每一项就是一个插件条目
- 一个仓库 URL 可以同时返回多个插件

也就是说，你要做的核心只有两件事：

1. 提供一个公开可下载的 `latest.zip`
2. 提供一个公开可访问的 `repo.json`

玩家安装流程通常是：

1. 打开 `/xlsettings`
2. 进入 `Experimental`
3. 添加你的仓库 URL
4. 打开插件安装器
5. 安装你的插件

## 四、repo.json 需要什么

官方 custom repository 文档说明，条目支持普通 manifest 字段，以及仓库额外字段，比如：

- `DownloadLinkInstall`
- `DownloadLinkUpdate`
- `IconUrl`
- `ImageUrls`
- `IsHide`
- 可选 testing 字段，例如 `DownloadLinkTesting`

一个最小可用的单插件 `repo.json` 结构大概是：

```json
[
  {
    "Author": "Endfish",
    "Name": "PosePad",
    "InternalName": "PosePad",
    "AssemblyVersion": "0.1.0.0",
    "Description": "Quick pose and emote buttons for GPose, screenshots, and performances.",
    "ApplicableVersion": "any",
    "RepoUrl": "https://github.com/endfish/PosePad",
    "DalamudApiLevel": 15,
    "Punchline": "Quick pose and emote buttons for GPose and performances.",
    "IsHide": false,
    "IsTestingExclusive": false,
    "DownloadLinkInstall": "https://example.com/latest.zip",
    "DownloadLinkUpdate": "https://example.com/latest.zip",
    "LastUpdate": "1710000000"
  }
]
```

其中最关键的是：

- `DownloadLinkInstall`
- `DownloadLinkUpdate`

它们都必须指向真实可下载的 zip。

## 五、当前这个 PosePad 仓库已经具备什么

现在这个项目在 `Release` 构建后，已经会产出：

```text
bin/Release/PosePad/latest.zip
```

所以发布真正缺的并不是打包，而是“分发层”：

- 把 zip 放到公开地址
- 把 `repo.json` 放到公开地址
- 把仓库 URL 发给用户

## 六、PosePad 当前最推荐的做法

对于 PosePad 单插件，最简单的方案是：

1. 源码放在：
   - `https://github.com/endfish/PosePad`
2. 生成并上传 `latest.zip`
3. 准备一个 `repo.json`
4. 让玩家添加这个 `repo.json` 的 raw 地址

如果 `repo.json` 放在 GitHub 仓库根目录，那么 URL 通常是：

```text
https://raw.githubusercontent.com/endfish/PosePad/main/repo.json
```

## 七、以后多个插件怎么维护

官方文档已经明确说了，一个仓库 URL 可以返回多个插件，所以未来你最适合的结构是：

1. 每个插件保持独立源码仓库
   - `endfish/PosePad`
   - `endfish/AnotherPlugin`
   - `endfish/WhateverPlugin`
2. 另外单独建一个聚合仓库，例如：
   - `endfish/DalamudPlugins`
3. 在这个聚合仓库里维护统一的：
   - `README.md`
   - `repo.json`
4. `repo.json` 里每个条目分别指向各自插件仓库的 zip 下载地址

这个方案的优点是：

- 每个插件源码、issue、release 各自独立
- 玩家只需要添加一次你的仓库地址
- 以后你新增第二个第三个插件，只要往 `repo.json` 里增加新条目

## 八、PosePad 实际发布检查表

当你要发布 PosePad 一个新版本时：

1. 修改 `PosePad.csproj` 里的版本号
2. 执行：

```powershell
dotnet build -c Release
```

3. 确认产物存在：

```text
bin/Release/PosePad/latest.zip
```

4. 把这个 zip 放到公开地址
5. 更新 `repo.json` 里的版本、下载地址、时间戳
6. 提交并推送仓库
7. 把 `repo.json` 的 raw 地址发给用户

## 九、当前建议

对你现在这个阶段，最合适的是：

- 先走自建 custom repository
- 先把 PosePad 挂到你自己的 `DalamudPlugins` 聚合仓库里
- 以后如果某个插件成熟了，再考虑提交官方 `DalamudPluginsD17`

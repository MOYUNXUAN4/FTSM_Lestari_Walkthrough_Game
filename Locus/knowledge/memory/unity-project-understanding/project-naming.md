---
id: kd_eb26fcfc-7820-40f9-99bb-a49b2f494d22
type: memory
path: unity-project-understanding/project-naming.md
title: project-naming
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
inheritAiConfig: true
createdAt: 1780073247191
updatedAt: 1780073427445
---

# project-naming

## Summary
项目对外名称、包名与工程显示名已统一：Unity 名称为 `FTSM Lestari Walkthrough`，application identifier 为 `com.ftsm.lestariwalkthrough`，解决方案文件名已同步更新。

<!-- locus:body:start -->
- 项目当前对外名称已统一为 `FTSM Lestari Walkthrough`。
- `ProjectSettings/ProjectSettings.asset` 中的 `productName`、`projectName`、`metroPackageName`、`metroApplicationDescription` 已改为 `FTSM Lestari Walkthrough`；`companyName` 已改为 `FTSM`。
- `ProjectSettings/ProjectSettings.asset` 中的 `applicationIdentifier` 已统一为 `com.ftsm.lestariwalkthrough`，覆盖 `Standalone`、`Android`、`iPhone`。
- 当前打包版本显示名 `bundleVersion` 保持为 `0.1.0`。
- 主菜单标题 `Assets/Scenes/GAME.unity/FTSM Start Menu/Panel/Title` 当前文案已是 `FTSM Lestari Walkthrough`。
- 解决方案文件已从根目录 `My project (2).slnx` 重命名为 `FTSM Lestari Walkthrough.slnx`。
- 当前 `.csproj` 仍保持 Unity 自动生成的默认程序集命名（如 `Assembly-CSharp.csproj`），本次未改动程序集名称以避免影响脚本编译与引用。
<!-- locus:body:end -->

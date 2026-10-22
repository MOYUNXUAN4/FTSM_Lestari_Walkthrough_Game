---
id: kd_6f5dabec-0550-4c1f-8419-1ba5fef67e65
type: memory
path: unity-project-understanding/game-start-menu-and-corridor.md
title: game-start-menu-and-corridor
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
aiMaintained: true
explicitMaintenanceRules: true
createdAt: 1779939944351
updatedAt: 1779991532587
---

# game-start-menu-and-corridor

## Summary
GAME 场景 corridor prefab 与起始菜单结构缓存

<!-- locus:maintain-rules:start -->
- 仅记录能减少后续场景/UI定位成本的稳定结构信息
- 保持为简短事实，避免记录一次性调试状态
- 若菜单层级、脚本名、相机切换方案变化，及时更新或删除过时内容
<!-- locus:maintain-rules:end -->

<!-- locus:body:start -->
- `Assets/Scenes/GAME.unity` 现有顶层对象 `corridor`，已保存并连接到 prefab `Assets/Prefabs/corridor.prefab`。
- `Assets/Scenes/GAME.unity` 的起始菜单使用场景内 `Start` 摄像机画面，不单独建菜单场景。
- 起始菜单根对象为 `Assets/Scenes/GAME.unity/FTSM Start Menu`，包含 `Canvas + CanvasScaler + GraphicRaycaster + FTSMStartMenu`，其下有 `Panel/Title Plate`、`Panel/Title`、`Panel/Subtitle`、`Panel/Title Divider`、`Panel/Play Button` 和 `Panel/Exit Button`。
- 起始菜单当前标题文案拆为主标题 `FTSM Lestari Walkthrough` 与副标题 `Virtual Energy Audit System`，统一使用字体 `Assets/Fonts_ComicSans.ttf`；为适配浅色/灰色背景，标题区当前采用更深的半透明底板、浅金色主标题、浅灰白副标题与高对比描边。
- `Assets/FTSMStartMenu.cs` 负责起始菜单流程：进入场景时显示菜单、隐藏/禁用玩家根对象 `PC VR Simulator Player` 与 `Crosshair Canvas`，并启用 `Start` 摄像机；点击 Play 后播放 click，短暂等待后切回玩家视角并隐藏菜单；点击 Exit 播放 click 后调用 `Application.Quit()`。
- 当前 `Start` 摄像机初始视角依赖其场景内手动保存的 Transform，不再在运行时自动同步到玩家主相机。
- 起始菜单当前按钮布局为更亮的绿色 `Play` 按钮与其下方红色 `Exit` 按钮，二者尺寸均为 `320x100`。
<!-- locus:body:end -->

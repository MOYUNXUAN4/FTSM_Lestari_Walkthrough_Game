---
id: kd_3c4fd95c-d43c-4bcd-921c-540ad9376ad3
type: memory
path: unity-project-understanding/game-scene-audio.md
title: game-scene-audio
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
aiMaintained: true
explicitMaintenanceRules: true
createdAt: 1779936059835
updatedAt: 1780020996940
---

# game-scene-audio

## Summary
`GAME` 场景的音频与菜单结构：`FTSMAudioManager` 统一管理音效/BGM，`FTSMStartMenu` 负责开始菜单、设置滑条、No VR 开关，以及新增的 Esc 暂停菜单与返回主菜单流程。

<!-- locus:maintain-rules:start -->
- 仅记录稳定的音频系统结构、绑定关系、触发规则与容易复发的约束
- 避免记录一次性调试操作；仅保留能减少后续排查成本的事实
- 若音频入口、AudioSource 数量、设置面板结构、成功界面行为或 LED 指示灯方案变化，及时更新或删除过时条目
<!-- locus:maintain-rules:end -->

<!-- locus:body:start -->
- `Assets/Scenes/GAME.unity` 现在使用场景对象 `FTSM Audio Manager` + `Assets/FTSMAudioManager.cs` 统一管理交互音效。已绑定 `Assets/sound/click.mp3`, `ac.mp3`, `microwave.mp3`, `switch.mp3`, `room.mp3`, `walk.mp3`, `touch.mp3`, `successful.mp3`，并新增背景音乐 `Assets/sound/backgroundMusic.mp3`。
- `FTSM Audio Manager` 需要保留 4 个 `AudioSource`：普通效果、脚步片段、通关长音频、背景音乐。若脚本更新后场景里旧对象缺少组件，需要补齐后保存场景，否则会出现 BGM 或 successful 不播的问题。
- successful 现在由两处触发：`Assets/FTSMPowerGame.cs` 的 `WinGame()`，以及 `Assets/FTSMTotalPowerUI.cs` 的成功遮罩 `ShowSuccessOverlay()`；成功长音频源需开启 `ignoreListenerPause`，因为遮罩显示时会把 `Time.timeScale` 设为 0。
- successful 当前播放策略：从头播放，先保持原音量 5 秒，再用未缩放时间在 1.5 秒内渐弱到 0 并停止。
- 起始菜单 `Assets/Scenes/GAME.unity/FTSM Start Menu` 现在包含 `Panel/Settings Button` 与弹出式 `Panel/Settings Panel`；面板内有 `Music` 和 `Effects` 两个滑条，分别控制背景音乐与其他音效音量。
- `Assets/Scenes/GAME.unity/FTSM Start Menu/Panel/Pause Panel` 现已新增暂停菜单，复用同一套音量逻辑，包含 `Music`、`Effects` 两个滑条和 `Back To Menu Button`。`Assets/FTSMStartMenu.cs` 负责开始菜单与暂停菜单的统一管理。
- `Assets/FTSMStartMenu.cs` 负责菜单按钮与设置面板开关，并把滑条值实时写入 `FTSMAudioManager`；音量通过 `PlayerPrefs` 保存，键名为 `FTSM_MusicVolume` 和 `FTSM_EffectsVolume`。`RequestGameRestart()` 会让重载后自动直接开始游戏，`RequestReturnToMenu()` 会让重载后停留在菜单；`TryTogglePause()` 负责游戏内 Esc 暂停开关。
- `Assets/FTSMPlayerController.cs` 与 `Assets/FTSMForcedXRControls.cs` 现在都通过 Esc 调用 `FTSMStartMenu.TryTogglePause()`；暂停打开时会停止移动/交互更新，恢复时继续。
- 背景音乐当前基础音量调高为 `backgroundMusicVolume=0.5`，默认音乐滑条值调高为 `0.8`；`touchVolume=0.45`，`walkVolume=0.35`。滑条是在这些基础值之上乘以用户音量系数。
- 成功界面按钮现为 `Restart Game` 与 `Back to Menu`：前者重载当前场景后自动跳过开始菜单直接重开；后者重载当前场景并返回开始菜单，而不再退出应用。
- `Assets/Scenes/GAME.unity` 中蓝色状态 LED 目前包括 `AC_Status_LED`、`Printer_Status_LED`、`Coffee_Status_LED`、`Microwave_Status_LED` 等 8 个对象，已统一增强 Point Light 强度/范围，并提升其蓝色指示材质发光。
<!-- locus:body:end -->

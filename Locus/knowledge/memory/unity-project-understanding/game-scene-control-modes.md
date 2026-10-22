---
id: kd_fd6f42a9-1d59-4251-a4b3-80903e5b927c
type: memory
path: unity-project-understanding/game-scene-control-modes.md
title: game-scene-control-modes
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
aiMaintained: true
explicitMaintenanceRules: true
createdAt: 1779994806938
updatedAt: 1780000472709
---

# game-scene-control-modes

## Summary
`GAME.unity` 现在通过 `FTSMStartMenu` 在 No VR 与 VR 两套玩家方案之间切换；VR 已恢复为更接近 XR Toolkit sample 的 rig 与交互表现，同时用桥接脚本把现有项目交互接到 `XRSimpleInteractable` / sample interactor 体系。

<!-- locus:maintain-rules:start -->
- 只记录能减少后续场景/玩家控制研究成本的稳定结构信息
- 若 `GAME.unity` 的玩家入口、设置菜单或 VR 方案再次调整，要同步更新本条记忆
- 保持内容简短，聚焦对象位置、入口脚本和切换关系
<!-- locus:maintain-rules:end -->

<!-- locus:body:start -->
- `Assets/Scenes/GAME.unity` 现��包含双模式开场玩家方案：`PC VR Simulator Player` 作为 No VR 模式，场景内 `XR Origin (XR Rig)` 与 `XR Device Simulator` 作为 VR 模式，由 `FTSM Start Menu` 统一切换；启用 VR 时需要同时激活 `XR Origin (XR Rig)` 和 `XR Device Simulator`。
- `Assets/FTSMStartMenu.cs` 负责读取 `PlayerPrefs` 键 `FTSM.NoVRMode`，Settings 面板中的 `No VR Model` Toggle 勾选时启用 `PC VR Simulator Player`，不勾选时启用 VR 方案。
- 切换到 VR 时，不能在 `PC VR Simulator Player` 仍激活的同一帧里直接启用 `XR Origin (XR Rig)`，因为两套 `CharacterController` 初始重叠会把 XR 玩家直接顶出/卡出。当前 `FTSMStartMenu.StartGameAfterClick()` 会在 VR 分支先等一帧，再切换；`ActivateSelectedPlayerRig()` 也会先关闭 PC 玩家，再把 XR Rig 对齐到 PC 玩家出生点后启用 XR。
- `StartGameAfterClick()` 中需要先完成玩家相机切换、再关闭 `Start` 菜单相机；否则会出现一帧没有任何启用中的渲染相机，导致用户点击 Play 后看到 `no camera rendering`。
- `FTSMStartMenu` 现在统一通过 `SetCameraState(Camera,bool)` 同步控制 `Camera.enabled` 与同物体上的 `AudioListener.enabled`。菜单状态下会关闭 PC/XR 相机及其 AudioListener，只保留 `Start`；进入 PC/VR 后则关闭 `Start` 的 Camera 和 AudioListener，避免 “There are 2 audio listeners in the scene” 并避免主菜单相机残留干扰。
- `GAME.unity` 的 XR 主相机需要沿用 PC 相机的大范围裁剪参数；已验证 VR 看不到场景的直接原因之一是 XR 相机原始 `farClipPlane` 仅 1000，而场景尺度需要与 PC 相机一致的 120000。
- `GAME.unity` 的 XR 视角高度需要与现有 PC 玩家基准一致；当前 `XR Origin (XR Rig)` 已调为 `Camera Offset.y = 3000`、`Camera Y Offset = 3000`，CharacterController 为 Height 1800、CenterY 850、Radius 220、StepOffset 120、SkinWidth 20。
- 为了避免进入 VR 后等待一会儿继续自动下落，不能直接禁用 `XR Origin (XR Rig)/Locomotion/Gravity` 这个对象，因为 `JumpProvider` 依赖 `GravityProvider` 组件存在。当前修复是保持 `Gravity` 对象处于激活状态，但把 `GravityProvider.Use Gravity` 与 `DynamicMoveProvider.Use Gravity` 都设为 false，使 VR 保持恒定高度且不触发 Jump Provider 缺失依赖报错。
- `GAME.unity` 的原游戏内 HUD 在 `PC VR Simulator Player/Main Camera/Crosshair Canvas` 下，包含 `Crosshair`、`Room Objective Panel`、`Total Power Panel` 和 `Success Overlay`。由于 No VR 模式关闭时会禁用整个 `PC VR Simulator Player`，VR 模式会把这套 HUD 一起关掉。
- 当前仍保留 VR 专用 `VR Crosshair Canvas` 与 `FTSMStartMenu.vrCrosshairCanvas` 切换，但其 Canvas 表现已按用户要求恢复为与 No VR 相同的 `Screen Space - Overlay` 方案，不再使用贴脸 world-space HUD。
- `VR Crosshair Canvas` 内的 `FTSMMP1ObjectiveUI` 已改绑到 `XR Origin (XR Rig)` 与 XR 主相机，因此房间目标 HUD 会按 VR 玩家位置正常切换。
- 当前 `GAME.unity` 已从自定义强制 VR 射线方案切回 sample 风格主交互：`FTSMForcedXRControls` 组件保留但默认禁用，`XR Origin (XR Rig)` 下自定义 `FTSM Forced Left Teleport Ray` / `FTSM Forced Right Interact Ray` / 命中小球对象全部保持 Inactive，左右手 `Teleport Interactor` 恢复为 Active。
- 为让 sample 的 `Near-Far Interactor` / `Teleport Interactor` 能操作现有项目对象，`GAME.unity` 里的所有 `FTSMRoomInteractable` 与 `FTSMTeleportInteractable` 物体都已补上 `XRSimpleInteractable`；同时新增桥接脚本 `Assets/FTSMXRRoomInteractableBridge.cs` 和 `Assets/FTSMXRTeleportInteractableBridge.cs`，用于把 XR select 事件转发到原有房间交互与传送逻辑。
<!-- locus:body:end -->

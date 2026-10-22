---
id: kd_e1847b58-2e20-4627-ac6c-23130076e41f
type: memory
path: unity-project-understanding/ftsm-room-interactable-objective-ui.md
title: ftsm-room-interactable-objective-ui
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
aiMaintained: true
explicitMaintenanceRules: true
createdAt: 1779904836823
updatedAt: 1779910953684
---

# ftsm-room-interactable-objective-ui

## Summary
FTSM 房间目标 UI 的归类规则：普通房间必须按房间根过滤，走廊 TV 和灯光/风扇墙开关分别计数；当前墙开关默认开启并显示 12 度，房间空调默认展开 70 度并可由墙开关控制。

<!-- locus:maintain-rules:start -->
- Record only Unity project structure knowledge and lookup info that reduce repeated exploration
- Maintain only project-derived engineering understanding, including directory responsibilities, system entry points, asset relationships, runtime entry points, and config mappings
- Write user-supplied design goals, gameplay intent, product direction, and solution decisions into Design
- Prioritize directory responsibilities, core system entry points, key scenes, prefabs, ScriptableObjects, assemblies, and config mappings
- Record verified asset relationships, runtime entry points, key dependencies, and common lookup paths
- Remove temporary investigation traces, one-off task residue, unverified guesses, and expired cache
<!-- locus:maintain-rules:end -->

<!-- locus:body:start -->
- `Assets/FTSMRoomInteractable.cs` 新增 `includeInObjectiveUi` 序列化字段与只读属性。房间目标 UI `Assets/FTSMMP1ObjectiveUI.cs` 现在会跳过该字段为 `false` 的交互物体，适用于“墙上联动开关能控制设备，但不应计作任务目标”的场景。
- `Assets/FTSMMP1ObjectiveUI.cs` 普通房间目标归类必须满足 `BelongsToRoom`，避免 `MP1`、`BK1`、`Staff_Office_Group` 的 AC/TV/灯光目标互相串房间。走廊根级 `wall_tv` 通过 `IsCorridorTelevision` 单独计入走廊 TV 目标；走廊灯光目标现在通过 `IsCorridorLightWallSwitch` 只统计根级且 `InteractionType == ToggleLight` 的墙开关，不再把走廊风扇墙开关混进灯光计数。
- `Assets/Scenes/GAME.unity` 中 `MP1_Room/LightSwitches01/LightSwitches01` 与 `MP1_Room/LightSwitches01 (1)/LightSwitches01` 目前已设置为 `includeInObjectiveUi = false`，因此它们控制空调时不会推进右上角任务 UI。
- `Assets/Scenes/GAME.unity` 中根级走廊双联开关分工已确认：`LightSwitches02` 为走廊灯光墙开关，左右分别联动 `corridor/Corridor_Ceiling_Lights/Corridor_Rect_Ceiling_Light_01~02` 与 `03~05`；`LightSwitches02 (1)` 为走廊吊扇墙开关，左右分别联动 `corridor/fan_alternative` 与 `corridor/fan_alternative (1)`、以及 `corridor/fan_alternative (2)` 与 `corridor/fan_alternative (3)`。`corridor/fan_small` 保持独立交互，不受墙开关控制。
- `Assets/FTSMRoomInteractable.cs` 的开关初始化逻辑已改为从编辑态当前角度反推基础角，避免 `animatedTransform` 在进入 Play 时因角度重复叠加而发生跳转。以后若手动摆好开/关角度并保持 `startOn` 一致，进入 Play 不应再偏转。
- 当前项目中所有带 `animatedTransform` 的墙上开关交互统一使用 `startOn = true`、`offAngle = 0`、`onAngle = 12`，并把可见开关片默认摆到 `localRotation.x = 12`；也就是默认处于开启状态，交互关闭时回到 0 度。
- `Assets/Scenes/GAME.unity` 里 4 台房间空调（`MP1_Room/air_conditioner`、`MP1_Room/air_conditioner (1)`、`BK1_Room/air_conditioner`、`Staff_Office_Group/air_conditioner`）当前叶片动画参数统一为 `startOn = true`、`offAngle = 0`、`onAngle = 70`、`toggleSpeed = 6`，并把 `AC B` 可见初始旋转直接摆到 `localRotation.x = 70`；即默认叶片打开约 70 度，交互关闭后缓慢收回到 0 度。
- 房间空调墙开关当前绑定关系：`MP1_Room/LightSwitches01/LightSwitches01 -> MP1_Room/air_conditioner`，`MP1_Room/LightSwitches01 (1)/LightSwitches01 -> MP1_Room/air_conditioner (1)`，`BK1_Room/LightSwitches01 -> BK1_Room/air_conditioner`，`Staff_Office_Group/LightSwitches02/LightSwitches02/LightSwitches02_Left -> Staff_Office_Group/air_conditioner`。
<!-- locus:body:end -->

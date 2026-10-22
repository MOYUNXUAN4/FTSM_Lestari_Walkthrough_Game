---
id: kd_fe06b7da-5119-41a1-8a50-3fdbd7576eab
type: memory
path: unity-project-understanding/game-scene-lighting.md
title: game-scene-lighting
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
aiMaintained: true
explicitMaintenanceRules: true
createdAt: 1779894615101
updatedAt: 1779896516275
---

# game-scene-lighting

## Summary
`GAME.unity` 中 corridor 区域与走廊 lamp 的定位和交互接线信息。

<!-- locus:maintain-rules:start -->
- Record only durable scene-lookup mappings and structure facts that reduce repeated exploration
- Keep entries short and specific to confirmed object paths or subsystem wiring
- Update or remove entries if scene hierarchy changes invalidate the mapping
<!-- locus:maintain-rules:end -->

<!-- locus:body:start -->
- `Assets/Scenes/GAME.unity` 中存在单独顶层对象 `corridor`，其主要可操作灯具路径为 `corridor/lamp`。该对象来源于 `Assets/VNB - Office Set/Prefabs/Furniture/lamp.prefab`，子物体 `corridor/lamp/light` 挂 `Light`。已确认可直接在 `corridor/lamp` 上挂 `FTSMRoomInteractable` 并用 `ToggleLight` 模式控制此子灯。
<!-- locus:body:end -->

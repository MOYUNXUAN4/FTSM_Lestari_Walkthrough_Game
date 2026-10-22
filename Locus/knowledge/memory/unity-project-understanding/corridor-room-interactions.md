---
id: kd_f8c7919d-6272-418a-80c0-e63453e9d3e6
type: memory
path: unity-project-understanding/corridor-room-interactions.md
title: corridor-room-interactions
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
aiMaintained: true
explicitMaintenanceRules: true
createdAt: 1779901366418
updatedAt: 1780089413202
---

# corridor-room-interactions

## Summary
Corridor room interaction structure, 7-objective UI, wall switch grouping, TV/light setup, and poster 6 teleport override in `Assets/Scenes/GAME.unity`.

<!-- locus:maintain-rules:start -->
- Record only Unity project structure knowledge and lookup info that reduce repeated exploration
- Maintain only project-derived engineering understanding, including directory responsibilities, system entry points, asset relationships, runtime entry points, and config mappings
- Write user-supplied design goals, gameplay intent, product direction, and solution decisions into Design
- Prioritize directory responsibilities, core system entry points, key scenes, prefabs, ScriptableObjects, assemblies, and config mappings
- Record verified asset relationships, runtime entry points, key dependencies, and common lookup paths
- Remove temporary investigation traces, one-off task residue, unverified guesses, and expired cache
<!-- locus:maintain-rules:end -->

<!-- locus:body:start -->
- In `Assets/Scenes/GAME.unity`, the root object `corridor` is tracked by `FTSMMP1ObjectiveUI` as room title `走廊` with 7 objectives: coffee machine, computer, microwave, desk fan, TV, 4 ceiling fans, and 5 ceiling light fixtures.
- Corridor objective UI requires 7 rows. `Assets/FTSMMP1ObjectiveUI.cs` now uses `MaxObjectives = 7`, and `PC VR Simulator Player/Main Camera/Crosshair Canvas/Room Objective Panel/Objectives` contains `Objective Row 1..7`.
- Corridor ceiling light objective counts five independent `FTSMRoomInteractable` objects on `corridor/Corridor_Ceiling_Lights/Corridor_Rect_Ceiling_Light_01..05`, not the wall switches. Each fixture controls its own child `Soft_Light`, `Down_Spot_Light`, and `Ceiling_Fill_Light` and brightens fixture renderers with multipliers `0.6/2.8`.
- Four corridor wall switch toggles are direct `FTSMRoomInteractable` components on `LightSwitches02/LightSwitches02/LightSwitches02_Left`, `LightSwitches02/LightSwitches02/LightSwitches02_Right`, `LightSwitches02 (1)/LightSwitches02/LightSwitches02_Left`, and `LightSwitches02 (1)/LightSwitches02/LightSwitches02_Right`, all on layer `Interactable` with MP1-style animated toggle settings `offAngle 0.64`, `onAngle 12.82`, `toggleSpeed 420`.
- Corridor switch grouping is: first root switch left -> light fixtures 01-02, first root switch right -> light fixtures 03-05, second root switch left -> `corridor/fan_alternative` and `(1)`, second root switch right -> `corridor/fan_alternative (2)` and `(3)`.
- `wall_tv` in `Assets/Scenes/GAME.unity` is a top-level interactable TV using `FTSMRoomInteractable` in `ToggleComputer` mode with renderer `wall_tv`, `screenMaterialIndex = 1`, and intended off material `Assets/VNB - Office Set/Materials/screen_off.mat`; YAML component summaries may still show `Off Material Override: None`, so verify runtime behavior in play mode if TV black-screen behavior is adjusted again.
- Corridor device indicator LEDs on `coffee_machine`, `printer`, and `microwave_new` use blue point lights with color `(0.32, 0.75, 1.0)`, intensity `18`, range `1800`, and boosted emission on their `BlueIndicatorLight` material instances.
- `corridor/poster 6` is a prefab instance child with `FTSMTeleportInteractable`, `XRSimpleInteractable`, and `FTSMXRTeleportInteractableBridge`. In `Assets/Scenes/GAME.unity`, all three components are disabled to remove poster 6 teleport. Code-side, both `Assets/FTSMPlayerController.cs` and `Assets/FTSMForcedXRControls.cs` now require `FTSMTeleportInteractable.isActiveAndEnabled` before treating a hit as a teleport target, so disabled poster teleport components no longer trigger movement, prompts, or highlights.
<!-- locus:body:end -->

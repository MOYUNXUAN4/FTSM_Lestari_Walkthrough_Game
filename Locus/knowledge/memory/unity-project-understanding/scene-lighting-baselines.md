---
id: kd_46361a84-7007-45fb-89fe-5bef538ba0d7
type: memory
path: unity-project-understanding/scene-lighting-baselines.md
title: scene-lighting-baselines
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
aiMaintained: true
explicitMaintenanceRules: true
createdAt: 1779728900632
updatedAt: 1779792353361
---

# scene-lighting-baselines

## Summary
BK1 global environment lighting and ceiling-light baselines, with MP1 and Staff Office sync rules

<!-- locus:maintain-rules:start -->
- Record only Unity project structure knowledge and lookup info that reduce repeated exploration
- Maintain only project-derived engineering understanding, including directory responsibilities, system entry points, asset relationships, runtime entry points, and config mappings
- Write user-supplied design goals, gameplay intent, product direction, and solution decisions into Design
- Prioritize directory responsibilities, core system entry points, key scenes, prefabs, ScriptableObjects, assemblies, and config mappings
- Record verified asset relationships, runtime entry points, key dependencies, and common lookup paths
- Remove temporary investigation traces, one-off task residue, unverified guesses, and expired cache
<!-- locus:maintain-rules:end -->

<!-- locus:body:start -->
- `Assets/Scenes/BK1.unity` global environment lighting currently uses: Ambient Mode Flat, Ambient Sky `(0.42, 0.42, 0.46)`, Ambient Equator `(0.08, 0.085, 0.09)`, Ambient Ground `(0.025, 0.023, 0.02)`, Ambient Intensity `0.85`, Reflection Intensity `0.35`, builtin default skybox, fog disabled.
- `Assets/Scenes/BK1.unity` sun/directional-light baseline is: color `(1.0, 0.985, 0.97)`, intensity `0.18`, shadows `None`, rotation `(50, 330, 0)`.
- `Assets/Scenes/GAME.unity` was updated to match the above BK1 global environment lighting values.
- BK1 ceiling-light template in `Assets/Scenes/GAME.unity/BK1_Room/BK1_Ceiling_Lights/BK1_Rect_Ceiling_Light_01`: `Soft_Light` point light color `(1, 0.985, 0.97)`, intensity `2400000`, range `36000`, no shadows; `Down_Spot_Light` spot light color `(1, 0.98, 0.96)`, intensity `4200000`, range `42000`, spot angle `178`, inner `70`, soft shadows with strength `0.02`; `Ceiling_Fill_Light` point light color `(1, 0.99, 0.98)`, intensity `1400000`, range `30000`, no shadows.
- `Assets/Prefabs/MP1_Room.prefab`, `Assets/Scenes/MP1.unity`, and `Assets/Scenes/GAME.unity/MP1_Room` have their `Soft_Light`, `Down_Spot_Light`, and `Ceiling_Fill_Light` components synced to the BK1 template values while preserving existing light GameObject positions.
- `Assets/Prefabs/Staff_Office_Group.prefab`, `Assets/Scenes/Staff_Office.unity`, and `Assets/Scenes/GAME.unity/Staff_Office_Group` use BK1-style warm point-light settings scaled for the small office: `Office_Ceiling_Light` and child lamp `light` use BK1 `Soft_Light` color/type/shadow style with intensity `360` and range `4320`.
<!-- locus:body:end -->

---
id: kd_04ae091e-efcf-42ac-8d45-d694b1043cab
type: memory
path: unity-project-understanding/mp1-objective-ui.md
title: mp1-objective-ui
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
aiMaintained: true
explicitMaintenanceRules: true
createdAt: 1779803760696
updatedAt: 1779891372931
---

# mp1-objective-ui

## Summary
Durable notes about GAME objective UI ownership, room scoping, imported font, initialization order, room detection, total power HUD, suggested watt mapping, room signs, and verified room objective counts.

<!-- locus:maintain-rules:start -->
- Record only durable scene and system structure facts that reduce repeated exploration
- Keep counts and wiring notes updated if objective UI or room interaction setup changes
- Prefer facts verified from scene/scripts over assumptions
<!-- locus:maintain-rules:end -->

<!-- locus:body:start -->
- `Assets/Scenes/MP1.unity` uses `FTSMRoomInteractable` rather than `FTSMInteractable` for its room task logic.
- `Assets/Scenes/MP1.unity` no longer carries the runtime objective UI on its player root.
- In `GAME`, the room objective panel is a real scene UI object at `Assets/Scenes/GAME.unity/PC VR Simulator Player/Main Camera/Crosshair Canvas/Room Objective Panel` rather than a purely runtime-generated panel.
- `FTSMMP1ObjectiveUI` is attached to `Assets/Scenes/GAME.unity/PC VR Simulator Player/Main Camera/Crosshair Canvas/Room Objective Panel` and exposes inspector-tunable layout fields including panel size, panel offset, font sizes, and row spacing/height.
- `FTSMMP1ObjectiveUI` is wired to `PC VR Simulator Player` and `Main Camera` and uses the imported font asset `Assets/Fonts_ComicSans.ttf`.
- The objective panel uses fixed-height objective rows so the line spacing stays visually consistent across rooms, even when some rows have detail text and others do not.
- `FTSMMP1ObjectiveUI` must rebuild tracked room interactables in `Start()` instead of `Awake()`, because `FTSMRoomInteractable.IsOn` is not reliable before other interactables finish their own `Awake()` initialization; otherwise MP1 can appear fully completed on the first frame.
- In `GAME`, the objective tracker must scope each room to its own subtree only; otherwise counts will incorrectly include other rooms in the combined scene.
- In `GAME`, room detection for the objective UI should prefer each room's `floor_tile` world-space bounds on the XZ plane and only fall back to nearest room center when outside all floor bounds. Using room root transforms or full renderer bounds causes incorrect switching across MP1, BK1, and Staff Office.
- For the MP1 room objective tracking in `GAME`, the intended rows are: student computers `33`, doctor computer `1`, air conditioners `2`, small fan `1`, TV `1`, light switches `3`.
- The MP1 doctor computer objective is now a standalone row in the UI rather than a detail note under the computers row.
- For the BK1 room objective tracking in `GAME`, the intended counts are: air conditioner `1`, computer `1`, TV `1`, light switches `2`.
- For the Staff Office objective tracking in `GAME`, the intended counts are: air conditioner `1`, printer `1`, computer `1`, ceiling fan `1`, desk fan `1`, light switches `1`.
- `Assets/Scenes/GAME.unity` lighting for the `MP1_Room` area was synchronized from `Assets/Scenes/MP1.unity`: the root `Directional Light` is disabled and the room lighting relies on the copied MP1 ceiling-light setup.
- `Assets/Scenes/GAME.unity/PC VR Simulator Player/Main Camera/Crosshair Canvas/Total Power Panel` is a right-side runtime HUD panel driven by `Assets/FTSMTotalPowerUI.cs`.
- `FTSMTotalPowerUI` tracks all non-door `FTSMRoomInteractable` instances with positive effective power, shows the current total wattage, and animates a green negative watt delta with fade-in, 1 second hold, and fade-out when a device is turned off.
- `FTSMRoomInteractable` now has `useSuggestedPower` plus `powerWatts`. Suggested power currently resolves by object role: ceiling/room lights `180W`, desk fan `35W`, larger fan `75W`, air conditioner `1500W`, computer `180W`, TV `120W`, printer `80W`, door `0W`, and proxy switch objects such as AC-linked `LightSwitches...` controls `0W`.
- `GAME` now has world-space room name signs parented under `MP1_Room`, `BK1_Room`, and `Staff_Office_Group` as `RoomNameSign`, using Comic Sans text on a dark plaque above the exterior door wall.
<!-- locus:body:end -->

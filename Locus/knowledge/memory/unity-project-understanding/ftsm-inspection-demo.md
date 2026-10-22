---
id: kd_008dee82-1e2b-4f35-b298-1b0dab843914
type: memory
path: unity-project-understanding/ftsm-inspection-demo.md
title: ftsm-inspection-demo
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
aiMaintained: true
explicitMaintenanceRules: true
createdAt: 1779331013073
updatedAt: 1780001793385
---

# ftsm-inspection-demo

## Summary
FTSM inspection demo scene structure, XR simulator setup, room interaction wiring, and GAME scene XR simulator bootstrap details.

<!-- locus:maintain-rules:start -->
Record only Unity project structure knowledge and lookup info that reduce repeated exploration
Maintain only project-derived engineering understanding, including directory responsibilities, asset relationships, runtime entry points, and config mappings
Write user-supplied design goals, gameplay intent, product direction, and solution decisions into Design
Prioritize directory responsibilities, core system entry points, key scenes, prefabs, ScriptableObjects, assemblies, and config mappings
Record verified asset relationships, runtime entry points, key dependencies, and common lookup paths
Remove temporary investigation traces, one-off task residue, unverified guesses, and expired cache
<!-- locus:maintain-rules:end -->

<!-- locus:body:start -->
## Summary
FTSM inspection demo scene structure, XR simulator setup, room interaction wiring, and `GAME` corridor additions including MP1-style ceiling lights, replicated fans, appliance LEDs, old computer screen toggling, and microwave door animation.

## Maintenance Rules
Record only Unity project structure knowledge and lookup info that reduce repeated exploration
Maintain only project-derived engineering understanding, including directory responsibilities, asset relationships, runtime entry points, and config mappings
Write user-supplied design goals, gameplay intent, product direction, and solution decisions into Design
Prioritize directory responsibilities, core system entry points, key scenes, prefabs, ScriptableObjects, assemblies, and config mappings
Record verified asset relationships, runtime entry points, key dependencies, and common lookup paths
Remove temporary investigation traces, one-off task residue, unverified guesses, and expired cache

## Content
- `Assets/Scenes/SampleScene.unity` is generated as an FTSM low-carbon inspection demo scene.
- Runtime scripts: `Assets/FTSMPowerGame.cs`, `Assets/FTSMInteractable.cs`, `Assets/FTSMPlayerController.cs`, `Assets/FTSMSpin.cs`, `Assets/FTSMXRInteractableBridge.cs`, `Assets/FTSMForcedXRControls.cs`, `Assets/FTSMRoomInteractable.cs`, `Assets/FTSMInteractableHighlighter.cs`, `Assets/FTSMInteractionPromptUI.cs`.
- XR Interaction Toolkit 3.1.1 and XR Management are added in `Packages/manifest.json`. XRI samples are imported under `Assets/Samples/XR Interaction Toolkit/3.1.1/`.
- Scene now contains `XR Origin (XR Rig)` from XRI Starter Assets and `XR Device Simulator` from XRI Device Simulator sample. The old `PC VR Simulator Player` remains in the scene but is inactive.
- `Teleport Floor - FTSM Corridor and Rooms` has `TeleportationArea`. FTSM click targets have `XRSimpleInteractable` + `FTSMXRInteractableBridge`, so XRI select events call existing `FTSMInteractable.Interact()`.
- Asymmetric XR setup: left controller Near-Far interactor disabled, left Teleport Interactor active; right Teleport Interactor disabled, right Near-Far Interactor active.
- Scene contains corridor + 3 rooms, 24 interactable click targets controlling 32 physical devices: 13 computer/laptop/projector targets, 4 AC remote targets, 4 light switches controlling 10 lights, 3 fan switches controlling 5 fans, plus exit door.
- Per-device wattage total follows user choice “按设备功耗做”: 6150W initial load, target 100W. Corridor emergency lights are currently interactable too, so all non-exit load can be reduced to 0W; HUD goal remains <=100W.
- Imported environment pack `Assets/VNB - Office Set/` uses legacy `Standard` materials in a URP project; when the pack appears to have “missing textures”, first verify texture references on the `.mat` files before assuming textures were lost. Converting those materials to `Universal Render Pipeline/Lit` restores expected rendering while preserving texture assignments.
- `Assets/Scenes/Staff_Office.unity/Staff_Office_Group` uses room-local interactions wired through `Assets/FTSMRoomInteractable.cs` and `Assets/FTSMSpin.cs`.
- In `Assets/Scenes/Staff_Office.unity/Staff_Office_Group`, the single switch `LightSwitches01/LightSwitches01` controls `Office_Ceiling_Light`; the double switch left rocker `LightSwitches02/LightSwitches02/LightSwitches02_Left` controls the AC indicator power and links to `air_conditioner`; the double switch right rocker `LightSwitches02/LightSwitches02/LightSwitches02_Right` controls ceiling fan `fan_1/R Fan`.
- Staff Office AC now matches MP1/BK1 animation wiring: `Assets/Scenes/Staff_Office.unity/Staff_Office_Group/air_conditioner` uses `FTSMRoomInteractable` with `animatedTransform = AC B`, `toggleAxis = Vector3.right`, `offAngle = 0`, `onAngle = 63`, `toggleSpeed = 20`; the left double switch keeps MP1/BK1 switch angles `offAngle = 0.64`, `onAngle = 12.82`, `toggleSpeed = 420`.
- `Assets/FTSMRoomInteractable.cs` supports `ToggleLight`, `ToggleFan`, `ToggleAirConditioner`, `ToggleComputer`, and `ToggleDoor`, and now exposes `InteractionType` for UI/display mapping. `animatedTransform` with `toggleAxis/offAngle/onAngle/toggleSpeed` is also used by non-door appliances when an on-off object animation is needed.
- `Assets/FTSMPlayerController.cs` and `Assets/FTSMForcedXRControls.cs` both use a center/hand `Physics.Raycast` against `interactMask`, resolve `FTSMRoomInteractable` first and then `FTSMInteractable`, and now also drive look-at highlight state plus center-screen English interaction labels from that same hit result.
- Interaction label naming is mostly type-based, but `printer` and `tv` object names override the generic room-interaction label so `Staff_Office_Group/printer` shows `Printer` and `wall_tv` objects show `TV` instead of the fallback `Computer`.
- `Assets/FTSMInteractableHighlighter.cs` lazily adds a runtime-only component to the currently targeted interactable root, clones child `MeshFilter` geometry into temporary child outline objects, and renders a thin orange outline shell via shader `Assets/LocusGenerated/Shaders/FTSMOutlineHighlight.shader`.
- `Assets/FTSMInteractionPromptUI.cs` lazily creates `Interaction Prompt` under the first-person/XR camera canvas at runtime and shows a semi-transparent dark label like `Computer`, `Light`, `Fan`, `Air Conditioner`, `Door`, `Printer`, `TV`, or `Exit` near the crosshair.
- `Assets/Prefabs/Staff_Office_Group.prefab/Staff_Office_Group/door_b` is the source for the Staff Office door used in `GAME.unity` and `Staff_Office.unity`; its `FTSMRoomInteractable.startOn` is `false` and its local rotation has been synchronized to the closed pose `(0,0,0)` so it starts closed consistently.
- `Assets/Scenes/Staff_Office.unity/Staff_Office_Group/computer_new` is configured so monitor screens, not the PC case, are the power-off visual target.
- `Assets/Scenes/Staff_Office.unity/Staff_Office_Group/fan_small` has its own `FTSMRoomInteractable`, and the blade object is `Assets/Scenes/Staff_Office.unity/Staff_Office_Group/fan_small/R Fan` using `FTSMSpin`.
- The large poster display in Staff Office is `Assets/Scenes/Staff_Office.unity/Staff_Office_Group/office_1_poster_display`, a Quad using `Assets/VNB - Office Set/Poster/Materials/Office_1_post.mat`. The original prefab poster `office_1_poster` is kept inactive.
- Staff Office has a custom ceiling flood light using `Assets/Scenes/Staff_Office.unity/Staff_Office_Group/Office_Ceiling_Lamp` plus `Assets/Scenes/Staff_Office.unity/Staff_Office_Group/Office_Ceiling_Light`.
- Small blue indicator LEDs in Staff Office use shared material asset `Assets/BlueIndicatorLight.mat`; avoid `renderer.material` in Edit Mode for these objects to prevent leaked material instances.
- `Assets/Scenes/GAME.unity` includes a non-prefab `PC VR Simulator Player` with `Assets/FTSMPlayerController.cs`, child `Main Camera`, crosshair canvas, teleport ray, and interact ray; the original root `Main Camera` is disabled to avoid duplicate active audio listeners/cameras.
- Current `Assets/Scenes/GAME.unity` first-person scale tuning: `PC VR Simulator Player` at `(24500, 900, 23000)`, yaw `180`, `CharacterController` height/radius/center = `1800 / 220 / (0,850,0)`, `Main Camera` local position `(0,1500,0)`, FOV `68`, near/far clip `3 / 120000`, `moveSpeed = 1800`, `rayDistance = 42000`, `interactMask = Interactable(15)`, and centered `Crosshair Canvas/Crosshair` image size `18x18`. Room root transforms were intentionally left unchanged.
- `Assets/Scenes/GAME.unity/corridor` now contains `Corridor_Ceiling_Lights` with 6 manually placed `Corridor_Rect_Ceiling_Light_0X` fixtures cloned from the MP1 ceiling light structure, spread across the main desk/chair zone and the back microwave end rather than directly centered on the fan row.
- All currently visible corridor fans are now interactable and spinning: `Assets/Scenes/GAME.unity/corridor/fan_alternative`, `Assets/Scenes/GAME.unity/corridor/fan_alternative (1)`, `Assets/Scenes/GAME.unity/corridor/fan_alternative (2)`, `Assets/Scenes/GAME.unity/corridor/fan_alternative (3)`, and `Assets/Scenes/GAME.unity/corridor/fan_small` each have `FTSMRoomInteractable` plus child `R Fan/FTSMSpin`.
- `Assets/Scenes/GAME.unity/corridor/coffee_machine`, `Assets/Scenes/GAME.unity/corridor/printer`, and `Assets/Scenes/GAME.unity/corridor/microwave_new` each now use `FTSMRoomInteractable` in `ToggleAirConditioner` mode with a child blue status LED light/renderer, following the same on-off indicator pattern as the AC and Staff Office printer.
- `Assets/Scenes/GAME.unity/corridor/microwave_new` also uses `animatedTransform = M_Door`, `toggleAxis = Vector3.up`, `offAngle = 20`, `onAngle = 0`, `toggleSpeed = 120`, so clicking it animates the door toward a 20-degree local Y offset when turned off.
- `Assets/Scenes/GAME.unity/corridor/computer_old` now has `FTSMRoomInteractable` in `ToggleComputer` mode targeting `Old Monitor` material index `1`, so clicking it blacks the screen without changing the whole device body.
- Shared blue LED material `Assets/BlueIndicatorLight.mat` was brightened by increasing `_EmissionColor`, and scene blue indicator `Light` components were normalized to at least range `180` and intensity `10` for stronger visibility.
- `Assets/Scenes/GAME.unity` room name signs were converted from world-space UI to lit 3D quads: `MP1_Room/MP1_Room_RoomNameSign_Lit`, `BK1_Room/BK1_Room_RoomNameSign_Lit`, and `Staff_Office_Group/Staff_Office_Group_RoomNameSign_Lit`. The original `RoomNameSign` canvases were left inactive.
- New teleport target script `Assets/FTSMTeleportInteractable.cs` lets a clicked object provide a forward-offset teleport destination in front of itself.
- `Assets/FTSMPlayerController.cs` and `Assets/FTSMForcedXRControls.cs` now resolve `FTSMTeleportInteractable` before other interactables; clicking such a target teleports the player in front of it instead of toggling device state.
- In `Assets/Scenes/GAME.unity`, poster/sign teleport targets are on layer `Interactable` and currently include `MP1_Room/MP1_Poster_Display`, `MP1_Room/MP1_Poster_Display_Exterior`, `MP1_Room/MP1_Room_RoomNameSign_Lit`, `BK1_Room/MP1_Poster_Display`, `BK1_Room/MP1_Poster_Display_Exterior`, `BK1_Room/BK1_Room_RoomNameSign_Lit`, `Staff_Office_Group/office_1_poster_display`, `Staff_Office_Group/office_1_poster_display_Exterior`, and `corridor/poster 5|6|7`.
- Current poster/sign teleport targets use `forwardOffset = 1800` and keep the player's existing Y height when moving, so teleport lands in front of the clicked sign at the current standing level.
- `Assets/FTSMStartMenu.cs` owns the settings toggle stored as PlayerPrefs key `FTSM.NoVRMode`; checked means legacy non-VR player, unchecked means XR simulator path using `XR Origin (XR Rig)` plus `XR Device Simulator`.
- `Assets/Scenes/GAME.unity` now includes `XRInteractionManager` as a root object and the start menu references `FTSM Start Menu/Panel/Settings Panel/No VR Model Row/Toggle`.
- `Assets/FTSMXRRoomInteractableBridge.cs` and `Assets/FTSMXRTeleportInteractableBridge.cs` extend XRI select events to existing room/device and teleport interactables in `GAME.unity`.
- `Assets/Scenes/GAME.unity` currently has 92 `XRSimpleInteractable` targets wired for XR selection: 81 room/device bridges and 11 teleport bridges.
- `Assets/FTSMXRSimulatorBootstrap.cs` is attached to `Assets/Scenes/GAME.unity/XR Origin (XR Rig)` and forces the sample simulator back into motion-controller mode for several startup frames so controller models and near-far laser visuals remain visible after pressing Play in VR mode.
- Runtime verification in Play Mode showed that after Play is pressed in VR mode, `XR Origin (XR Rig)`, `XR Device Simulator`, both controller visuals, and both `Near-Far Interactor/LineVisual` objects stay active while the simulator reports `deviceMode=Controller` and the modality manager reports both hands in `MotionController` mode.
<!-- locus:body:end -->

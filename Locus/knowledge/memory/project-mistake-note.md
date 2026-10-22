---
id: kd_builtin_memory_project_mistake_note
type: memory
path: project-mistake-note.md
title: project-mistake-note
injectMode: full
summaryEnabled: false
summaryCache: 'Verified scene-editing pitfall: preserve GAME Staff_Office_Group Transform overrides while changing lighting/prefab-related settings'
commandEnabled: false
readOnly: false
aiMaintained: true
explicitMaintenanceRules: true
createdAt: 1779318567045
updatedAt: 1779792719293
---

# project-mistake-note

<!-- locus:maintain-rules:start -->
- Record only verified problems, rework causes, and avoidance steps
- Prioritize recurring pitfalls, constraints, regression points, and confirmed fixes
- Keep each entry short and focused on one lesson or constraint
- Keep the list within 20 items and merge duplicates regularly
- Remove outdated issues, non-reproducible issues, and unsupported guesses
<!-- locus:maintain-rules:end -->

<!-- locus:body:start -->
- 修改 `Assets/Scenes/GAME.unity/Staff_Office_Group` 灯光或 Prefab 关联内容时，必须先记录并最终复查该实例 Transform 覆盖值。已验证用户要求保持的位置/旋转/缩放为 localPosition `(-19347, 191, 21491)`, localRotation `(0, -90, 0)`, localScale `(219.9307, 219.9307, 219.9307)`. Prefab 保存/场景重载流程可能把实例显示为 prefab 默认 Transform，发现后要立即恢复覆盖值。
<!-- locus:body:end -->

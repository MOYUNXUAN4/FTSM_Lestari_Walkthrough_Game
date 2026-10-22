---
id: kd_df1855a6-b695-40d3-b4ae-f0f3d8fb9ede
type: memory
path: unity-project-understanding/unity6-xr-legacyinputhelpers-compile-fix.md
title: unity6-xr-legacyinputhelpers-compile-fix
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
inheritAiConfig: true
createdAt: 1782162902299
updatedAt: 1782162902309
---

# unity6-xr-legacyinputhelpers-compile-fix

## Summary
Unity 6 项目中，旧版 XR Legacy Input Helpers 编辑器代码会与当前 URP 组合产生编译失败；已通过嵌入包并降级旧菜单检查逻辑修复。

<!-- locus:body:start -->
## 现象
- 点击 Play 前后触发编译时，项目报错卡在包源码而不是业务脚本。
- 典型报错位置包括 `Library/PackageCache/com.unity.xr.legacyinputhelpers.../Editor/MenuUtils.cs`，提示 `UnityEngine.Rendering.Universal` 命名空间不存在；在缓存损坏时还可能伴随 `com.unity.render-pipelines.universal` 与 `com.unity.visualscripting` 的源码缺失类型错误。

## 已验证原因
- 当前项目运行于 Unity 6 (`6000.3.9f1`) + URP 17。
- `com.unity.xr.management@4.5.1` 依赖的 `com.unity.xr.legacyinputhelpers@2.1.13` 面向较老 Unity 版本，其 Editor 菜单代码中的 URP/HDRP 兼容分支在当前包组合下可能导致编译失败。
- 若 `Library/PackageCache` 某些目录不完整，还会放大为包内类型互相找不到的问题。

## 已验证修复
1. 先删除损坏的 `Library/PackageCache/com.unity.render-pipelines.universal@...` 与 `Library/PackageCache/com.unity.visualscripting@...`，让 Unity 重新解析包。
2. 将 `com.unity.xr.legacyinputhelpers` 嵌入到 `Packages/com.unity.xr.legacyinputhelpers`。
3. 修改 `Packages/com.unity.xr.legacyinputhelpers/Editor/MenuUtils.cs`，移除对 URP/HDRP Editor 识别逻辑的编译依赖，保留通用相机组件数量检查。
4. 重新 `unity_recompile` 后编译成功。

## 后续约束
- 如果后面再次出现 Play 前编译失败且错误位于 `Library/PackageCache`，优先检查包缓存损坏和旧 XR 包兼容问题，不要先怀疑业务脚本。
- 若升级 XR 相关包版本，需要重新验证 `Packages/com.unity.xr.legacyinputhelpers/Editor/MenuUtils.cs` 这个本地嵌入补丁是否仍然需要。
<!-- locus:body:end -->

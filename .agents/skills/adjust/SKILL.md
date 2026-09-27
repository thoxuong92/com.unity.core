---
name: adjust
description: >-
  Directs execution, code edits, and tools to the Unity Adjust Service package located at E:\Clients\com.unity.adjust.
  Activate this skill when the user types /adjust or asks to work on, inspect, modify, test, or manage com.unity.adjust.
---

# Unity Adjust Package Skill (/adjust)

When this skill is activated (e.g. via `/adjust <request>`), you MUST focus all operations on the **`com.unity.adjust`** package.

## Package Identity & Target Directory
- **Target Directory**: `E:\Clients\com.unity.adjust`
- **Package ID**: `com.unity.adjust`
- **Git Repository**: `https://github.com/thoxuong92/com.unity.adjust.git`
- **Main Assemblies**:
  - Runtime: `Unity.Adjust` (`Runtime/Unity.Adjust.asmdef` -> references `Unity.Core`, `AdjustSdk.Scripts`)
  - Editor: `Unity.Adjust.Editor` (`Editor/Unity.Adjust.Editor.asmdef` -> references `Unity.Core`, `Unity.Adjust`, `Unity.Core.Editor`, `AdjustSdk.Scripts`, `AdjustSdk.Editor`)
- **Root Namespace**: `Unity.Adjust` & `Unity.Adjust.Editor`
- **Core Dependency**: `com.unity.core` (`1.0.0`)

## Core Modules & Features
- **Adjust Tracking Provider**: `Unity.Adjust.AdjustTrackingProvider` (implements `ITrackingProvider` and `IAnalyticsProvider`)
  - Auto-registers with `TrackingService` & `AnalyticsService`
  - Attribution tracking, deep linking processing, In-App events
  - Impression-level ad revenue tracking via `Adjust.TrackAdRevenue()`
- **Config & Resource Loader**: `Unity.Adjust.AdjustConfigData`
  - Loads configuration from `Resources/Info.json` or `Resources/Info_Android.json`
- **Editor Config Window**: `Unity.Adjust.Editor.AdjustEditorWindow`
  - Menu: `Unity Core > Adjust > Setup & Configuration`
- **Preprocess & Manifest Safety**: `AdjustEditorPreprocessor.cs`
  - Manages Android manifest without deprecated `INSTALL_PACKAGES` or `debuggable=true`

## Execution Instructions
1. **Working Directory**: Always set `Cwd` to `E:\Clients\com.unity.adjust` when running shell commands.
2. **File Paths**: Target files within `E:\Clients\com.unity.adjust\`.
3. **Store Safety**: Never re-introduce `android:debuggable="true"` or `android.permission.INSTALL_PACKAGES` into `AdjustAndroidManifest.xml`.
4. **Stripping Safety**: Ensure `link.xml` preserves `Unity.Adjust` and `AdjustSdk.Scripts`.

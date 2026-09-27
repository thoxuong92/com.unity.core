---
name: appsflyer
description: >-
  Directs execution, code edits, and tools to the Unity AppsFlyer Service package located at E:\Clients\com.unity.appsflyer.
  Activate this skill when the user types /appsflyer or asks to work on, inspect, modify, test, or manage com.unity.appsflyer.
---

# Unity AppsFlyer Package Skill (/appsflyer)

When this skill is activated (e.g. via `/appsflyer <request>`), you MUST focus all operations on the **`com.unity.appsflyer`** package.

## Package Identity & Target Directory
- **Target Directory**: `E:\Clients\com.unity.appsflyer`
- **Package ID**: `com.unity.appsflyer`
- **Git Repository**: `https://github.com/thoxuong92/com.unity.appsflyer.git`
- **Main Assemblies**:
  - Runtime: `Unity.AppsFlyer` (`Runtime/Unity.AppsFlyer.asmdef` -> references `Unity.Core`, `AppsFlyer`)
  - Editor: `Unity.AppsFlyer.Editor` (`Editor/Unity.AppsFlyer.Editor.asmdef` -> references `Unity.Core`, `Unity.AppsFlyer`, `Unity.Core.Editor`, `AppsFlyer`, `AppsFlyer.Editor`)
- **Root Namespace**: `Unity.AppsFlyer` & `Unity.AppsFlyer.Editor`
- **Core Dependency**: `com.unity.core` (`1.0.0`)

## Core Modules & Features
- **AppsFlyer Tracking Provider**: `Unity.AppsFlyer.AppsFlyerTrackingProvider` (implements `ITrackingProvider` and `IAnalyticsProvider`)
  - Auto-registers with `TrackingService` & `AnalyticsService`
  - Attribution tracking, In-App events, iOS ATT (App Tracking Transparency) timeout support
  - Impression-level ad revenue tracking via `AppsFlyerAdRevenue.logAdRevenue()`
- **Config & Resource Loader**: `Unity.AppsFlyer.AppsFlyerConfig`
  - Loads `DevKey`, `AppID`, `AttTimeoutSeconds` from `Resources/Info.json`
- **Editor Config Window**: `Unity.AppsFlyer.Editor.AppsFlyerEditorWindow`
  - Menu: `Unity Core > AppsFlyer > Setup & Configuration`
- **Apple Privacy Manifest**: `PrivacyInfo.xcprivacy` included in iOS framework bundles.

## Execution Instructions
1. **Working Directory**: Always set `Cwd` to `E:\Clients\com.unity.appsflyer` when running shell commands.
2. **File Paths**: Target files within `E:\Clients\com.unity.appsflyer\`.
3. **Stripping Safety**: Ensure `link.xml` preserves `Unity.AppsFlyer` and `AppsFlyer` assemblies.

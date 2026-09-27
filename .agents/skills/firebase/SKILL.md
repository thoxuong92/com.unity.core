---
name: firebase
description: >-
  Directs execution, code edits, and tools to the Unity Firebase Service package located at E:\Clients\com.unity.firebase.
  Activate this skill when the user types /firebase or asks to work on, inspect, modify, test, or manage com.unity.firebase.
---

# Unity Firebase Package Skill (/firebase)

When this skill is activated (e.g. via `/firebase <request>`), you MUST focus all operations on the **`com.unity.firebase`** package.

## Package Identity & Target Directory
- **Target Directory**: `E:\Clients\com.unity.firebase`
- **Package ID**: `com.unity.firebase`
- **Git Repository**: `https://github.com/thoxuong92/com.unity.firebase.git`
- **Main Assemblies**:
  - Runtime: `Unity.Firebase` (`Runtime/Unity.Firebase.asmdef` -> references `Unity.Core`)
  - Editor: `Unity.Firebase.Editor` (`Editor/Unity.Firebase.Editor.asmdef` -> references `Unity.Core`, `Unity.Firebase`, `Unity.Core.Editor`)
- **Root Namespace**: `Unity.Firebase` & `Unity.Firebase.Editor`
- **Core Dependency**: `com.unity.core` (`1.0.0`)

## Core Modules & Features
- **Firebase Analytics**: `Unity.Firebase.FirebaseAnalyticsProvider` (implements `IAnalyticsProvider`)
  - Auto-registers with `AnalyticsService.AddProvider()`
  - Standard Ad formats & Level tracking methods
- **Firebase Remote Config**: `Unity.Firebase.FirebaseRemoteConfigProvider` (implements `IRemoteConfigProvider`)
  - Safe generic `GetValue<T>` with fallback to local encrypted cache and `Resources/RemoteConfig.json`
- **Ad Revenue Attribution**: `Unity.Firebase.FirebaseTrackingProvider` (implements `ITrackingProvider`)
  - Converts `AdRevenueInfo` to Firebase standard `ad_impression` events
- **Lifecycle Manager**: `Unity.Firebase.FirebaseManager` (Thread-safe initialization & fallbacks)
- **Editor Sync Tool**: `Unity.Firebase.Editor.FirebaseRemoteConfigSyncWindow`
  - Menu: `Unity Core > Firebase > Remote Config Sync Tool`
  - OAuth2 Google REST API to pull Remote Config templates into `Resources/`

## Execution Instructions
1. **Working Directory**: Always set `Cwd` to `E:\Clients\com.unity.firebase` when running shell commands.
2. **File Paths**: Target files within `E:\Clients\com.unity.firebase\`.
3. **Git Operations**: Commit and push within `E:\Clients\com.unity.firebase`.
4. **Stripping Safety**: Ensure `link.xml` preserves `Unity.Firebase` and Google Apis assemblies.

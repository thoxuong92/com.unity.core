---
name: applovin
description: >-
  Directs execution, code edits, and tools to the Unity AppLovin MAX Service package located at E:\Clients\com.unity.applovin.
  Activate this skill when the user types /applovin or asks to work on, inspect, modify, test, or manage com.unity.applovin.
---

# Unity AppLovin MAX Package Skill (/applovin)

When this skill is activated (e.g. via `/applovin <request>`), you MUST focus all operations on the **`com.unity.applovin`** package.

## Package Identity & Target Directory
- **Target Directory**: `E:\Clients\com.unity.applovin`
- **Package ID**: `com.unity.applovin`
- **Git Repository**: `https://github.com/thoxuong92/com.unity.applovin.git`
- **Main Assemblies**:
  - Runtime: `Unity.AppLovin` (`Runtime/Unity.AppLovin.asmdef` -> references `Unity.Core`)
  - Editor: `Unity.AppLovin.Editor` (`Editor/Unity.AppLovin.Editor.asmdef` -> references `Unity.Core`, `Unity.AppLovin`, `Unity.Core.Editor`)
- **Root Namespace**: `Unity.AppLovin` & `Unity.AppLovin.Editor`
- **Core Dependency**: `com.unity.core` (`1.0.0`)

## Core Modules & Features
- **Ads Service Adapter**: `Unity.AppLovin.AppLovinAdsAdapter` (implements `IAdsService`)
  - Auto-registers with `AdsService.Register()`
  - Full lifecycle for App Open (AOA), Banner, Interstitial, and Rewarded Ads
  - Graceful fallback with Mock mode in Editor or when SDK is missing
  - Binds MAX revenue events (`OnAdRevenuePaid`) and forwards `AdRevenueInfo` to `TrackingService`
- **Config & Settings**: `Unity.AppLovin.AppLovinConfig`
  - Loads SDK keys and ad unit IDs from `Resources/Info.json`
- **Editor Mediation Window**: `Unity.AppLovin.Editor.AppLovinEditorWindow`
  - Menu: `Unity Core > AppLovin MAX > Integration & Mediation Setup`
- **Scoped Registries Manager**: `Unity.AppLovin.Editor.AppLovinUpmManifest`
  - Automates AppLovin MAX and OpenUPM registry entries in `Packages/manifest.json`
- **Mediation Networks Registry**: `Unity.AppLovin.Editor.MaxMediation`
- **Sanitized AAR**: `LibAar~/ads_resource.aar` (`package="com.unity.ads.resources"`, secure network config)

## Execution Instructions
1. **Working Directory**: Always set `Cwd` to `E:\Clients\com.unity.applovin` when running shell commands.
2. **File Paths**: Target files within `E:\Clients\com.unity.applovin\`.
3. **Stripping Safety**: Ensure `link.xml` preserves `Unity.AppLovin` and `MaxSdk` assemblies.

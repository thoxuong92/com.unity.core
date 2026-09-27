---
name: core
description: >-
  Directs execution, code edits, and tools to the Unity Core Framework package located at E:\Clients\com.unity.core.
  Activate this skill when the user types /core or asks to work on, inspect, modify, test, or manage com.unity.core.
---

# Unity Core Package Skill (/core)

When this skill is activated (e.g. via `/core <request>`), you MUST focus all operations on the **`com.unity.core`** framework package.

## Package Identity & Target Directory
- **Target Directory**: `E:\Clients\com.unity.core`
- **Package ID**: `com.unity.core`
- **Git Repository**: `https://github.com/thoxuong92/com.unity.core.git`
- **Main Assemblies**:
  - Runtime: `Unity.Core` (`Runtime/Unity.Core.asmdef`)
  - Editor: `Unity.Core.Editor` (`Editor/Unity.Core.Editor.asmdef`)
- **Root Namespace**: `Unity.Core`

## Core Architecture & Modules
- **Service Locator**: `Unity.Core.Services.Core.ServiceRegistry` & `IService`
- **Event Bus**: `Unity.Core.Events.EventBus` & `IEvent`
- **State Machine**: `Unity.Core.FSM.GameStateMachine`, `IState`, `FSMRunner`
- **Object Pooling**: `Unity.Core.Pool.ObjectPooler` & `IPoolable`
- **Data & Crypto**: `Unity.Core.Data.DataCrypto`, `DataHandler<T>`, `DataManager`
- **Logging**: `Unity.Core.Logging.AppLogger` (stripped in release builds via conditional compilation)
- **Editor Tools**:
  - `Unity.Core.Editor.Tools.AccountSafetyScaffolder`: Store multi-account security & anti-fingerprint checker
  - `Unity.Core.Editor.Tools.PackageInstallerWizard`: UPM Package Manager Hub

## Execution Instructions
1. **Working Directory**: Always set `Cwd` to `E:\Clients\com.unity.core` when running shell commands.
2. **File Paths**: All edits, views, and inspections must target paths within `E:\Clients\com.unity.core\`.
3. **Git Operations**: Run `git` commands inside `E:\Clients\com.unity.core`. Never mix commits with other packages.
4. **Policy & Multi-Account Guidelines**:
   - Maintain anti-fingerprinting measures (no hardcoded shared keys, no static central URLs, dynamic crypto key derivation).
   - Ensure `link.xml` preserves `Unity.Core`.

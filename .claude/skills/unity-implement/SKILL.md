---
name: unity-implement
description: Implement an approved Unity task end to end.
disable-model-invocation: true
---

Implement the work described by the user in the spec or tickets.

## 1. Boundary

- The user names a task file, `docs/<feature-slug>/tasks/<NN>-<slug>.md`. If not, stop and ask the user.
- Confirm the `/unity-cli` skill is available. If not, stop and ask the user.
- Confirm the connected Editor's `com.unity.pipeline` version matches the project. Run `unity pipeline list` and compare its `pipelineVersion` for this project against the `com.unity.pipeline` version in both `Packages/manifest.json` and `Packages/packages-lock.json`. If they don't all match, stop and ask the user.
- Confirm the `/ui-ugui` and `/ui-imgui` skills are available when the task involves UI. If not, stop and ask the user.
- Confirm the repository contains `Assets/` and `ProjectSettings/ProjectVersion.txt`. If not, stop and ask the user.
- Confirm there is an opened editor by running `unity status`. If there is no opened editor, use `unity open --args "-automated"` to open one.
- If the task needs an external package not already referenced by the project (e.g. TextMeshPro/`Unity.TextMeshPro`, or any package requiring an `.asmdef` reference addition, a `Packages/manifest.json` change, or an editor-resource import like TMP Essentials), stop and ask the user.
- Always use `/unity-cli` to recompile after editing C# script and test files (`.cs`).
- Always use `/unity-cli` to edit any non-code file (e.g. `.unity` scenes, `.prefab`, `.asset` ScriptableObjects, `.meta` files, `ProjectSettings/*`) or any `.asmdef` file. DO NOT edit YAML file directly.

## 2. Implement one task

- Always use `/unity-tdd` if possible, at pre-agreed seams.
- Run typechecking regularly, single test files regularly, and the full test suite once at the end by using `/unity-cli`.
- Never trigger a Player build yourself.

## 3. UI work

When the task involves UI, read the matching reference skill first and follow it:

- Use `/ui-ugui` for Runtime/Canvas UI (Canvas, uGUI, RectTransform, Layout Groups, `.prefab` UI).
- Use `/ui-imgui` for Editor IMGUI (`OnGUI`, `OnInspectorGUI`, EditorWindow, custom Inspector, PropertyDrawer).

## 4. Report

Report to the user: Changed files, checks run, results, any validation that remains unavailable, applied `/unity-tdd` or not and the reason for that decision.

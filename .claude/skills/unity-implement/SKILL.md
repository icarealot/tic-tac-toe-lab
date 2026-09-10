---
name: unity-implement
description: Implement a Unity task end to end.
disable-model-invocation: true
---

Implement the task file supplied by the user.

## 1. Boundary

Satisfy every gate before editing. Stop and ask the user when a gate fails:

- The user supplied `docs/<feature-slug>/tasks/<NN>-<slug>.md`.
- `/unity-cli` is available.
- The repository contains `Assets/` and `ProjectSettings/ProjectVersion.txt`.
- The connected Editor and project use the same `com.unity.pipeline` version: compare the project's `pipelineVersion` from `unity pipeline list` with both `Packages/manifest.json` and `Packages/packages-lock.json`.
- For UI work, both `/ui-ugui` and `/ui-imgui` are available.
- The task needs no external package that the project does not already reference, including one requiring an `.asmdef` reference, `Packages/manifest.json` change, or editor-resource import such as TMP Essentials.

Run `unity status`. If no Editor is open, run `unity open --args "-automated"`.

## 2. Implement

- Use `/unity-tdd` whenever possible, at pre-agreed seams.
- Before every focused, task-level, or complete EditMode or PlayMode run, read and follow [test-protocol.md](test-protocol.md).
- Before the first edit, run the task's validation-tier suite and record its summary. Only a test that passed this baseline can later be a regression.
- After every `.cs` edit, use `/unity-cli` to refresh and compile: run `unity command eval 'UnityEditor.AssetDatabase.Refresh();'`, then poll `unity command eval 'return "compiling=" + UnityEditor.EditorApplication.isCompiling + " updating=" + UnityEditor.EditorApplication.isUpdating;'` until both values are `False`. Before testing, confirm each touched assembly in `Library/ScriptAssemblies/` is newer than the edit. `eval` requires an explicit `return`; a trailing expression fails to compile.
- If a Pipeline command cannot connect, rerun `unity status`. When no instance is `ready`, run `unity open --args "-automated"` and poll until one is. An `open` timeout while Unity boots is not fatal.
- Use `/unity-cli` for every non-code or `.asmdef` edit, including scenes, prefabs, ScriptableObjects, `.meta` files, and `ProjectSettings/*`. Edit no Unity YAML directly.
- For UI work, use `/ui-ugui` for Runtime/Canvas UI and `/ui-imgui` for Editor IMGUI.
- Implement every acceptance criterion. Run typechecking, focused tests, and every available task-level check.
- After task-level checks pass, run all EditMode tests and then all PlayMode tests.
- Leave Player builds and human playtests unrun.

## 3. Report

Report changed files; checks and results; unavailable validation; whether `/unity-tdd` was applied and why; and every observed test failure, attributed as pre-existing, flaky, or a regression, with its resolution.

---
name: unity-implement
description: Implement a Unity task end to end.
disable-model-invocation: true
---

Implement the work described by the user in the spec or task.

## 1. Boundary

- The user names a task file, `docs/<feature-slug>/tasks/<NN>-<slug>.md`. If not, stop and ask the user.
- Confirm the `/unity-cli` skill is available. If not, stop and ask the user.
- Confirm the connected Editor's `com.unity.pipeline` version matches the project. Run `unity pipeline list` and compare its `pipelineVersion` for this project against the `com.unity.pipeline` version in both `Packages/manifest.json` and `Packages/packages-lock.json`. If they don't all match, stop and ask the user.
- Confirm the `/ui-ugui` and `/ui-imgui` skills are available when the task involves UI. If not, stop and ask the user.
- Confirm the repository contains `Assets/` and `ProjectSettings/ProjectVersion.txt`. If not, stop and ask the user.
- Confirm there is an opened editor by running `unity status`. If there is no opened editor, use `unity open --args "-automated"` to open one.
- If the task needs an external package not already referenced by the project (e.g. TextMeshPro/`Unity.TextMeshPro`, or any package requiring an `.asmdef` reference addition, a `Packages/manifest.json` change, or an editor-resource import like TMP Essentials), stop and ask the user.

## 2. Implement

- Always use `/unity-tdd` if possible, at pre-agreed seams.
- Always use `/unity-cli` to recompile after every edit to C# script or test files (`.cs`). Run typechecking and focused tests regularly.
- Always use `/unity-cli` to edit any non-code file (e.g. `.unity` scenes, `.prefab`, `.asset` ScriptableObjects, `.meta` files, `ProjectSettings/*`) or any `.asmdef` file. DO NOT edit YAML file directly.
- Use the matching UI skill for UI work: `/ui-ugui` for Runtime/Canvas UI and `/ui-imgui` for Editor IMGUI.
- Implement every acceptance criterion and run every available task-level check.
- After the task-level checks pass, use `/unity-cli` to run **all EditMode and all PlayMode tests**.
- Leave Player builds and human playtests unrun.

### EditMode and PlayMode test protocol

Apply this protocol to every focused, task-level, and complete EditMode or PlayMode test run. When `unity status` reports a connected Editor, run tests through its Pipeline commands so the project lock is not contested:

1. Start the requested mode asynchronously: EditMode uses `unity command run_tests --mode editor --async_tests true --project-path . --format json`; PlayMode uses `unity command run_tests --mode playmode --async_tests true --project-path . --format json`. Add the appropriate `run_tests` filter for a focused run; omit it to run every test in that mode.
2. Poll `unity command test_status --project-path . --format json` and decode `data.result` when it is a JSON string. Treat `data.result.status` as authoritative; do not infer completion from elapsed time, a fixed sleep, or the initial `run_tests` response.
3. When a check requires both modes, wait for EditMode to reach a terminal state before starting PlayMode, then apply the same poll. Live PlayMode is asynchronous because entering Play Mode causes a domain reload; run the two modes separately rather than requesting async `all` mode.
4. At each terminal state, record `summary.total`, `passed`, `failed`, `skipped`, and `inconclusive`, plus any non-passed test details. Check the nested command result as well as the outer CLI envelope; an outer `success` does not override a nested test error. Stop polling and report the summary immediately without waiting for or printing the full per-test payload.
5. A run passes only after every requested mode reaches a terminal state with no test failure or test-run/compile error.

## 3. Report

Report to the user: Changed files, checks run, results, any validation that remains unavailable, applied `/unity-tdd` or not and the reason for that decision.

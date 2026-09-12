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
- For UI work:
  - `/ui-ugui` if the task requires Runtime/Canvas UI.
  - `/ui-imgui` if the task requires Editor IMGUI.
- The task needs no external package that the project does not already reference, including one requiring an `.asmdef` reference, `Packages/manifest.json` change, or editor-resource import such as TMP Essentials.

Run `unity status`. If no Editor is open, run `unity open --args "-automated"`.

## 2. Implement

- Use `/unity-cli` for every non-code or `.asmdef` edit, including scenes, prefabs, ScriptableObjects, `.meta` files, and `ProjectSettings/*`. Never edit Unity YAML directly.
- For UI work:
  - Use `/ui-ugui` for Runtime/Canvas UI.
  - use `/ui-imgui` for Editor IMGUI.
- Use `/unity-tdd` where possible, at pre-agreed seams.
- Read and follow [test-protocol.md](test-protocol.md) for every run in the task.
- After acceptance checks pass, rerun each EditMode or PlayMode suite named by the validation tier.
- Leave Player builds and human playtests unrun and report them as deferred validation.

## 3. Report

Report changed files; checks and results, including the failure diagnoses required by the test protocol; deferred or unavailable validation; and whether `/unity-tdd` was applied and why.

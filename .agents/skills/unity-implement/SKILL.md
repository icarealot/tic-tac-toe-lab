---
name: unity-implement
description: Implement a Unity task end to end.
disable-model-invocation: true
---

Implement the task file supplied by the user.

## 1. Boundary

Satisfy every gate before editing. Stop and ask when a gate fails:

- The user supplied `docs/<feature-slug>/tasks/<NN>-<slug>.md`; read it and its referenced spec completely.
- `/unity-cli` is available.
- For UI work, `/ui-ugui` is available for Runtime/Canvas UI or `/ui-imgui` is available for Editor IMGUI.
- The task needs no external package the project does not already reference, including one requiring an `.asmdef` reference, `Packages/manifest.json` change, or editor-resource import such as TMP Essentials.
- Discover the project standards if not already known.

Run `unity status`. If no Editor is open, ask the user to open it.

## 2. Implement

- Apply the discovered standards to every code change.
- Use `/unity-cli` for every non-code or `.asmdef` edit, including scenes, prefabs, ScriptableObjects, `.meta` files, and `ProjectSettings/*`. Never edit Unity YAML directly.
- Use `/ui-ugui` for Runtime/Canvas UI and `/ui-imgui` for Editor IMGUI.
- Apply `/unity-tdd` to automated behavior selected by the task and discovered testing standard.
- Read and follow [test-protocol.md](test-protocol.md) for every test run in the task.
- After acceptance checks pass, rerun every EditMode or PlayMode suite named by the validation tier.
- Leave Player builds and human playtests unrun and report them as deferred validation.
- Never run `git add`, `git commit`, or `git stash` unless the user explicitly asks for that exact operation.

## 3. Report

Report:

- Changed files.
- Checks and results, including failure diagnoses required by the test protocol.
- Deferred or unavailable validation.
- Whether `/unity-tdd` was applied and why.


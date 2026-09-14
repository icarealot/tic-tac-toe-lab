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

Before coding, map each validation item to the named behavior or risk it protects. An automated test earns its place by protecting a game rule, calculation, meaningful branch, state change, lifecycle, infrastructure contract, or critical player journey. Trivial forwarding, construction, diagnostic text, styling, and incidental hierarchy need no dedicated test. Treat qualitative behavior—feel, visuals, audio, controls, camera behavior, and usability—as human-playtest work rather than claiming an automated test proves it.

Use the smallest sufficient fixture:

1. Plain EditMode with no `GameObject`, asset, frame, coroutine timing, or Unity lifecycle dependency.
2. Isolated PlayMode `GameObject` for focused component or engine behavior.
3. Production prefab only when serialized prefab configuration is part of the contract.
4. Production scene only when bootstrap or cross-object production wiring is the contract.

Resolve any production fixture described by domain role from the current repository. Loading it for validation does not authorize modifying it. Creating or modifying a scene, prefab, `.asmdef`, or other non-code asset merely to support a test requires task-specific approval naming the exact asset.

- Use `/unity-cli` for every non-code or `.asmdef` edit, including scenes, prefabs, ScriptableObjects, `.meta` files, and `ProjectSettings/*`. Never edit Unity YAML directly.
- For UI work:
  - Use `/ui-ugui` for Runtime/Canvas UI.
  - use `/ui-imgui` for Editor IMGUI.
- Use `/unity-tdd` for behavior that qualifies for an automated test, at the owning public seam. Assert independent expected values and avoid repeating the same outcome at every architectural layer.
- Read and follow [test-protocol.md](test-protocol.md) for every run in the task.
- After acceptance checks pass, rerun each EditMode or PlayMode suite named by the validation tier.
- Leave Player builds and human playtests unrun and report them as deferred validation.
- Never run `git add`, `git commit`, or `git stash` unless the user explicitly asks for that exact operation.

## 3. Report

Report changed files; checks and results, including the failure diagnoses required by the test protocol; deferred or unavailable validation; and whether `/unity-tdd` was applied and why. When no dedicated automated test was added, name the policy reason rather than presenting test count or coverage as evidence.

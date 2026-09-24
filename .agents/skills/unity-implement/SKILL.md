---
name: unity-implement
description: Implement a Unity task end to end.
---

Implement exactly one supplied work item.

## 1. Establish the boundary

Read the supplied `docs/<feature>/tasks/<NN>-<slug>.md` and its referenced spec completely.

Before editing, require:

- `/unity-cli`;
- `/ui-ugui` for Runtime/Canvas UI or `/ui-imgui` for Editor IMGUI, when applicable;
- no new external package, `.asmdef` reference, `Packages/manifest.json` change, or editor-resource import such as TMP Essentials.

On a failed gate, return `Status: blocked` with the required user action. Run `unity status`; if no Editor is open, ask the user to open it and return blocked.

Find and apply project standards under `docs/` when present. Trace the existing behavior before editing.

## 2. Implement and validate

- Apply the discovered standards to every change.
- Use `/unity-cli` for every non-code or `.asmdef` change, including scenes, prefabs, ScriptableObjects, `.meta` files, and `ProjectSettings/*`; never edit Unity YAML directly.
- Use `/ui-ugui` for Runtime/Canvas UI and `/ui-imgui` for Editor IMGUI.
- Apply `/unity-tdd` to automated behavior selected by the work item and testing standard.
- Follow [test-protocol.md](test-protocol.md) for every test run.
- After acceptance checks pass, apply the smallest-sufficient-change standard to the task's diff.
- After simplification and disposable-check removal, rerun each EditMode or PlayMode suite required by the task's validation tier.
- Report Player builds and human playtests as deferred rather than running them.

Use repository-root `.scratch/` for all temporary scripts, files, logs, exports, screenshots, and command output. Remove every artifact created for this work item before reporting.

Leave Git state intact unless the user explicitly requests that exact operation: do not run `git add`, `git commit`, or `git stash`.

## 3. Report compactly

Begin with exactly `Status: complete` only when every acceptance criterion is implemented and all available required validation passes; otherwise begin with `Status: blocked` and name the blocker and required action.

Then list only:

- `Changed:` every changed file path;
- `Validation:` deferred or unavailable checks;
- `TDD:` whether `/unity-tdd` was applied and why.

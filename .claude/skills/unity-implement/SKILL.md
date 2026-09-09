---
name: unity-implement
description: Implement a set of tasks through sequential subagents, then review the resulting changes.
disable-model-invocation: true
---

Orchestrate every task in a set of tasks. Keep orchestration in this context; give each task to one fresh subagent and wait for it before dispatching the next.

## 1. Resolve the feature

- The user names a feature folder, `docs/<feature-slug>/`. If not, stop and ask for it.
- Require `<feature-folder>/SPEC.md` and at least one `<feature-folder>/tasks/<NN>-<slug>.md` file. If either is missing, stop and ask the user.
- Sort task files by their numeric filename prefix. This is the execution order.
- Read the spec and every task before preflight.

## 2. Run shared preflight

Run shared checks once, before spawning a worker subagent:

- Confirm the `/unity-cli` skill is available. If not, stop and ask the user.
- Confirm the connected Editor's `com.unity.pipeline` version matches the project. Run `unity pipeline list` and compare its `pipelineVersion` for this project against the `com.unity.pipeline` version in both `Packages/manifest.json` and `Packages/packages-lock.json`. If all three do not match, stop and ask the user.
- Confirm the repository contains `Assets/` and `ProjectSettings/ProjectVersion.txt`. If not, stop and ask the user.
- Run `unity status`. If there is no opened editor, use `unity open --args "-automated"` to open one.
- If any task involves UI, confirm the matching `/ui-ugui` or `/ui-imgui` skill is available. If not, stop and ask the user.

## 3. Dispatch tasks sequentially

For each task in numeric filename order:

1. Spawn a worker subagent. Give it the exact spec path, exact task path, and the worker contract below.
2. Wait for its structured result before doing anything with the next task.
3. Accept `COMPLETE` only when the worker's completion criteria are met.
4. On `BLOCKED` or a failed available check, stop the sequence, preserve the working tree, and report the task and blocker. Do not dispatch later tasks or the inspector.

Never run task workers concurrently.

### Worker contract

Implement only the assigned task:

- Read the complete spec and task, then inspect the relevant project context.
- Use `/unity-tdd` at pre-agreed seams when possible. If it is not applicable, report why.
- Use the matching UI skill for UI work: `/ui-ugui` for Runtime/Canvas UI and `/ui-imgui` for Editor IMGUI.
- If the task needs an external package not already referenced by the project—including an `.asmdef` reference, `Packages/manifest.json` change, or editor-resource import such as TMP Essentials—return `BLOCKED` with the requirement.
- Use `/unity-cli` to recompile after every edit to C# script or test files (`.cs`). Run typechecking and focused tests regularly.
- Use `/unity-cli` for every non-code Unity file (`.unity`, `.prefab`, `.asset`, `.meta`, `ProjectSettings/*`) and every `.asmdef`; edit none of their serialized text directly.
- Implement every acceptance criterion and run every available task-level check.
- After the task-level checks pass, use `/unity-cli` to run the full test suite. Return `BLOCKED` if compilation or any test fails.
- Leave Player builds and human playtests unrun; record them as unavailable validation. They do not block `COMPLETE` when implementation and available automated checks pass.
- Preserve all working-tree changes. Do not commit, stage, unstage, discard, or rewrite task files.

Return exactly these fields to the orchestrator:

```text
Status: COMPLETE | BLOCKED
Task: <task path>
Changed files: <paths, or None>
Checks: <commands and results>
Unavailable validation: <items and reasons, or None>
TDD: <applied and seams, or not applied and reason>
Blocker: <reason, or None>
```

## 4. Dispatch final review

Spawn an inspector subagent and tell it to invoke `/unity-code-review` against the exact feature spec. The inspector must inspect the current staged, unstaged, and untracked files and return its complete `Standards` and `Spec` report in context. Wait for that report

## 5. Report

Report to the user:

- Each task and its `COMPLETE` result
- Changed files by task
- Task-level and per-task full-suite checks with results
- Unavailable validation
- Whether `/unity-tdd` was applied, including seams or the reason it was not
- The inspector's complete `Standards` and `Spec` sections

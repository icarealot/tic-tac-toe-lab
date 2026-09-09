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
4. On `BLOCKED` or a failed available check, stop the sequence, preserve the working tree, and report the task and blocker. Do not dispatch later tasks or the reviewer.

### Worker contract

Implement only the assigned task:

- Read the complete spec and task, then inspect the relevant project context.
- Use `/unity-tdd` at pre-agreed seams when possible. If it is not applicable, report why.
- Use the matching UI skill for UI work: `/ui-ugui` for Runtime/Canvas UI and `/ui-imgui` for Editor IMGUI.
- If the task needs an external package not already referenced by the project—including an `.asmdef` reference, `Packages/manifest.json` change, or editor-resource import such as TMP Essentials—return `BLOCKED` with the requirement.
- Use `/unity-cli` to recompile after every edit to C# script or test files (`.cs`). Run typechecking and focused tests regularly.
- Use `/unity-cli` for every non-code Unity file (`.unity`, `.prefab`, `.asset`, `.meta`, `ProjectSettings/*`) and every `.asmdef`; edit none of their serialized text directly.
- Implement every acceptance criterion and run every available task-level check.
- After the task-level checks pass, use `/unity-cli` to run **all EditMode and all PlayMode tests**. Return `BLOCKED` if compilation, test execution, or any test fails.
- Leave Player builds and human playtests unrun; record them as unavailable validation. They do not block `COMPLETE` when implementation and available automated checks pass.
- Preserve all working-tree changes. Do not commit, stage, unstage, discard, or rewrite task files.

### EditMode and PlayMode test protocol

Apply this protocol to every focused, task-level, and complete EditMode or PlayMode test run. When `unity status` reports a connected Editor, run tests through its Pipeline commands so the project lock is not contested:

1. Start the requested mode asynchronously: EditMode uses `unity command run_tests --mode editor --async_tests true --project-path . --format json`; PlayMode uses `unity command run_tests --mode playmode --async_tests true --project-path . --format json`. Add the appropriate `run_tests` filter for a focused run; omit it to run every test in that mode.
2. Poll `unity command test_status --project-path . --format json` and decode `data.result` when it is a JSON string. Treat `data.result.status` as authoritative; do not infer completion from elapsed time, a fixed sleep, or the initial `run_tests` response.
3. When a check requires both modes, wait for EditMode to reach a terminal state before starting PlayMode, then apply the same poll. Live PlayMode is asynchronous because entering Play Mode causes a domain reload; run the two modes separately rather than requesting async `all` mode.
4. At each terminal state, record `summary.total`, `passed`, `failed`, `skipped`, and `inconclusive`, plus any non-passed test details. Check the nested command result as well as the outer CLI envelope; an outer `success` does not override a nested test error. Stop polling and report the summary immediately without waiting for or printing the full per-test payload.
5. A run passes only after every requested mode reaches a terminal state with no test failure or test-run/compile error.

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

Spawn an reviewer subagent and tell it to invoke `/unity-code-review` against the exact feature spec. The reviewer must inspect the current staged, unstaged, and untracked files and return its complete `Standards` and `Spec` report in context. Wait for that report

## 5. Report

Summarize each worker's structured result, then reproduce the reviewer's complete `Standards` and `Spec` sections.

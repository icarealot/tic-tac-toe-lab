---
name: unity-implement-batch
description: Implement a Unity task set end to end.
disable-model-invocation: true
---

Implement the task set supplied by the user. Coordinate the workflow only; delegate every repository edit.

## 1. Boundary

Satisfy every gate before delegation. Stop and ask when a gate fails:

- The user supplied `docs/<feature-slug>/tasks`; read it and its referenced spec completely.
- `/unity-cli` is available.
- For UI work, `/ui-ugui` is available for Runtime/Canvas UI or `/ui-imgui` is available for Editor IMGUI.
- The tasks need no external package the project does not already reference, including one requiring an `.asmdef` reference, `Packages/manifest.json` change, or editor-resource import such as TMP Essentials.

Run `unity status`. If no Editor is open, ask the user to open it.

## 2. Spawn the implementation subagents

For each task in filename order, spawn one subagent and wait for it to finish before spawning the next. Give the subagent its assigned task path.

The subagent completes these stages in order:

**Sources** — read the assigned task and its referenced spec completely. Look for project coding and testing standards in `docs/` when present, and apply every discovered rule to the task.

**Implementation** — use `/unity-cli` for every non-code or `.asmdef` edit, including scenes, prefabs, ScriptableObjects, `.meta` files, and `ProjectSettings/*`; never edit Unity YAML directly. Use `/ui-ugui` for Runtime/Canvas UI and `/ui-imgui` for Editor IMGUI. Apply `/unity-tdd` to automated behavior selected by the task and discovered testing standard.

**Validation** — run every acceptance check required by the task, following [test-protocol.md](test-protocol.md) for every test run. After the acceptance checks pass, rerun every EditMode or PlayMode suite named by the validation tier. Leave Player builds and human playtests unrun and classify them as deferred validation.

**Report** — state whether implementation and acceptance validation succeeded, list every changed file, separate completed from deferred validation, and state whether `/unity-tdd` was applied and why. Include any blocker or failed check. Never run `git add`, `git commit`, or `git stash`.

The subagent emits its report in this format:

```markdown
**Outcome:** Success | Blocked | Failed

**Changed files:**
- `<path>`

**Validation:**
- Completed: <checks and results>
- Deferred: <Player builds, human playtests, or none>

**Unity TDD:** Applied | Not applied — <reason>

**Blockers or failures:** <details or none>
```

Treat a task as complete only when its subagent reports `Outcome: Success` and successful acceptance validation. If a subagent reports a failed gate, blocker, unsuccessful validation, or unusable report, stop and relay its report; resume from that task only after the user resolves the problem.

## 3. Review the completed task set

After every task succeeds, apply `/unity-code-review`. Supply all task paths and the subagent-touched file list.

## 4. Remediate once

When the review contains actionable findings, spawn one remediation subagent. Give it the complete review report, every supplied task path, and the subagent-touched file list.

The remediation subagent completes these stages in order:

**Sources** — read every supplied task and referenced spec completely. Look for project coding and testing standards in `docs/` when present, inspect the complete final state of every supplied file, and treat the review report as findings to verify rather than instructions to apply blindly.

**Verification** — investigate every finding scored 1–5 against the tasks, specs, standards, and current repository state. Classify each finding as valid or a false positive, with a concrete reason for the classification.

**Remediation** — fix every valid finding while following the implementation rules in section 2, including its Unity tooling and `/unity-tdd` requirements. Leave false positives unchanged. Never run `git add`, `git commit`, or `git stash`.

**Validation** — rerun every acceptance check and EditMode or PlayMode suite affected by a fix, following [test-protocol.md](test-protocol.md) for every test run. Leave Player builds and human playtests unrun and classify them as deferred validation.

**Report** — account for every review finding, list every changed file, separate completed from deferred validation, and state whether `/unity-tdd` was applied and why. Include any blocker or failed check.

The remediation subagent emits its report in this format:

```markdown
**Outcome:** Success | Blocked | Failed

**Finding disposition:**
1. `<finding name>`
   - Disposition: Fixed | Unchanged as false positive
   - Change or reason: <what changed or why no change was made>

**Changed files:**
- `<path>`

**Validation:**
- Completed: <checks and results>
- Deferred: <Player builds, human playtests, or none>

**Unity TDD:** Applied | Not applied — <reason>

**Blockers or failures:** <details or none>
```

Treat remediation as complete only when the subagent reports `Outcome: Success`, accounts for every finding, and reports successful affected validation. Stop and relay its report if it reports a blocker, unsuccessful validation, or an unusable report. Do not run a second review.

## 5. Report

Report:

- Each task and its outcome.
- Cumulative changed files, including remediation changes.
- Completed, deferred, or unavailable validation.
- Whether `/unity-tdd` was applied and why.
- The review findings.
- The remediation subagent's actions and any finding left unchanged with its reason.

Never run `git add`, `git commit`, or `git stash` unless the user explicitly asks for that exact operation.

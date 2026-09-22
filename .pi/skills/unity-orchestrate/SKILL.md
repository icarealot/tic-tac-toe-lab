---
name: unity-orchestrate
description: Implement and review a Unity feature.
disable-model-invocation: true
---

Orchestrate one `docs/<feature>/` directory. Use fresh subagents to discover completed work, implement the remainder, and review the resulting change set. Stop at the first failed gate and report the blocker.

## 1. Preflight

1. Require one feature directory containing `spec.md` and at least one file under `tasks/`.
2. Read the complete spec and every task.
3. Capture `git rev-parse HEAD` as the fixed point. A dirty worktree is valid; scouting uses the current repository state, and review covers every change since the fixed point.

## 2. Discover remaining tasks

Call one fresh `scout` subagent with the feature directory and ask it to read the spec and every task, inspect the current repository, and classify each task in numeric order:

- `COMPLETE` — the requested behavior appears implemented.
- `RUN` — implementation is missing, incomplete, or uncertain.

The scout begins with `Status: complete` and briefly explains each decision, or `Status: blocked` with the blocker. It does not perform detailed validation; workers validate the tasks they run.

Continue only when the scout returns `Status: complete` and covers every task. Otherwise report the scouting blocker.

## 3. Implement

Process each `RUN` task sequentially in numeric order. Call one fresh `worker` subagent per task with:

```text
/skill:unity-implement <task-path>
```

Continue only when the response begins `Status: complete`. Record changed files and deferred or unavailable validation. On failure, stop before starting another task and report progress and the blocker. Tasks classified `COMPLETE` need no worker and retain that status.

## 4. Review

Inspect tracked, staged, committed, and untracked changes against the fixed point. If none exist, record `not run (no changes)` and finish.

Otherwise call one fresh `reviewer` subagent with:

```text
/skill:unity-code-review <fixed-point> <feature-directory>
```

A response of `No findings.` is clean. Treat a failed or incomplete review as a blocker.

## 5. Remediate once

When the first review has findings, pass them unchanged to one fresh `worker` subagent:

```text
/skill:unity-implement Review remediation for <feature-directory>.

Review findings:
<verbatim findings>
```

Continue only when the response begins `Status: complete`; otherwise report the blocker, findings, and validation status. Record remediation changes and finding dispositions.

After successful remediation, run one fresh full review with the original fixed point and feature directory. Report its result without another remediation cycle.

## 6. Report

Return a compact report with:

- each task's `complete` or `blocked` status;
- implementation changed files;
- deferred or unavailable validation, including `HUMAN-PENDING` items;
- first-review result and findings;
- remediation changed files and finding dispositions, when applicable;
- final-review result and remaining findings, when applicable.

Do not run `git add`, `git commit`, or `git stash`.

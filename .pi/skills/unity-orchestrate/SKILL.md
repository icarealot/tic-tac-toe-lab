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

## 2. Discover completed tasks

Call one fresh `scout` subagent with the feature directory, fixed point, and these instructions:

- Read the complete spec and every task, then inspect the current implementation, tests, assets, configuration, validation records, and relevant Git history.
- Judge every acceptance criterion and required validation from repository evidence. Names, checkboxes, commits, test source, and human claims do not prove completion or a successful validation run.
- Use `HUMAN-PENDING` only for work that inherently requires human action or judgment. Missing evidence for an automatable check is `UNPROVEN`.
- Mark a task `COMPLETE` only when every criterion is evidenced or `HUMAN-PENDING`, every validation is proven or `HUMAN-PENDING`, every blocker is complete, and no implementation or automatable validation remains. Mark every other task `RUN`.
- Begin with `Status: complete` or `Status: blocked`. On success, list every task once in numeric order with its `COMPLETE` or `RUN` decision, concise acceptance and validation evidence, any `HUMAN-PENDING` items, and blocker status.

Continue only when scouting returns `Status: complete`, covers every task, and supports every `COMPLETE` decision. Otherwise report the scouting blocker; do not default to running all tasks.

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

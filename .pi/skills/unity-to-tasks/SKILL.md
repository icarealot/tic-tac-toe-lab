---
name: unity-to-tasks
description: Break a plan, spec, or the current conversation into tracer-bullet tasks that declare their blocking edges.
disable-model-invocation: true
---

Break a plan, spec, or conversation into tracer-bullet tasks, each declaring the tasks that block it.

## Process

### 1. Gather context

Use the context already in the conversation. If the user supplies a spec path, read its complete body and comments.

### 2. Explore the codebase when needed

Understand the current code before planning. Use the project's domain glossary vocabulary, apply the project standards and respect applicable ADRs.

Look for opportunities to prefactor: make the change easy, then make the easy change.

### 3. Draft vertical slices

Each tracer-bullet slice:

- Cuts a narrow but complete path through every applicable layer rather than implementing one horizontal layer.
- Is independently demoable or verifiable.
- Fits within one fresh context window.
- Follows any prerequisite prefactoring.
- Derives its validation from the discovered testing standard.

Give every task its blocking edges. A task with no blockers can start immediately.

When a production fixture is selected, identify it by stable domain role and state the distinct configuration or wiring risk. Keep implementation paths out of tasks.

**Wide refactors are the exception to vertical slicing.** A wide refactor is one mechanical change—such as renaming a shared symbol—whose blast radius fans across the codebase so no narrow slice can land green. Sequence it as expand–contract:

1. Expand by adding the new form beside the old so the project remains green.
2. Migrate callers in independently green batches sized by blast radius. Every migration task is blocked by the expansion.
3. Contract by deleting the old form in a task blocked by every migration.

When migration batches cannot remain green independently, preserve the sequence on an integration branch and make every batch block a final integrate-and-verify task.

### 4. Quiz the user

Present the proposed breakdown as a numbered list. For every task show:

- **Title**: a short descriptive name.
- **Blocked by**: only tasks that genuinely gate it.
- **What it delivers**: independently verifiable end-to-end behavior.
- **Validation tier**: the feature-specific validation decision and rationale derived from the standards.

Ask whether the granularity and blocking edges are right and whether any tasks should be merged or split. Iterate until approved.

### 5. Create task files

Write one file per task under `docs/<feature-slug>/tasks/<NN>-<slug>.md`, numbered from `01` in dependency order. Each file lists its blockers by number and title.

Work the frontier: any task whose blockers are complete can start.

<task-template>

# <NN> — <task title>

## Spec

Reference the source spec.

## What to build

Describe the independently verifiable end-to-end behavior from the user's perspective.

## Blocked by

Reference each blocking task, or write "None (can start immediately)."

## Validation tier

Record the feature-specific checks and rationale derived from the project standards.

## Acceptance criteria

- [ ] Acceptance criterion 1
- [ ] Acceptance criterion 2

</task-template>

Keep implementation file paths and code snippets out of task files because they become stale.

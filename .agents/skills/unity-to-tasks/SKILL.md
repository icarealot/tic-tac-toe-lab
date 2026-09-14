---
name: unity-to-tasks
description: Break a plan, spec, or the current conversation into a set of tracer-bullet tasks, each declaring its blocking edges.
disable-model-invocation: true
---

Break a plan, spec, or conversation into a set of **tasks**: tracer-bullet vertical slices, each declaring the tasks that **block** it.

## Process

### 1. Gather context

Work from whatever is already in the conversation context. If the user passes a spec path as an argument, fetch it and read its full body and comments.

### 2. Explore the codebase (optional)

If you have not already explored the codebase, do so to understand the current state of the code. task titles and descriptions should use the project's domain glossary vocabulary, and respect ADRs in the area you're touching.

Look for opportunities to prefactor the code to make the implementation easier. "Make the change easy, then make the easy change."

### 3. Draft vertical slices

Break the work into **tracer bullet** tasks.

<vertical-slice-rules>

- Each slice cuts a narrow but COMPLETE path through every applicable layer (schema, API, UI, validation): vertical, NOT a horizontal slice of one layer
- A completed slice is demoable or verifiable on its own
- Each slice is sized to fit in a single fresh context window
- Any prefactoring should be done first

</vertical-slice-rules>

Give each task its **blocking edges** — the other tasks that must complete before it can start. A task with no blockers can start immediately.

For each slice, select validation by risk rather than requiring an automated test by default. Retain a test only when it protects a named game rule, calculation, meaningful branch, state change, lifecycle, infrastructure contract, or critical player journey. Trivial forwarding, construction, diagnostic text, styling, and incidental hierarchy do not justify dedicated tests.

Choose the smallest sufficient fixture: plain EditMode; isolated PlayMode `GameObject`; production prefab when serialized configuration is the contract; production scene only for bootstrap or cross-object production wiring. Assert a rule at its owning layer instead of repeating the same outcome through every layer. Reserve player builds for platform-sensitive behavior and human playtests for feel, visuals, audio, controls, camera behavior, and usability.

When a production prefab or scene is necessary, identify it by stable domain role and state the distinct configuration or wiring risk. Keep implementation paths out of the task.

**Wide refactors are the exception to vertical slicing.** A **wide refactor** is one mechanical change (rename a column, retype a shared symbol) whose **blast radius** fans across the whole codebase, so a single edit breaks thousands of call sites at once and no vertical slice can land green. Don't force it into a tracer bullet; sequence it as **expand–contract**. First expand: add the new form beside the old so nothing breaks. Then migrate the call sites over in batches sized by blast radius (per package, per directory), each batch its own task blocked by the expand, keeping CI green batch to batch because the old form still exists. Finally contract: delete the old form once no caller remains, in a task blocked by every migrate batch. When even the batches can't stay green alone, keep the sequence but let them share an integration branch that all block a final integrate-and-verify task; green is promised only there.

### 4. Quiz the user

Present the proposed breakdown as a numbered list. For each task, show:

- **Title**: short descriptive name
- **Blocked by**: which other tasks (if any) must complete first
- **What it delivers**: the end-to-end behaviour this task makes work
- **Validation tier**: the named behavior, smallest sufficient fixture, and applicable EditMode, PlayMode, Player build, and Human playtest checks; include the reason when no dedicated automated test is warranted

Ask the user:

- Does the granularity feel right? (too coarse / too fine)
- Are the blocking edges correct: does each task only depend on tasks that genuinely gate it?
- Should any tasks be merged or split further?

Iterate until the user approves the breakdown.

### 5. Create the tasks

- Write one file per task under `docs/<feature-slug>/tasks//<NN>-<slug>.md`, numbered from `01` in dependency order (blockers first). Each file's "Blocked by" lists the numbers/titles it depends on. Use the per-task file template below — one task per file, never a single combined file.

Work the **frontier**: any task whose blockers are all done. For a purely linear chain that means top to bottom.

<task-template>

# <NN> — <task title>

## Spec

A reference to the spec.

## What to build

The end-to-end behaviour this ticket makes work, from the user's perspective, not layer-by-layer implementation.

## Blocked by

A reference to each blocking task, or "None (can start immediately)".

## Validation tier

For each check, name the behavior or risk, its owning seam, and the smallest sufficient fixture: EditMode, isolated PlayMode, production prefab or scene identified by domain role, Player build, or Human playtest. State why any changed behavior has no dedicated automated test.

## Acceptance criteria

- [ ] Acceptance criterion 1
- [ ] Acceptance criterion 2

</task-template>

In either form, avoid implementation file paths or code snippets because they go stale.

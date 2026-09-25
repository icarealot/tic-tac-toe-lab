---
name: unity-code-review
description: Spawn a subagent to review staged Unity changes against coding, specification, and testing requirements.
disable-model-invocation: true
---

Spawn a subagent to review the current Git index against applicable coding, specification, and testing requirements. The staged diff is the review boundary; use other files only as evidence.

## 1. Establish the review set

- Inspect `git status --short` and `git diff --cached`, including staged additions, modifications, deletions, and renames.
- If there are no staged changes, report exactly `No staged changes to review.` and stop.
- Read every staged reviewable text file completely. For binaries, inspect their identities and applicable Unity metadata or repository tooling; never load binary payloads.
- Do not treat a supplied task, feature directory, file list, project scope, or unstaged/untracked change as a review target. Read related assets, configuration, prefabs, callers, and tests only to understand staged behavior.

## 2. Establish sources and evidence

- Find and read completely the single relevant task, specification, context, and decision sources. Each review covers one task.
- If no task/specification source exists, omit the Spec evaluation.
- Find project standards under `docs/`; omit the corresponding evaluation when its source is absent.
- Gather evidence once. Evaluate every relevant implementation detail and caller-visible behavior independently against each applicable axis without rereading the complete staged review set between axes.

## 3. Evaluate

### Standards

Apply every documented coding rule. Limit smallest-sufficient-change findings to staged additions and old paths made obsolete by the staged change; omit unrelated legacy cleanup.

For each violation, record the rule and evidence, the risk it hides, and a concrete fix.

### Spec

Report missing, partial, extra, or incorrect behavior. Quote the controlling task or specification requirement and give a concrete fix.

### Testing

Apply every testing rule to production and test code in the staged review set. Review only behavior that automation can establish. Omit human-judgment checks, including missing, deferred, incomplete, or unsigned human checks; never request human checklists or sign-off.

For each caller-visible behavior within the automated boundary:

1. Name its risk from the implementation and spec contract.
2. Select the cheapest sufficient test level—unit, integration, or E2E—and smallest fixture.
3. Verify that the tests provide sufficient evidence.

Judge automated validation evidence rather than spec correctness; report a spec defect only when this evaluation exposes one. Report each retained test that violates a rule and each behavior lacking sufficient automated validation, with evidence and a concrete fix. For missing automation within the testing standard's automated boundary, name the behavior and sufficient fixture.

Merge issues found on multiple axes and apply all relevant tags instead of duplicating them.

### Score and tag

- **5** — wrong or missing behavior, spec violation, or broken invariant
- **4** — uncovered defect risk or hard standards violation, including complexity that can hide bugs
- **3** — clear standards or testing-standard violation, including unjustified maintenance surface
- **2** — minor convention drift or removable indirection
- **1** — style nit
- **0** — omit

Tag findings `[Standards]`, `[Spec]`, `[Testing]`, or a combination.

## 4. Report findings

Sort findings by score descending and report only this flat list in chat:

```markdown
1. **[5] [Spec + Testing] <finding name>**

- `<quoted requirement>`
- `<path/to/file>`
- <Risk or hidden deviation>.
- Action: <concrete fix>.
```

Report exactly `No findings.` when the staged changes are clean.

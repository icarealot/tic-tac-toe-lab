---
name: unity-code-review
description: Review Unity changes against coding, specification, and testing requirements.
---

Review changes since a supplied fixed point through independent **Standards**, **Spec**, and **Testing** evaluations. Score, merge, tag, and sort the findings.

## 1. Capture the change set

- Ask for a fixed point when absent, then confirm it with `git rev-parse <fixed-point>`.
- Capture once: `git diff <fixed-point>`, `git diff --numstat <fixed-point>`, `git status --short`, `git ls-files --others --exclude-standard`, and `git log <fixed-point>..HEAD --oneline`.
- Treat tracked worktree changes, staged changes, later commits, and untracked files as one change set.
- Read every reviewable untracked text file completely. For changed binary files, inspect identities plus applicable Unity metadata or repository tooling; do not load binary patch payloads.
- Stop on an invalid ref or when no tracked or untracked change exists.

## 2. Establish sources and evidence

Classify the supplied scope:

- **Task** — read the task and referenced spec completely.
- **Feature** — read every file in the supplied feature directory completely.
- **Project** — with no task or feature source, review the whole change set and omit the Spec evaluation.

Find project coding and testing standards under `docs/`; omit an evaluation whose source is absent. Inspect every full changed file and, where relevant, its assets, configuration, prefabs, callers, and tests.

Gather this evidence once. Then evaluate every changed hunk and caller-visible behavior independently against each applicable axis below; do not recapture or reread the complete change set between axes.

## 3. Evaluate

### Standards

Apply every documented coding rule. For each violation, record the rule and evidence, the risk it hides, and a concrete fix.

### Spec

Report missing, partial, extra, or incorrect behavior. Quote the controlling task or spec requirement and give a concrete fix.

### Testing

Apply every testing rule to changed production and test code. For each changed caller-visible behavior:

1. Name its risk from the diff and spec contract.
2. Decide whether automation or human judgment can establish it.
3. Select the cheapest sufficient validation level and smallest fixture.
4. Verify that the selected check and evidence are recorded.

Judge validation evidence rather than spec correctness; report a spec defect only when this evaluation exposes one. Report each retained test that violates a rule and each behavior lacking sufficient validation, with evidence and a concrete fix. For missing automation, name the behavior and sufficient fixture; for human judgment, give the concrete checklist. Accept omitted validation only when its reason is recorded.

Merge an issue seen on multiple axes and add all applicable tags instead of duplicating it.

### Score and tag

- **5** — wrong or missing behavior, spec violation, or broken invariant
- **4** — uncovered defect risk or hard standard violation that can hide bugs
- **3** — clear standards or testing-standard violation; quality risk
- **2** — minor convention drift
- **1** — style nit
- **0** — omit

Tag findings `[Standards]`, `[Spec]`, `[Testing]`, or a combination.

## 4. Return only findings

Sort by score descending and return only this flat list:

```markdown
1. **[5] [Spec + Testing] <finding name>**

- `<quoted requirement>` / `<path/to/file>`
- <Risk or hidden deviation>.
- Action: <concrete fix>.
```

Return exactly `No findings.` when clean.

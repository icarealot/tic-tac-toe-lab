---
name: unity-code-review
description: Review changes since a fixed point along independent coding, specification, and testing axes using subagent with sequential passes, severity scores, and axis tags.
disable-model-invocation: true
---

Review changes since a fixed point—a commit, branch, tag, or merge-base—along multiple independent axes:

- **Standards** — conformance to the discovered coding standard.
- **Spec** — fidelity to the originating spec.
- **Testing** — conformance to the discovered testing standard.

The subagent checks each axis in a separate sequential pass against its own source, scores every finding 0–5 by severity, merges duplicates, and emits the final sorted report.

## 1. Pin the fixed point

Use the fixed point supplied by the user. If none was supplied, ask for it.

Capture these commands once:

- Diff: `git diff <fixed-point>...HEAD`
- Commits: `git log <fixed-point>..HEAD --oneline`

Confirm the fixed point resolves with `git rev-parse <fixed-point>` and the diff is non-empty. Stop on a bad ref or empty diff before spawning the subagent.

## 2. Identify the sources

Look for these sources in `docs/` when present:

- Project standards
- Originating spec

If a source is missing, skip that axis's pass.

## 3. Spawn the review subagent

Give the subagent the captured diff command, commit list, any temporary standards exception relevant to an axis, and the complete contents of every discovered source.

The subagent runs three sequential passes, one per axis, each against its own source:

**Standards pass** — apply every documented coding rule and heuristic to every changed code hunk. Report each hard violation with the exact rule, and label heuristic findings as judgment calls. Quote the relevant hunk, explain what it could hide, and suggest a concrete fix. Skip checks already enforced by tooling.

**Spec pass** — report requirements that are missing or partial, behavior not requested by the spec, and requirements whose implementation appears incorrect. Quote the relevant spec line for every finding and suggest a concrete fix.

**Testing pass** — apply every testing rule to changed production and test code. Report each retained test that violates one and each changed behavior that warrants validation but remains unprotected. Quote the relevant hunk, name the behavior and sufficient fixture, and suggest a concrete fix. Account for every behavior or assertion in each added or modified test retained at `HEAD`: after searching all repository test code—not only the diff—mark it unique or place it in an overlap group containing a changed test. Report groups with redundant coverage as ordinary findings, naming the shared behavior and the exact overlapping test methods, and recommending consolidation or removal.

**Extend, don't repeat.** If a later pass finds an issue already reported in an earlier pass, extend the existing finding with the new axis tag and any new detail. Never create a duplicate entry.

**Score every finding 0–5 by severity:**

- **5** — wrong or missing behavior, violates a spec requirement, or breaks an invariant
- **4** — defect risk the tests don't cover; hard standards violation that can hide bugs
- **3** — clear standards or testing-standard violation, quality risk
- **2** — minor convention drift
- **1** — style nit
- **0** — not reportable; omit the finding

Tag every finding with the axis that caught it: `[Standards]`, `[Spec]`, `[Testing]`, or combined, e.g. `[Spec + Testing]`.

Emit the final report as a flat list sorted by score descending.

## 4. Relay the report

The subagent authors every finding end to end—name, score, tag, evidence, and fix suggestion—then merges duplicates and emits the final report. The main agent fills nothing in and relays it verbatim.

```markdown
1. **[5] [Spec + Testing] <finding name>**

- `<quoted spec line>` / `<path/to/file>`
- <What the deviation could hide>.
- Action: <the subagent's suggested fix>.
```

## Why multiple axes

A change can pass any axis and fail another:

- Correctly styled code can implement the wrong behavior.
- Spec-compliant behavior can violate project conventions.
- Correct behavior and style can still rely on brittle or low-value tests.

Sequential passes keep one lens from masking another: each pass re-reads the diff against its own source. Tags keep every finding traceable to the source that caught it, and the severity score makes cross-axis priority obvious at a glance.

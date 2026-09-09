---
name: unity-code-review
description: Review staged, unstaged, and untracked Unity project changes when asked for a working-tree code review or when unity-implement requests its final review. Evaluate Standards and Spec independently and return the report.
---

Review the current working-tree changes along two axes:

- **Standards** — does the change conform to [CODING_STANDARDS.md](CODING_STANDARDS.md)?
- **Spec** — does the change faithfully implement the originating spec?

Inspect the change once, complete two independent passes in this context—Standards followed by Spec—then return the report to the caller.

## 1. Capture the working-tree change

Capture all three working-tree states:

- `git diff HEAD --` for the combined staged and unstaged tracked changes.
- `git ls-files --others --exclude-standard` for untracked files; read each reviewable untracked file directly.
- `git status --short` as the exhaustive changed-file inventory.

Review no committed-only changes. Account for every path in the inventory. If the inventory is empty, return `No working-tree changes to review.` and stop. Record binary or unreadable files as unavailable rather than silently omitting them.

## 2. Identify the spec source

Use the originating spec in this order:

1. The exact spec path passed by the caller.
2. `SPEC.md` in a feature folder passed by the caller.
3. A spec under `docs/`, `specs/`, or `.scratch/` matching the branch name or feature.
4. If no spec is found, stop and ask the caller.

Read the selected spec completely.

## 3. Standards source

The Standards axis always carries [CODING_STANDARDS.md](CODING_STANDARDS.md) and the **smell baseline** below. The bundled file overrides the baseline: where it endorses something the baseline would flag, suppress the smell. Skip anything tooling already enforces.

Baseline smells are labelled judgement calls, never hard violations:

- **Mysterious Name** — a name does not reveal what it does or holds. Rename it; if no honest name emerges, clarify the design.
- **Duplicated Code** — the same logic shape appears in multiple changed locations. Extract and share the shape.
- **Feature Envy** — a method reaches into another object's data more than its own. Move the behavior toward that data.
- **Data Clumps** — the same fields or parameters repeatedly travel together. Bundle them into a type.
- **Primitive Obsession** — a primitive or string substitutes for a domain concept. Introduce a small domain type.
- **Repeated Switches** — the same conditional dispatch recurs. Replace it with polymorphism or one shared map.
- **Shotgun Surgery** — one logical change requires scattered edits. Gather what changes together.
- **Divergent Change** — one module changes for unrelated reasons. Split its responsibilities.
- **Speculative Generality** — abstractions, parameters, or hooks serve no requirement. Remove or inline them.
- **Message Chains** — navigation such as `a.b().c().d()` leaks structure. Hide the walk behind behavior.
- **Middle Man** — a type or function mostly delegates. Call the real target directly.
- **Refused Bequest** — a subtype ignores most inherited behavior. Prefer composition.

## 4. Review the change

Use the captured tracked diff and untracked contents. Complete one pass before starting the next, and rank findings within each pass.

### Pass 1 — Standards

Use only the change, [CODING_STANDARDS.md](CODING_STANDARDS.md), and the smell baseline. Report:

- Every documented-standard violation, citing the file and rule.
- Every applicable baseline smell, naming the smell and quoting the changed location.

Distinguish hard documented violations from judgement-call smells.

### Pass 2 — Spec

Use only the change and selected spec. Report:

- Missing or partially implemented requirements.
- Behavior the spec did not request.
- Requirements that appear implemented incorrectly.

Quote the spec requirement for every finding.

## 5. Return the report

Return the complete report directly to the caller:

```markdown
## Standards

<ranked findings, or "No findings.">

## Spec

<ranked findings, or "No findings.">
```

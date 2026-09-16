---
name: unity-code-review
description: Review changes since a fixed point along independent coding, specification, and testing axes using parallel sub-agents.
disable-model-invocation: true
---

Review changes since a fixed point—a commit, branch, tag, or merge-base—along multiple independent axes:

- **Standards** — conformance to the discovered coding standard.
- **Spec** — fidelity to the originating spec.
- **Testing** — conformance to the discovered testing standard.

Run the axes as parallel sub-agents so their contexts remain independent, then aggregate their findings.

## 1. Pin the fixed point

Use the fixed point supplied by the user. If none was supplied, ask for it.

Capture these commands once:

- Diff: `git diff <fixed-point>...HEAD`
- Commits: `git log <fixed-point>..HEAD --oneline`

Confirm the fixed point resolves with `git rev-parse <fixed-point>` and the diff is non-empty. Stop on a bad ref or empty diff before spawning sub-agents.

## 2. Identify the sources

Look for these sources in `docs/` when present:

- Project standards
- Originating spec

If either is missing, skip this axis.

## 3. Spawn all review axes in parallel

Give every sub-agent the captured diff command, commit list, and any temporary standards exception relevant to its axis.

**Standards sub-agent**

Pass the complete discovered coding standards contents and this brief:

> Review every changed code hunk against every documented coding rule and heuristic. Report each hard violation with the exact rule, and label heuristic findings as judgment calls. Quote the relevant hunk, explain what it could hide, and suggest an action. Skip checks already enforced by tooling.

**Spec sub-agent**

Pass the spec path or complete fetched contents and this brief:

> Report requirements that are missing or partial, behavior not requested by the spec, and requirements whose implementation appears incorrect. Quote the relevant spec line for every finding and suggest an action.

**Testing sub-agent**

Pass the complete discovered testing standards contents and this brief:

> Apply every testing rule to changed production and test code. Report each retained test that violates one and each changed behavior that warrants validation but remains unprotected. Quote the relevant hunk, name the behavior and sufficient fixture, and suggest an action.
>
> Account for every behavior or assertion in each added or modified test retained at `HEAD`: after searching all repository test code—not only the diff—mark it unique or place it in an overlap group containing a changed test. Report every group, including justified ones. Name the shared behavior; identify exact test methods and assertions at `file:line`; state each fixture's distinct integration, configuration, lifecycle, or wiring risk; and classify each relationship as **Justified** or **Redundant**. Label groups containing both **Mixed**. Recommend retaining justified coverage and consolidating or removing redundant coverage.

## 5. Aggregate

Merge findings in axis order. Omit any axis without findings.

```markdown
## Standards

1. **<finding name>**

- `<path/to/file>`
- <What the issue could hide>.
- Action: <suggested fix>.

## Spec

1. **<finding name>**

- `<quoted spec requirement>`
- <What the deviation could hide>.
- Action: <suggested fix>.

## Testing

1. **<finding name or Coverage overlap — behavior [Justified, Redundant, or Mixed]>**

- `<path/to/file>`
- <The behavior left unprotected, assertion problem, or distinct risk protected by each overlapping fixture>.
- Action: <suggested fix, retention, consolidation, or removal>.
```

## Why multiple axes

A change can pass any axis and fail another:

- Correctly styled code can implement the wrong behavior.
- Spec-compliant behavior can violate project conventions.
- Correct behavior and style can still rely on brittle or low-value tests.

Separate axes prevent one result from masking another.

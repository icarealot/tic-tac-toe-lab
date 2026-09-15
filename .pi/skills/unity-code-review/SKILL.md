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

## 2. Identify the spec source

Look for the originating spec in this order:

1. A path supplied by the user.
2. A spec under `docs/`, `specs/`, or `.scratch/` matching the branch name or feature.
3. Ask the user when none is found. If the user confirms there is no spec, skip the Spec axis and report that no spec was available.

## 3. Identify the standards

Discover the project standards if not already known.

## 4. Spawn all review axes in parallel

Give every sub-agent the captured diff command, commit list, and any temporary standards exception relevant to its axis.

**Standards sub-agent**

Pass the complete discovered coding standards contents and this brief:

> Review every changed code hunk against every documented coding rule and heuristic. Report each hard violation with the exact rule, and label heuristic findings as judgment calls. Quote the relevant hunk, explain what it could hide, and suggest an action. Skip checks already enforced by tooling.

**Spec sub-agent**

Pass the spec path or complete fetched contents and this brief:

> Report requirements that are missing or partial, behavior not requested by the spec, and requirements whose implementation appears incorrect. Quote the relevant spec line for every finding and suggest an action.

**Testing sub-agent**

Pass the complete discovered testing standards contents and this brief:

> Review all changed production and test code against every documented testing rule. Report every retained test that violates the standard and every changed behavior that the standard says warrants validation but leaves unprotected. Quote the relevant hunk, name the behavior involved, identify the sufficient fixture, and suggest an action.

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

1. **<finding name>**
- `<path/to/file>`
- <The behavior left unprotected or the assertion problem>.
- Action: <suggested fix>.
```

## Why multiple axes

A change can pass any axis and fail another:

- Correctly styled code can implement the wrong behavior.
- Spec-compliant behavior can violate project conventions.
- Correct behavior and style can still rely on brittle or low-value tests.

Separate axes prevent one result from masking another.

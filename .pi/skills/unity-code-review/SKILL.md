---
name: unity-code-review
description: Review changes since a fixed point along three independent axes — coding standards, specification fidelity, and testing quality — using parallel sub-agents.
disable-model-invocation: true
---

Review the changes since a fixed point (commit, branch, tag, or merge-base) along three axes:

- **Standards** — does the code conform to [coding-standards.md](coding-standards.md)?
- **Spec** — does the code faithfully implement the originating spec?
- **Testing** — do the tests protect meaningful behavior at the smallest sufficient fixture?

All axes run as **parallel sub-agents** so they don't pollute each other's context, then this skill aggregates their findings.

## Process

### 1. Pin the fixed point

Whatever the user said is the fixed point — a commit SHA, branch name, tag, `main`, `HEAD~5`, etc. If they didn't specify one, ask for it.

Capture the diff command once: `git diff <fixed-point>...HEAD` (three-dot, so the comparison is against the merge-base). Also note the list of commits via `git log <fixed-point>..HEAD --oneline`.

Before going further, confirm the fixed point resolves (`git rev-parse <fixed-point>`) and the diff is non-empty. A bad ref or empty diff should fail here — not inside the parallel sub-agents.

### 2. Identify the spec source

Look for the originating spec, in this order:

1. A path the user passed as an argument.
2. A spec file under `docs/`, `specs/`, or `.scratch/` matching the branch name or feature.
3. If nothing is found, ask the user. If they say there isn't one, the **Spec** sub-agent will skip and report "no spec available".

### 3. Standards source

The Standards axis always carries [coding-standards.md](coding-standards.md), including its code-smell heuristics.

### 4. Testing source

The Testing axis always carries [testing-standards.md](testing-standards.md). Apply every rule in that file to changed production and test code.

### 5. Spawn all sub-agents in parallel

**Standards sub-agent prompt** — include:

- The full diff command and commit list.
- [coding-standards.md](coding-standards.md) pasted in full — the sub-agent has no other access to it.
- The brief: "Report — per file/hunk where relevant — (a) every place the diff violates a documented standard: cite the standard (file + the rule); and (b) any baseline smell you spot: name it and quote the hunk. Distinguish hard violations from judgement calls — documented-standard breaches can be hard, but baseline smells are always judgement calls, and a documented repo standard overrides the baseline. Skip anything tooling enforces. Under 400 words."

**Spec sub-agent prompt** — include:

- The full diff command and commit list.
- The path or fetched contents of the spec.
- The brief: "Report: (a) requirements the spec asked for that are missing or partial; (b) behaviour in the diff that wasn't asked for (scope creep); (c) requirements that look implemented but where the implementation looks wrong. Quote the spec line for each finding. Under 400 words."


**Testing sub-agent prompt** — include:

- The full diff command and commit list.
- The path or fetched contents of the spec and task validation tier when available.
- [testing-standards.md](testing-standards.md) pasted in full — the sub-agent has no other access to it.
- The brief: "Review changed production and test code against every testing-baseline rule. Report (a) retained tests that lack a qualifying named behavior; (b) fixtures larger than necessary; (c) implementation-coupled, duplicated, or non-independent assertions; and (d) changed behavior whose named risk warrants but lacks a test. For every finding, quote the relevant hunk, name the protected or unprotected behavior, and state the smallest sufficient fixture. Do not request tests based on count, coverage, or bug history. Under 400 words."

### 6. Aggregate

Merge the three axis reports into one report of findings, in axis order. Each axis with findings gets its template section; the report ends after the last finding.

A finding is a named deviation with a file (or spec line), one line on what it could hide, and a suggested action. Keep that shape and nothing else: recaps of rules or acceptance criteria verified as satisfied, "none found" lines, clean-axis notes, run summaries, and notes the sub-agent itself justified as correct are conversation material, not report content. An axis whose sub-agent found nothing contributes no section, and an all-clean review reports nothing.

#### Report template

Omit the section of any axis without findings; a real report may contain only some of these sections.

```markdown
## Standards

1. **<finding name>**
- `<path/to/file>`
- <one line: what it could hide>.
- Action: <suggested fix>.

## Spec

1. **<finding name>**
- `<spec line it deviates from>`
- <one line: what it could hide>.
- Action: <suggested fix>.

## Testing

1. **<finding name>**
- `<path/to/file>`
- <the behavior left unprotected or the assertion problem>.
- Action: <suggested fix>.
```

## Why three axes

A change can pass any axis and fail another:

- Code that follows every standard but implements the wrong thing → **Standards pass, Spec fail.**
- Code that implements the requested behavior but breaks project conventions → **Spec pass, Standards fail.**
- Code that matches the spec and style rules while relying on brittle or low-value tests → **Standards and Spec pass, Testing fail.**

Reporting them separately stops one axis from masking another.

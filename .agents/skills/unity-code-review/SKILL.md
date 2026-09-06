---
name: unity-code-review
description: Review the changes since a fixed point (commit, branch, tag, or merge-base) along two axes — Standards (does the code follow CODING_STANDARDS.md?) and Spec (does the code match what the originating spec asked for?) then write the report.
disable-model-invocation: true
---

Review the changes since a fixed point (commit, branch, tag, or merge-base) along two axes:

- **Standards** — does the code conform to [CODING_STANDARDS.md](CODING_STANDARDS.md)?
- **Spec** — does the code faithfully implement the originating spec?

Read the diff once, then perform two independent passes in this context — Standards followed by Spec — so findings stay separated. Write the report to `CODE_REVIEW.md` in the selected spec's folder.

## Process

### 1. Pin the fixed point

Whatever the user said is the fixed point — a commit SHA, branch name, tag, `main`, `HEAD~5`, etc. If they didn't specify one, ask for it.

Capture the diff command once: `git diff <fixed-point>...HEAD` (three-dot, so the comparison is against the merge-base). Also note the list of commits via `git log <fixed-point>..HEAD --oneline`.

Before going further, confirm the fixed point resolves (`git rev-parse <fixed-point>`) and the diff is non-empty. A bad ref or empty diff stops the review before analysis begins.

### 2. Identify the spec source

Look for the originating spec, in this order:

1. A path the user passed as an argument.
2. A spec file under `docs/`, `specs/`, or `.scratch/` matching the branch name or feature.
3. If nothing is found, stop and ask the user.

### 3. Standards source

The Standards axis always carries [CODING_STANDARDS.md](CODING_STANDARDS.md) and the **smell baseline** below — a fixed set of Fowler code smells (_Refactoring_, ch.3) that applies on top of the bundled file. Two rules bind it:

- **The bundled file overrides.** [CODING_STANDARDS.md](CODING_STANDARDS.md) always wins; where it endorses something the baseline would flag, suppress the smell.
- **Always a judgement call.** Each smell is a labelled heuristic ("possible Feature Envy"), never a hard violation — and, like any standard here, skip anything tooling already enforces.

Each smell reads *what it is* → *how to fix*; match it against the diff:

- **Mysterious Name** — a function, variable, or type whose name doesn't reveal what it does or holds. → rename it; if no honest name comes, the design's murky.
- **Duplicated Code** — the same logic shape appears in more than one hunk or file in the change. → extract the shared shape, call it from both.
- **Feature Envy** — a method that reaches into another object's data more than its own. → move the method onto the data it envies.
- **Data Clumps** — the same few fields or params keep travelling together (a type wanting to be born). → bundle them into one type, pass that.
- **Primitive Obsession** — a primitive or string standing in for a domain concept that deserves its own type. → give the concept its own small type.
- **Repeated Switches** — the same `switch`/`if`-cascade on the same type recurs across the change. → replace with polymorphism, or one map both sites share.
- **Shotgun Surgery** — one logical change forces scattered edits across many files in the diff. → gather what changes together into one module.
- **Divergent Change** — one file or module is edited for several unrelated reasons. → split so each module changes for one reason.
- **Speculative Generality** — abstraction, parameters, or hooks added for needs the spec doesn't have. → delete it; inline back until a real need shows.
- **Message Chains** — long `a.b().c().d()` navigation the caller shouldn't depend on. → hide the walk behind one method on the first object.
- **Middle Man** — a class or function that mostly just delegates onward. → cut it, call the real target direct.
- **Refused Bequest** — a subclass or implementer that ignores or overrides most of what it inherits. → drop the inheritance, use composition.

### 4. Review the diff

Read the captured diff once. Complete each pass before starting the next. Use only the sources listed for that pass, and rank findings within that pass.

**Pass 1 — Standards.** Use the diff, [CODING_STANDARDS.md](CODING_STANDARDS.md), and the smell baseline. Report — per file/hunk where relevant — (a) every place the diff violates a documented standard: cite the standard (file + the rule); and (b) any baseline smell you spot: name it and quote the hunk. Distinguish hard violations from judgement calls — documented-standard breaches can be hard, but baseline smells are always judgement calls, and [CODING_STANDARDS.md](CODING_STANDARDS.md) overrides the baseline. Skip anything tooling enforces. Draft the findings under `## Standards`.

**Pass 2 — Spec.** Use the diff and the selected spec. Report: (a) requirements the spec asked for that are missing or partial; (b) behaviour in the diff that wasn't asked for (scope creep); and (c) requirements that look implemented but where the implementation looks wrong. Quote the spec line for each finding. Draft the findings under `## Spec`.

### 5. Write the report

Write `docs/<feature-name>/CODE_REVIEW.md` with format:

```markdown
## Standards

<Standards findings>

## Spec

<Spec findings>

<one-line summary: total findings per axis and the worst issue within each axis, if any>
```

Keep findings in their separate sections and rank each axis independently. The summary is the only cross-axis operation: name the worst issue within each axis without choosing a single overall winner.

After the write succeeds, respond with the file path.

## Why two axes

A change can pass one axis and fail the other:

- Code that follows every standard but implements the wrong thing → **Standards pass, Spec fail.**
- Code that does exactly what the issue asked but breaks the project's conventions → **Spec pass, Standards fail.**

Reporting them separately stops one axis from masking the other. The sequential passes keep each axis tied to its own sources, while the summary reports the worst issue within each axis without collapsing them into one ranking.

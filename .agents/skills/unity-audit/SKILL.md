---
name: unity-audit
description: Audit a Unity project’s C# code for refactoring, simplification, correctness, performance, security, standards, architecture, and testing issues.
disable-model-invocation: true
---

Run an audit of one Unity project.

## 1. Establish scope

- Require the input directory. If missing, STOP and ask for it.

## 2. Read governing sources

From the current working directory, find these governing-source categories and read them completely:

- the root `CONTEXT.md`.
- ADR files,  applicable standards under `docs/`.

Resolve conflicts in this order: accepted/current ADRs, context/domain invariants, project standards, then general best practices.

## 3. Generate the Repomix file

Run Repomix with the supplied input directory and exact output file:

```bash
npx --yes repomix "<input-directory>" \
  --include "**/*.cs" \
  --output "<current-working-directory>/.scratch/repomix-output.xml" \
  --remove-comments \
  --remove-empty-lines
```

## 4. Audit

Read the Repomix generated file, audit for:

- correctness and domain-invariant violations;
- security vulnerabilities and unsafe trust/data boundaries;
- concrete performance or allocation risks;
- unnecessary complexity, duplication, coupling, indirection, dead paths, refactoring, and simplification opportunities;
- context, ADR, architecture, and standards violations;
- missing or insufficient automated validation for caller-visible behavior.

Report concrete issues or demonstrable risks only; include evidence, impact, and a specific action. Label uncertainty and omit speculation and subjective style preferences.

Severity: **5** correctness/security/invariant failure; **4** high-risk bug, performance issue, or hard standards/architecture violation; **3** meaningful maintainability, testing, or design problem; **2** minor drift or removable indirection; **1** low-impact style issue; **0** omit.

Use applicable tags in one combined block, such as `[Bug + Security]`, from `Bug`, `Security`, `Performance`, `Standards`, `Architecture`, `Refactor`, `Simplification`, and `Testing`. Merge duplicate issues and sort by severity descending.

## 5. Report

Start with input/output paths, governing sources, and limitations. Then use:

```markdown
1. **[5] [Bug + Security] <finding title>**

- `<project-relative/path.cs:line-range>`
- Evidence: <specific evidence>.
- Rule or contract: <source, when applicable>.
- Risk: <caller-visible impact>.
- Action: <smallest concrete fix or validation>.
```

Report `No findings.` when the complete audit is clean.

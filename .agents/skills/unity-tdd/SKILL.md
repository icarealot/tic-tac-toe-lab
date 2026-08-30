---
name: unity-tdd
description: Test-driven development. Use when the user wants to build features or fix bugs test-first, mentions "red-green-refactor", or wants integration tests.
---

# Unity Test-Driven Development

TDD is the red → green loop. This skill is the reference that makes that loop produce tests worth keeping: what a good test is, where tests go, the anti-patterns, and the rules of the loop. Every section applies on every cycle — consult them before and during the loop, not after.

When exploring the codebase, read `CONTEXT.md` (if it exists) so test names and interface vocabulary match the project's domain language, and respect ADRs in the area you're touching.

## What a good test is

Tests verify behavior through public interfaces, not implementation details. Code can change entirely; tests shouldn't. A good test reads like a specification — "player cannot spend more mana than is available" tells you exactly what capability exists — and survives refactors because it doesn't care about internal structure.

See [tests.md](tests.md) for EditMode and PlayMode examples. Read [mocking.md](mocking.md) before the code under test depends on an engine static (`Time`, `Input`, `PlayerPrefs`, `SceneManager`), a backend client, or file IO — it covers where the seam goes and how to fake it.

## Seams — where tests go

A **seam** is the public boundary you test at: the interface where you observe behavior without reaching inside. Tests live at seams, never against internals.

### Choose the Unity seam

- Use EditMode tests for deterministic rules and code that does not require a running scene.
- Use PlayMode tests only when behavior depends on Unity lifecycle, components, scenes, physics, or other engine integration.
- Do not use a slow PlayMode seam when an EditMode seam fully verifies the same behavior.
- Treat TDD as optional for visual polish, animation feel, audio, controls, camera feel, and level design. Require explicit human playtest criteria; do not present an automated test as evidence that the experience feels right.
- Do not create or modify scenes, prefabs, `.asmdef` files, or other non-code assets merely to make a test convenient. If one is necessary, preview the proposed change and require task-specific approval naming the exact file or narrow file group before editing.

## Anti-patterns

- **Implementation-coupled** — mocks internal collaborators, tests private methods, or verifies through a side channel (querying the database instead of using the interface). The tell: the test breaks when you refactor but behavior hasn't changed.
- **Tautological** — the assertion recomputes the expected value the way the code does (`expect(add(a, b)).toBe(a + b)`, a snapshot derived by hand the same way, a constant asserted equal to itself), so it passes by construction and can never disagree with the code. Expected values must come from an independent source of truth — a known-good literal, a worked example, the spec.
- **Horizontal slicing** — writing all tests first, then all implementation. Bulk tests verify _imagined_ behavior: you test the _shape_ of things rather than user-facing behavior, the tests go insensitive to real changes, and you commit to test structure before understanding the implementation. Work in **vertical slices** instead — one test → one implementation → repeat, each test a **tracer bullet** that responds to what the last cycle taught you.

## Rules of the loop

- **Red before green.** Write the failing test first, then only enough code to pass it. Don't anticipate future tests or add speculative features.
- **One slice at a time.** One seam, one test, one minimal implementation per cycle.
- **Refactoring is not part of the loop.** It belongs to the review stage (see the `/code-review` skill), not the red → green implementation cycle.

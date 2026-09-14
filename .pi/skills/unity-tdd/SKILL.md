---
name: unity-tdd
description: Test-driven development. Use when the user wants to build features or fix bugs test-first, mentions "red-green-refactor", or wants integration tests.
---

# Unity Test-Driven Development

TDD is the red → green loop. This skill is the reference that makes that loop produce tests worth keeping: what a good test is, where tests go, the anti-patterns, and the rules of the loop. Every section applies on every cycle — consult them before and during the loop, not after.

When exploring the codebase, read `CONTEXT.md` (if it exists) so test names and interface vocabulary match the project's domain language, and respect ADRs in the area you're touching.

## Risk gate

A test earns its place by protecting a named game rule, calculation, meaningful branch, state change, lifecycle, infrastructure contract, or critical player journey. Test behavior according to failure risk, not a target count or coverage percentage. A past bug has no special status: retain its regression test only while the behavior independently clears this gate.

Prioritize deterministic business logic, then infrastructure whose failure could break the app or lose player progress, then critical external-service contracts. Test glue only when it owns meaningful branching, coordination, state, or lifecycle behavior. Trivial forwarding and construction need no dedicated tests.

Treat TDD as optional for visual polish, animation feel, audio, controls, camera feel, usability, and level design. Define human playtest criteria for those outcomes; an automated test cannot establish that an experience feels right.

## What a good test is

Tests verify behavior through public interfaces, not implementation details. Code can change entirely; tests shouldn't. A good test reads like a specification — "player cannot spend more mana than is available" tells you exactly what capability exists — and survives refactors because it doesn't care about internal structure.

Expected values come from an independent source of truth such as a rule, worked example, or known-good literal. Assert each rule primarily at its owning layer rather than repeating the same outcome through every architectural layer. Parameterize equivalent cases.

See [tests.md](tests.md) for EditMode and PlayMode examples. Read [mocking.md](mocking.md) before the code under test depends on an engine static (`Time`, `Input`, `PlayerPrefs`, `SceneManager`), a backend client, or file IO — it covers where the seam goes and how to fake it.

## Smallest sufficient fixture

A **seam** is the public boundary where a caller observes behavior. Choose the cheapest fixture that can prove the behavior:

1. A plain EditMode test for deterministic behavior. Create no `GameObject`s and depend on no scene, prefab, frame, coroutine timing, or Unity lifecycle. Unity value types are fine.
2. An isolated PlayMode `GameObject` for focused component, lifecycle, physics, or other engine integration.
3. A production prefab only when its serialized configuration is part of the behavior.
4. A production scene only when bootstrap or cross-object production wiring is part of the behavior.

Prefer an existing public seam. Do not load a production scene merely to obtain a component, and extend an existing scene-level critical journey when that remains readable. Create only the objects a focused PlayMode test needs and clean them up.

Creating or modifying scenes, prefabs, `.asmdef` files, or other non-code assets merely to make a test convenient requires task-specific approval naming the exact file or narrow file group before editing.

## Assertion boundaries

Assert public, observable behavior. Exact ordering belongs in a test only when ordering is itself the contract. Exclude incidental collaborator call counts, private sequencing, exact diagnostic text, labels, colors, styling, layout, and hierarchy or object presence unless that structure is the agreed contract. One lifecycle round trip is enough when it establishes the invariant.

## Anti-patterns

- **Implementation-coupled** — mocks internal collaborators, tests private methods, or verifies through a side channel (querying the database instead of using the interface). The tell: the test breaks when you refactor but behavior hasn't changed.
- **Tautological** — the assertion recomputes the expected value the way the code does (`expect(add(a, b)).toBe(a + b)`, a snapshot derived by hand the same way, a constant asserted equal to itself), so it passes by construction and can never disagree with the code. Expected values must come from an independent source of truth — a known-good literal, a worked example, the spec.
- **Horizontal slicing** — writing all tests first, then all implementation. Bulk tests verify _imagined_ behavior: you test the _shape_ of things rather than user-facing behavior, the tests go insensitive to real changes, and you commit to test structure before understanding the implementation. Work in **vertical slices** instead — one test → one implementation → repeat, each test a **tracer bullet** that responds to what the last cycle taught you.

## Rules of the loop

- **Red before green.** Write the failing test first, then only enough code to pass it. Don't anticipate future tests or add speculative features.
- **One slice at a time.** One seam, one test, one minimal implementation per cycle.
- **Refactoring is not part of the loop.** It belongs to the review stage (see the `/code-review` skill), not the red → green implementation cycle.

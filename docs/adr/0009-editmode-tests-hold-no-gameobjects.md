# EditMode tests hold no GameObjects; Unity objects use PlayMode scene tests

## Status

Superseded by [ADR 0012](./0012-unity-tests-use-the-smallest-sufficient-fixture.md).

## Context and Problem Statement

Some EditMode tests had begun constructing Unity objects and using reflection to imitate serialized prefab wiring. Other tests passed absent collaborators because satisfying their Unity dependencies was expensive. What may an EditMode test require from the types it exercises?

## Decision Drivers

- Reflection over private serialized state couples tests to details the compiler cannot check.
- Bare Unity components do not reproduce prefab wiring.
- One concrete Unity dependency can pull a large object graph into a small test.
- Fast tests should reason about plain behavior and explicit seams.
- Scene-dependent behavior should be tested against the scene and assets that ship.
- The boundary should make the problematic testing pattern difficult to reintroduce.

## Considered Options

- EditMode tests hold no GameObjects; Unity-dependent behavior uses PlayMode scene tests
- Allow GameObjects in EditMode but forbid dependencies on prefab wiring
- Instantiate real prefabs in EditMode tests

## Decision Outcome

Chosen option: “EditMode tests hold no GameObjects; Unity-dependent behavior uses PlayMode scene tests,” because it structurally removes the need to reconstruct serialized wiring through reflection.

EditMode tests construct plain objects only. They do not create components or GameObjects. A subject that fundamentally requires a Unity object is exercised in PlayMode against the real scene.

PlayMode scene tests share a fixture that loads the app consistently and obtains fully wired objects from production assets. The scene is the fixture rather than something each test recreates by hand.

Several architectural consequences follow:

- Window-facing APIs use interfaces so user-interface stack behavior can be tested with plain substitutes.
- Board geometry owns derived cell placements, avoiding factory dependencies in presentation-rule tests.
- Delayed state behavior uses an injectable scheduling seam so tests can observe duration and trigger completion directly.
- States depend on narrow session and state-machine roles rather than a concrete object graph.
- Subscription and lifecycle integration tests remain deliberate PlayMode or graph-level tests rather than accidental unit-test setup.

### Positive Consequences

- Reflection-based prefab imitation has no reason to return.
- EditMode setups contain only collaborators relevant to the tested behavior.
- Timing behavior is testable without real delays.
- PlayMode component tests use actual prefab wiring.
- The distinction between plain logic and Unity integration is explicit.

### Negative Consequences

- PlayMode scene tests are slower than EditMode tests.
- Scene-loading tests are more distant from a small component under test.
- The rule remains a team convention rather than a compiler restriction.
- More interfaces and seams exist than the current app size alone would require.
- Editor-specific scene test infrastructure may not run unchanged on devices.

## Pros and Cons of the Options

### EditMode tests hold no GameObjects

- Good, because serialized wiring cannot be faked through reflection.
- Good, because plain behavior stays fast and focused.
- Good, because Unity components are tested with real assets.
- Bad, because the PlayMode tier is slower and broader.
- Bad, because additional interfaces are needed.

### Allow GameObjects but no prefab-wiring dependencies

- Good, because more component behavior remains fast to test.
- Good, because the immediate migration is smaller.
- Bad, because the rule is easy to violate when a serialized dependency is added.
- Bad, because large setup graphs remain attractive.

### Instantiate real prefabs in EditMode

- Good, because serialized wiring is authentic.
- Good, because fewer interfaces are necessary.
- Bad, because unit-level tests become coupled to assets.
- Bad, because tests become editor-specific and slower.
- Bad, because object-graph complexity remains.

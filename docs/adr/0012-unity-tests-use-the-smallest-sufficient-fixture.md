# Unity-dependent tests use the smallest sufficient fixture

## Status

Accepted

## Context and Problem Statement

ADR 0009 kept `GameObject`s out of EditMode tests and required Unity-dependent behavior to run in PlayMode against the production scene. That removed reflection-based prefab imitation, but made the scene the default fixture even when a test needed only one component or a small object graph. How can tests preserve a clear Unity boundary without paying for scene loading when scene wiring is irrelevant?

## Decision Drivers

- Business behavior should remain fast and independent of scenes, assets, frames, and Unity object lifecycles.
- A test should load no more runtime structure than the behavior requires.
- Production prefabs and scenes remain necessary when serialized configuration or aggregate wiring is the behavior under test.
- Constructing a small PlayMode object graph is cheaper and more focused than loading the application scene.
- EditMode tests must not return to reflection over private serialized state or incomplete component wiring.
- Critical player journeys still need protection against failures in bootstrap and production-scene wiring.

## Considered Options

- Keep `GameObject`s out of EditMode tests, use isolated PlayMode objects or prefabs by default, and reserve the production scene for critical wiring journeys
- Continue requiring every Unity-dependent test to use the production scene under ADR 0009
- Permit isolated `GameObject`s in EditMode tests

## Decision Outcome

Chosen option: “keep `GameObject`s out of EditMode tests, use isolated PlayMode objects or prefabs by default, and reserve the production scene for critical wiring journeys,” because it preserves a strict unit-test boundary while making Unity-dependent tests use the smallest fixture that can prove their behavior.

EditMode tests construct plain objects only. They may use Unity value types, but they do not create `GameObject`s or depend on scenes, prefabs, frames, coroutine timing, or Unity lifecycle callbacks.

A focused Unity-dependent test runs in PlayMode. It creates an isolated `GameObject` when component or lifecycle semantics are sufficient, and uses a production prefab when serialized prefab configuration is part of the behavior. It does not load the production scene merely to locate a component.

A PlayMode test loads the production scene only when bootstrap, input routing, or coordination across the shipped scene object graph is the behavior being protected. The current scene-level behavioral inventory and the test-selection rules live in [`TESTING.md`](../../TESTING.md).

### Positive Consequences

- Pure behavior remains isolated from Unity object lifecycles.
- Most component tests avoid production-scene startup and teardown costs.
- Focused failures identify the component contract more directly.
- Production prefab configuration can still be tested when it matters.
- A small scene smoke tier continues to protect critical shipped wiring.

### Negative Consequences

- Isolated object construction can diverge from production prefab or scene configuration.
- Contributors must decide whether serialized configuration is relevant to each test.
- Some Unity behavior remains in PlayMode even when it does not require a full scene.
- The suite has three fixture tiers instead of the simpler unit-versus-scene distinction from ADR 0009.

## Pros and Cons of the Options

### Isolated PlayMode fixtures by default, with scene smoke tests

- Good, because each test uses only the Unity runtime structure its behavior requires.
- Good, because EditMode tests remain free of `GameObject`s.
- Good, because prefabs and scenes are still exercised where their configuration is the contract.
- Good, because critical player journeys retain production-scene coverage.
- Bad, because isolated fixtures may omit a production wiring defect.
- Bad, because choosing the correct fixture requires judgment.

### Require the production scene for every Unity-dependent test

- Good, because every Unity-dependent test uses shipped scene and prefab wiring.
- Good, because contributors have one PlayMode fixture strategy.
- Bad, because small component tests pay for unrelated scene startup and object graphs.
- Bad, because failures are broader and can be caused by unrelated scene changes.
- Bad, because the strategy encourages granular scene-wiring assertions to justify the expensive fixture.

### Permit isolated GameObjects in EditMode tests

- Good, because component tests can avoid PlayMode and scene startup.
- Good, because small fixtures remain easy to construct.
- Bad, because EditMode tests can drift back toward reflection-based serialized wiring.
- Bad, because EditMode does not reproduce PlayMode lifecycle and frame behavior.
- Bad, because the boundary between plain behavior and Unity components becomes less explicit.

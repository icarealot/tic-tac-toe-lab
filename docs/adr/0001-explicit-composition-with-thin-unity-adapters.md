# Use explicit composition with a plain C# core and thin Unity adapters

## Status

Accepted

## Context and Problem Statement

Tic Tac Toe Lab exists to explore Unity testing and an architecture friendlier to tests than global singletons. It may also yield code worth reusing, but designing generic frameworks before a second use case has made the small game harder to follow. How should runtime dependencies be structured without coupling game behavior to Unity scenes or speculative abstractions?

## Decision Drivers

- Deterministic rules and application flow should run in fast EditMode tests.
- Unity lifecycle, input, timing, camera, rendering, and serialized wiring need focused PlayMode validation.
- Dependencies must be visible rather than resolved through global state.
- Production abstractions must serve current behavior, not hypothetical reuse.
- The execution path should remain understandable in a small learning project.
- Reusable modules should be extracted only after a concrete second consumer reveals their real contract.

## Considered Options

- Explicit composition, plain C# core, and narrow Unity adapters
- MonoBehaviour singletons or a service locator
- A generalized framework of factories, state machines, window stacks, and interfaces for every role

## Decision Outcome

Chosen option: "explicit composition, plain C# core, and narrow Unity adapters," because it provides deterministic seams at actual system boundaries without turning every owned collaborator into an abstraction.

`Bootstrap` is the composition root. It receives long-lived Unity adapters through explicit scene references, constructs plain C# objects, and connects their lifetimes. Domain rules and application flow remain plain C#. MonoBehaviours adapt Unity input, camera conversion, rendering, timing, user interface, lifecycle, and serialized assets.

Interfaces are retained only at current Unity or nondeterministic boundaries, where multiple current implementations require a shared role, or where a heterogeneous mechanism genuinely needs one. Owned application classes otherwise use concrete collaborators and are tested together. Values cross boundaries before new interfaces are introduced.

Runtime Unity objects are created by the adapter that owns the concrete prefab dependency. There is no global singleton, service locator, generic prefab registry, or interface-based object factory. Application flow and user-interface operations express only the phases and windows the current game uses rather than generalized state and navigation frameworks.

The project keeps one runtime assembly and its existing namespace and feature-oriented folders. Potentially reusable code remains cohesive inside the project until another project provides a concrete extraction requirement.

Tests use the cheapest sufficient fixture: deterministic behavior in EditMode, focused Unity behavior and serialized wiring in PlayMode, and a short critical journey through the production scene. Tests may substitute system boundaries but do not require production interfaces solely to fake owned code.

### Positive Consequences

- Game rules and application flow can be tested without scenes or real elapsed time.
- Runtime dependencies and lifetime ownership are visible in one composition root.
- Unity-specific tests focus on actual engine and wiring risks.
- Fewer interfaces, forwarding layers, registries, and generic mechanisms obscure the game.
- A future extraction will be based on evidence from a real consumer.

### Negative Consequences

- `Bootstrap` must explicitly wire several collaborators.
- Some boundaries still require interfaces and hand-written fakes.
- Concrete prefab references can require scene or prefab updates when assets move.
- Code cannot be consumed as a ready-made package until a real extraction is performed.
- Adding genuinely more complex navigation later may justify introducing a stronger flow abstraction.

## Pros and Cons of the Options

### Explicit composition, plain C# core, and narrow Unity adapters

- Good, because dependencies and ownership are explicit.
- Good, because deterministic code uses the fastest test fixture.
- Good, because interfaces correspond to real system boundaries.
- Good, because the architecture can grow from observed needs.
- Bad, because composition and teardown require deliberate wiring.
- Bad, because some Unity adapters still need PlayMode coverage.

### MonoBehaviour singletons or a service locator

- Good, because any component can obtain shared dependencies with little initial wiring.
- Good, because the pattern is familiar in many small Unity projects.
- Bad, because dependencies are hidden and global state leaks between tests.
- Bad, because initialization order and teardown become implicit.
- Bad, because deterministic behavior becomes harder to construct in isolation.

### A generalized framework of factories, state machines, window stacks, and interfaces for every role

- Good, because future projects might reuse some mechanisms unchanged.
- Good, because nearly every collaborator can be replaced by a test double.
- Bad, because hypothetical requirements determine current production structure.
- Bad, because framework behavior needs tests unrelated to the game's current risks.
- Bad, because the execution path crosses many small abstractions and forwarding layers.
- Bad, because the actual reusable contract remains unproven until another project uses it.

# Use explicit composition with a plain C# core and thin Unity adapters

## Status

Accepted

## Context and Problem Statement

Tic Tac Toe Lab exists to explore Unity testing and an architecture friendlier to tests than global singletons. It may also yield code worth reusing, but designing generic frameworks before a second use case has made the small game harder to follow. How should runtime dependencies be structured without coupling game behavior to Unity scenes or speculative abstractions?

## Decision Drivers

- Deterministic rules, application flow, and exact delays should run in fast EditMode tests through an injected or fake scheduling boundary.
- Unity lifecycle, input, and other non-presentation engine behavior need focused PlayMode validation in isolated fixtures.
- Production prefab and scene wiring, presentation, and camera behavior need human validation rather than permanent automation.
- Dependencies must be visible rather than resolved through global state.
- The production scene should remain a minimal entry point rather than a second composition definition.
- Ownership and teardown of long-lived runtime objects should be explicit.
- Production abstractions must serve current behavior, not hypothetical reuse.
- The execution path should remain understandable in a small learning project.
- Reusable modules should be extracted only after a concrete second consumer reveals their real contract.

## Considered Options

- Explicit composition, plain C# core, and narrow Unity adapters
- MonoBehaviour singletons or a service locator
- A generalized framework of factories, state machines, window stacks, and interfaces for every role

## Decision Outcome

Chosen option: "explicit composition, plain C# core, and narrow Unity adapters," because it provides deterministic seams at actual system boundaries without turning every owned collaborator into an abstraction.

`Bootstrap` is the composition root. `Main.unity` contains exactly one root: an instance of the Bootstrap prefab with no scene overrides. The Bootstrap prefab directly references the camera, board view, application UI, input service, and delay scheduler prefabs.

In `Awake`, Bootstrap validates every prefab reference before creating anything, instantiates those five long-lived adapters as separately named scene roots, constructs the plain C# objects, and connects their lifetimes. Bootstrap is scene-scoped. It owns the instantiated roots and explicitly tears them down after disposing the application flow and board presenter. Domain rules and application flow remain plain C#. MonoBehaviours adapt Unity input, camera conversion, rendering, timing, user interface, lifecycle, and serialized assets.

Interfaces are retained only at current Unity or nondeterministic boundaries, where multiple current implementations require a shared role, or where a heterogeneous mechanism genuinely needs one. Owned application classes otherwise use concrete collaborators and are tested together. Values cross boundaries before new interfaces are introduced.

Bootstrap creates only the long-lived root adapters. Runtime Unity objects below those roots are created by the adapter that owns the concrete prefab dependency: BoardView creates CellView instances, each CellView creates and owns its MarkView, and ApplicationUI creates panels and popups. There is no global singleton, service locator, generic prefab registry, or interface-based object factory. Application flow and user-interface operations express only the phases and windows the current game uses rather than generalized state and navigation frameworks.

The project keeps one runtime assembly and its existing namespace and feature-oriented folders. Potentially reusable code remains cohesive inside the project until another project provides a concrete extraction requirement.

Tests use the cheapest sufficient fixture: deterministic rules and application flow in plain EditMode, and focused component, lifecycle, input, and uGUI behavior in isolated PlayMode fixtures built from generated objects. Permanent automation stops at non-production seams; production prefab and scene wiring, including the authored-scene invariant and Bootstrap's ownership of separately rooted adapters in the shipped scene, is assigned to a user-confirmed Human Playtest. Tests may substitute system boundaries but do not require production interfaces solely to fake owned code.

### Positive Consequences

- Game rules and application flow can be tested without scenes or real elapsed time.
- Runtime dependencies and lifetime ownership are visible in one composition root.
- The production scene remains a replaceable, minimal entry point with no adapter-specific wiring.
- Unity-specific tests focus on actual engine and lifecycle risks in isolated fixtures.
- Automation does not freeze authored prefab structure, so the production scene and prefabs can change without rewriting tests.
- Fewer interfaces, forwarding layers, registries, and generic mechanisms obscure the game.
- A future extraction will be based on evidence from a real consumer.

### Negative Consequences

- `Bootstrap` must explicitly instantiate, wire, name, and tear down several collaborators.
- Some boundaries still require interfaces and hand-written fakes.
- Concrete prefab references require Bootstrap prefab updates when assets move.
- Runtime inspection shows several Bootstrap-owned scene roots even though the authored scene has only one.
- Production prefab and scene wiring has no permanent automated regression guard; a broken shipped composition is caught only by a human playtest.
- Code cannot be consumed as a ready-made package until a real extraction is performed.
- Adding genuinely more complex navigation later may justify introducing a stronger flow abstraction.

## Pros and Cons of the Options

### Explicit composition, plain C# core, and narrow Unity adapters

- Good, because dependencies and ownership are explicit.
- Good, because deterministic code uses the fastest test fixture.
- Good, because interfaces correspond to real system boundaries.
- Good, because the architecture can grow from observed needs.
- Bad, because composition, validation, and teardown require deliberate wiring.
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

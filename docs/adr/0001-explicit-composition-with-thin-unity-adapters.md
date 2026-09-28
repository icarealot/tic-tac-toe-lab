# Use explicit composition with a plain C# core and thin Unity adapters

## Status

Accepted

## Context and Problem Statement

Tic Tac Toe Lab exists to explore Unity testing and an architecture friendlier to tests than global singletons. It may also yield code worth reusing, but designing generic frameworks before a second use case has made the small game harder to follow. How should runtime dependencies be structured without coupling game behavior to Unity scenes or speculative abstractions?

## Decision Drivers

- Deterministic rules, application flow, and exact delays should run in fast EditMode tests through an injected or fake scheduling boundary.
- Unity lifecycle, input, and other non-presentation engine behavior need focused PlayMode validation in isolated fixtures.
- Distinct production-prefab serialized wiring risks and one critical player journey need sparse permanent PlayMode automation.
- Presentation, camera feel, and other qualities requiring human judgment need human validation rather than permanent automation.
- Dependencies must be visible rather than resolved through global state.
- The production scene should remain a minimal entry point rather than a second composition definition.
- Ownership and teardown of long-lived runtime objects should be explicit.
- Production abstractions must serve current behavior, not hypothetical reuse.
- The execution path should remain understandable in a small learning project.
- Reusable modules should be extracted only after a concrete second consumer reveals their real contract.

## Considered Options

- Explicit composition, a plain C# core, narrow Unity adapters, and a constrained type-keyed screen registry
- MonoBehaviour singletons or a service locator
- A generalized framework of factories, state machines, window stacks, and interfaces for every role

## Decision Outcome

Chosen option: "explicit composition, a plain C# core, narrow Unity adapters, and a constrained type-keyed screen registry," because it provides deterministic seams at actual system boundaries while removing repeated screen lifecycle code without introducing a general navigation framework.

`Bootstrap` is the composition root. `Main.unity` contains exactly one root: an instance of the Bootstrap prefab with no scene overrides. The Bootstrap prefab directly references the camera, board view, application UI, and delay scheduler prefabs.

In `Awake`, Bootstrap validates those four prefab references before creating anything, instantiates them as separately named scene roots, constructs the plain C# objects including the input service, and connects their lifetimes. Bootstrap is scene-scoped. It owns the instantiated roots and explicitly tears them down after disposing the app state machine, board presenter, and input service. Domain rules and application flow remain plain C#. The input service is a directly constructed, Unity-specific C# adapter that owns and disposes its generated Input System actions; it has no scene root and does not rely on Unity destruction for cleanup. MonoBehaviours adapt camera conversion, rendering, timing, user interface, lifecycle, and serialized assets on the four remaining prefab-backed scene roots.

Interfaces are retained only at current Unity or nondeterministic boundaries, where multiple current implementations require a shared role, or where a heterogeneous mechanism genuinely needs one. User-interface screens are such a heterogeneous mechanism: each screen component implements exactly one role interface derived from `IScreen`, allowing application flow to configure screens without depending on concrete Unity components. Owned application classes otherwise use concrete collaborators and are tested together. Values cross boundaries before new interfaces are introduced.

Bootstrap creates only the long-lived root adapters. Runtime Unity objects below those roots are created by the adapter that owns the concrete prefab dependency: BoardView creates CellView instances, each CellView creates and owns its MarkView, and AppUI creates screens.

AppUI owns a serialized list of screen registrations. Each registration pairs a screen component prefab with either the `Base` or `Popup` layer. On initialization, AppUI validates that every prefab implements exactly one role interface derived from `IScreen`, rejects missing or duplicate role registrations, and builds a registry keyed by that role-interface type. `IAppUI` exposes generic `Show<TScreen>(Action<TScreen> configure = null)` and `Close<TScreen>()` operations rather than one operation per concrete screen. Requests for unregistered roles and invalid layer transitions fail immediately.

Each layer has at most one active screen. Showing a screen replaces the active screen on the same layer, deactivating the outgoing instance immediately before scheduling its destruction. Showing a popup requires an active base. A base cannot be shown, replaced, or closed while a popup is active; application flow must explicitly close its popup first. Closing a registered screen that is not active is an idempotent no-op and never closes a replacement of another role.

This registry is limited to AppUI-owned screen prefab selection and two-layer lifecycle enforcement. It is not a global prefab registry, service locator, navigation history, or window stack. Application flow continues to define transitions explicitly.

The project keeps one runtime assembly and its existing namespace and feature-oriented folders. Potentially reusable code remains cohesive inside the project until another project provides a concrete extraction requirement.

Tests use the cheapest sufficient fixture: deterministic rules and application flow in plain EditMode, and focused component, lifecycle, input, and uGUI behavior in isolated PlayMode fixtures built from generated objects. Generated fixtures remain the preferred seam for detailed component and lifecycle behavior. Sparse production-asset automation adds only what generated fixtures cannot prove: focused PlayMode checks for distinct serialized wiring risks in production prefabs, and one sparse PlayMode journey through the authored `Main` scene covering the critical player route from Home through an X win and back Home. These checks observe caller-visible behavior with bounded timeouts rather than asserting private fields, exact hierarchy, serialized values, or authored presentation. The journey exercises the authored-scene invariant and Bootstrap's ownership of separately rooted adapters in the shipped scene. Tests may substitute system boundaries but do not require production interfaces solely to fake owned code. Styling, animation appearance, cadence, camera feel, safe-area appearance, and other presentation judgments remain assigned to human playtesting.

### Positive Consequences

- Game rules and application flow can be tested without scenes or real elapsed time.
- Runtime dependencies and lifetime ownership are visible in one composition root.
- The production scene remains a replaceable, minimal entry point with no adapter-specific wiring.
- Unity-specific tests focus on actual engine and lifecycle risks in isolated fixtures.
- Automation does not freeze authored prefab structure, so the production scene and prefabs can change without rewriting tests.
- A broken shipped-prefab wiring reference or a broken critical journey is caught by sparse permanent automation instead of only by a human playtest.
- A single screen lifecycle path removes repeated instantiate, replace, close, and layer-selection code.
- New screens can be added through a role interface, component prefab, and AppUI registration without adding purpose-specific methods to AppUI.
- The constrained registry does not introduce global lookup, navigation history, or implicit application transitions.
- A future extraction will be based on evidence from a real consumer.

### Negative Consequences

- `Bootstrap` must explicitly instantiate, wire, name, and tear down several collaborators.
- Some boundaries and screen roles require interfaces and hand-written fakes.
- The screen registry adds startup validation and type-based lookup that direct serialized fields did not require.
- Application flow depends on screen role interfaces, although it remains independent of concrete Unity components.
- Concrete prefab references require Bootstrap prefab updates when assets move.
- Runtime inspection shows several Bootstrap-owned scene roots even though the authored scene has only one.
- Sparse production-asset checks add maintenance when authored prefab wiring or the critical journey changes; the rest of production composition and presentation still relies on human playtesting.
- Code cannot be consumed as a ready-made package until a real extraction is performed.
- Adding genuinely more complex navigation later may justify introducing a stronger flow abstraction.

## Pros and Cons of the Options

### Explicit composition, plain C# core, narrow Unity adapters, and a constrained screen registry

- Good, because dependencies and ownership remain explicit.
- Good, because deterministic code uses the fastest test fixture.
- Good, because interfaces correspond to the Unity boundary and heterogeneous screen roles.
- Good, because adding a screen does not expand AppUI with another purpose-specific operation.
- Good, because the registry has explicit layer and lifecycle invariants rather than implicit navigation behavior.
- Bad, because composition, registry validation, and teardown require deliberate wiring.
- Bad, because role-interface lookup and generic test doubles are more complex than direct prefab fields.
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

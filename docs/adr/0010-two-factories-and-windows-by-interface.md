# Creation belongs to two factories, and windows are requested by interface

## Status

Superseded by [ADR 0011](./0011-one-interface-based-factory-for-runtime-unity-objects.md).

## Context and Problem Statement

ADR 0009 requires EditMode tests to hold no GameObjects. User-interface behavior therefore needs plain substitute windows, while production windows remain Unity components created from prefabs. At the same time, boot-time scene objects and repeatedly created windows appeared to have different creation needs. How should callers name windows, and who should create and configure them?

## Decision Drivers

- Windows seen by EditMode tests must be representable as plain objects.
- Unity can discover components through interfaces even though interfaces cannot themselves be instantiated.
- The prefab registry should remain the source of available implementations.
- Panel and popup roles must remain distinct.
- User-interface layer selection and safe-area initialization are specific to window creation.
- General scene objects and windows appeared to form separate creation populations.

## Considered Options

- Two factories, with windows requested by interface
- One interface-based factory that also understands window layers
- Resolve window roles from concrete naming conventions
- Identify windows with untyped identifiers
- Create every window at startup

## Decision Outcome

Chosen option: “two factories, with windows requested by interface,” because it preserved plain EditMode substitutes while keeping general component creation strongly tied to Unity components.

ComponentFactoryService served scene objects and spawned non-window views from the prefab registry. UIFactoryService adapted interface-based panel and popup requests, selected the correct hierarchy layer and initialized each window before returning it. UIService depended only on the window factory and remained focused on stack behavior.

Window roles formed an interface hierarchy distinguishing all windows, panels, popups and specific window capabilities. This kept panel and popup misuse visible at compile time and let tests supply plain substitutes.

Lifecycle operations retained get-and-return language to leave room for future pooling.

### Positive Consequences

- User-interface stack tests can use plain windows.
- Panel and popup roles remain distinct.
- The prefab registry remains the only production registration source.
- UIService remains independent of Unity hierarchy and initialization details.
- General component consumers retain stronger type constraints.

### Negative Consequences

- Two factories wrap one underlying prefab registry and creation mechanism.
- Contributors must decide which factory owns each new runtime object.
- Returning interface-based windows still requires a runtime transition back to a Unity object.
- Every window role requires an interface.
- UIFactoryService and UIService are easy to confuse by name.

## Pros and Cons of the Options

### Two factories with interface-based windows

- Good, because EditMode windows remain plain objects.
- Good, because UIService stays focused on stack rules.
- Good, because general component creation retains stronger constraints.
- Bad, because creation has two abstractions and two vocabularies.
- Bad, because ownership depends on classifying each created object.

### One interface-based factory aware of window layers

- Good, because all creation has one home.
- Good, because only one factory contract is needed.
- Bad, because the general factory gains user-interface policy.
- Bad, because interface-only restrictions require runtime enforcement.

### Convention-based role resolution

- Good, because explicit registration is unnecessary.
- Bad, because naming changes can silently break resolution.
- Bad, because failures move from compile time to runtime convention.

### Untyped window identifiers

- Good, because per-window interfaces are unnecessary.
- Bad, because typed configuration is lost.
- Bad, because identifiers and prefab registration can disagree.

### Create every window at startup

- Good, because runtime window creation disappears.
- Bad, because all windows remain resident whether used or not.
- Bad, because it contradicts the chosen window lifecycle.

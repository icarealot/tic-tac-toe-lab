# Runtime Unity objects come from one interface-based factory

## Status

Accepted

## Context and Problem Statement

ADR 0010 split creation between ComponentFactoryService and UIFactoryService. That separation kept user-interface setup away from the general component factory, but it left two factory abstractions around one prefab registry and divided the lifecycle of runtime Unity objects according to whether an object was a window.

The project now wants one place for instantiation and destruction. How can one factory serve scene objects, spawned views and windows without also becoming responsible for configuring them, while preserving GameObject-free EditMode tests and the distinction between panels and popups?

## Decision Drivers

- Instantiation and destruction are one mechanism and should have one implementation and one interface.
- A factory should produce and reclaim instances; it should not decide how consumers configure them.
- Consumers and tests should depend on roles rather than concrete Unity components.
- Panel and popup requests must remain distinct so they cannot enter the wrong stack.
- The prefab registry should remain the single source of available runtime objects.
- Registry order must not silently choose between multiple implementations of the same role.
- Lifecycle-neutral language should leave room for pooling later.
- The factory cannot instantiate itself, so startup requires one explicit exception to the single-instantiation-path rule.

## Considered Options

- One FactoryService and one IFactoryService, with configuration owned by consumers
- Keep the two factories selected by ADR 0010
- One concrete factory behind separate component and user-interface factory contracts
- One factory that also configures windows

## Decision Outcome

Chosen option: “one FactoryService and one IFactoryService, with configuration owned by consumers,” because it creates one lifecycle boundary for all runtime Unity objects without teaching the factory about board layout, user-interface layers, safe areas or coroutine behavior.

All factory requests name an interface describing the role the consumer needs. The factory resolves that role against the prefab registry and requires exactly one matching prefab. A missing registration and an ambiguous registration are both errors; registry order never chooses an implementation implicitly.

FactoryService is responsible only for instantiation, parenting supplied as part of instantiation, and returning an instance. Returning currently destroys the instance, but the chosen language does not prevent pooling later. The factory performs no positioning, naming, dependency injection or construction specific to the returned object.

UIService owns window-specific setup. It chooses the panel or popup layer, initializes the window, applies caller configuration and manages visibility and stacking. Its separate panel and popup operations preserve the structural distinction established by ADR 0008.

Every kind obtained from the factory exposes an interface suitable for its consumers. Concrete Unity details remain behind those interfaces, allowing EditMode tests to use plain substitutes under ADR 0009.

Bootstrap directly instantiates FactoryService because a factory cannot create itself. That is the only direct runtime instantiation outside the factory.

### Positive Consequences

- There is one factory implementation, one factory contract and one prefab registry to inspect when creation fails.
- Consumers request roles rather than concrete component implementations.
- FactoryService remains independent of user-interface and board policy.
- User-interface stack rules remain testable with plain objects in EditMode.
- Missing and duplicate registrations fail loudly instead of depending on prefab order.
- Returning objects can later use pooling without changing the architectural vocabulary.

### Negative Consequences

- The language cannot enforce “any interface, but not a concrete class” as a generic compile-time constraint, so the boundary requires runtime validation.
- Returning an interface requires verifying that its runtime implementation is a Unity object the factory can reclaim.
- UIService now owns engine-facing window setup and is less isolated from Unity than under ADR 0010.
- Every factory-created role needs an interface, including small view roles.
- Broad interfaces may match several prefabs, so consumers must request roles specific enough to identify one registration.
- Bootstrap retains one direct instantiation for the factory itself.

## Pros and Cons of the Options

### One FactoryService and one IFactoryService, with configuration owned by consumers

All runtime Unity objects are resolved by role from one registry. Consumers perform object-specific setup after creation.

- Good, because instantiation and destruction have one implementation and one abstraction.
- Good, because the factory contains no user-interface or board policy.
- Good, because test substitutes remain plain objects.
- Good, because ambiguous registrations are diagnosed rather than selected by registry order.
- Bad, because interface-only use is guarded at runtime rather than by the compiler.
- Bad, because UIService gains engine-facing collaborators and initialization responsibilities.
- Bad, because additional interfaces are required for every spawned role.

### Keep the two factories selected by ADR 0010

One factory creates general components, while another handles interface-based window requests and setup.

- Good, because component creation retains stronger compile-time constraints.
- Good, because UIService remains focused on stack behavior.
- Good, because window placement and construction stay behind a focused abstraction.
- Bad, because one registry and one underlying creation mechanism are presented as two factories.
- Bad, because contributors must decide which factory population owns each new runtime object.
- Bad, because returning a window still crosses from an interface to a Unity object at runtime.

### One concrete factory behind separate component and user-interface factory contracts

One implementation owns the registry but presents narrow contracts to different consumers.

- Good, because there is one implementation while consumers see only the capabilities they need.
- Good, because general component consumers retain stronger constraints.
- Bad, because there are still two factory contracts and two creation vocabularies.
- Bad, because the implementation still knows about user-interface setup.
- Bad, because it does not provide the chosen single factory contract.

### One factory that also configures windows

The single factory recognizes window roles, chooses their layers and supplies their initialization dependencies.

- Good, because consumers receive fully initialized windows.
- Good, because UIService remains independent of hierarchy details.
- Bad, because a general object factory gains user-interface-specific dependencies and policy.
- Bad, because unrelated runtime objects depend indirectly on user-interface infrastructure.
- Bad, because the factory does more than the chosen instantiate-and-return boundary.

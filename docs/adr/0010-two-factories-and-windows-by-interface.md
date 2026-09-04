# Creation belongs to two factories, and windows are asked for by interface

## Status

Accepted

## Context and Problem Statement

[ADR 0009](./0009-editmode-tests-hold-no-gameobjects.md) rules that an EditMode test holds no `GameObject`s. `UIService` creates every window it stacks, through a factory whose signature is `Get<T>() where T : Component` and an API whose signature is `ShowPanel<TPanel>() where TPanel : Panel`. Under the new rule those two constraints are unsatisfiable: a test that shows a panel would have to produce a real component, and producing one is precisely what it may no longer do.

The type parameter is doing two jobs at once. `ShowPanel<GameplayPanel>(panel => panel.Setup(session))` names *what the caller wants* and *what the factory must instantiate*, and those are now different things — the caller wants something it can call `Setup` on, while the factory needs a concrete `MonoBehaviour` to `Instantiate`. Something has to supply the second without the caller naming it.

The factory has a second problem, older than this one. `IFactoryService` serves two unrelated populations: scene furniture created exactly once by `Bootstrap` — `Camera`, `CoroutineService`, `BoardView`, `UIRoot` — and things spawned repeatedly during play — `MarkView`, and every window. One interface, one constraint and one test double have been covering both, which is why the double had to be clairvoyant.

How does a caller name the window it wants, and who builds it?

## Decision Drivers

- Under ADR 0009 a window an EditMode test sees must be a plain object, so whatever a caller names cannot be a `Component`.
- Unity's `GetComponent<T>()` and `TryGetComponent<T>(out T)` are unconstrained and accept interface type arguments. `prefab.TryGetComponent(out IGameplayPanel panel)` already finds the `GameplayPanel` component, which means the existing `[SerializeField] GameObject[] _prefabs` array **is** an interface registry, maintained in the Inspector, with no registration code anywhere.
- `Object.Instantiate<T>` is `where T : Object`, so an interface cannot be instantiated directly. A factory must hold the prefab as a `Component`, instantiate that, and cast on the way out.
- Creation belongs to a factory, not to the composition root. A `Register<IGameplayPanel, GameplayPanel>(prefab)` list in `Bootstrap` was considered and rejected: the composition root should construct services, not enumerate what each of them is able to make.
- [ADR 0008](./0008-panels-and-popups-are-two-stacks.md) states that the ordering invariant must be enforced "by the hierarchy and the type system rather than by the service's own correctness", and that pushing a popup onto the panel stack must be a compile error. Any replacement has to keep that guarantee, not merely re-document it.
- `Factory.prefab` already registers all eight prefabs, both windows among them, so nothing about the existing resolution mechanism needs to change to support this.
- The two populations differ in more than type: boot-time furniture is asked for once by name, while windows are asked for by role, parented to a layer, and handed a coroutine service on creation.

## Considered Options

- Two factories; windows asked for by interface
- One factory relaxed to `where T : class` and taught about layers
- Per-window interfaces resolved from concrete types by convention
- Windows named by a `WindowId` enum
- Windows created once at boot and only shown and hidden

## Decision Outcome

Chosen option: "two factories; windows asked for by interface", because it is the only option that satisfies ADR 0009 while *strengthening* ADR 0008's compile-time guarantee rather than trading it away, and it keeps each factory answering for one population.

`FactoryService` becomes `ComponentFactoryService`, behind `IComponentFactoryService`, and is otherwise untouched — same `where T : Component`, same prefab array, same `Get` / `Return`. It serves `Camera`, `CoroutineService`, `BoardView`, `UIRoot`, `CellView` and `MarkView`. Its constraint was never wrong; depending on it from an EditMode-tested type was.

`UIFactoryService`, behind `IUIFactoryService`, sits beside it, offering three things: get a panel, get a popup — each constrained to the matching window interface, so neither will accept the other's type — and return a window when it is done with.

It holds `IComponentFactoryService`, `IUIRoot` and `ICoroutineService`. Which layer a window is parented to is settled by *which method was called*, so nothing inspects a type at runtime to decide, and `UIService` never names a layer or touches a `RectTransform`.

Windows are asked for by interface. `IWindow` carries `IsVisible`, `Show()` and `Hide()`; `IPanel` and `IPopup` extend it and add nothing; `IGameplayPanel` and `IConfirmQuitPopup` add the surface their callers actually use. `Window`, `Panel` and `Popup` remain exactly as they are and implement them.

`Window.Construct(ICoroutineService)` — the safe-area plumbing — moves to the factory, because the factory now owns creation and construction is part of creating. `IWindow` therefore stays free of `MonoBehaviour` concerns, and a fake window is four lines with no dependencies.

`UIService` is left holding one collaborator, `IUIFactoryService`, and becomes what ADR 0008 always described it as: two stacks and the rules governing them, with no engine types in sight. `IUIService.ShowPanel<TPanel>` becomes `where TPanel : class, IPanel`, which is a **narrower** constraint than today's `where TPanel : Panel` — every misuse ADR 0008 wanted caught at compile time is still caught, and `GameplayState` now writes `ShowPanel<IGameplayPanel>(panel => panel.Setup(session))`.

### On the two names

`UIFactoryService` was chosen over `WindowFactoryService` deliberately, and against the argument that it collides with the two `UI*` types already present. The objection is recorded here because it is real: `UIService` and `UIFactoryService` differ by one word and both parse as "the UI thing" when spoken, and `CONTEXT.md` already gives the thing this factory makes a canonical name — **Window** — with a hard boundary, "every window is either a panel or a popup; there is no third kind". Consistency with the `UI*` family was preferred to that specificity. If the pair proves confusable in review, renaming to `WindowFactoryService` reverses this cleanly and touches nothing but names.

`Get` / `Return` was kept over `Create` / `Destroy` so the two factories read as siblings, and because `Return` is the more useful word: it does not promise destruction. `ComponentFactoryService.Return` destroys today but is named to allow pooling later, and windows are the most likely thing to be pooled first.

### Positive Consequences

- A window in an EditMode test is a plain C# object, so ADR 0009's rule holds all the way through the UI layer.
- The compile-time guarantee ADR 0008 asked for gets stronger, not weaker: `where TPanel : class, IPanel` rejects more misuse than `where TPanel : Panel` did.
- No registration code exists anywhere. The prefab array in the Inspector is the registry, and adding a window means adding a prefab to it — the same gesture as today.
- `UIService` becomes pure logic with a single collaborator, so its rules are readable without reference to Unity.
- Each factory answers for one population, so neither test double has to serve needs it was never designed for.
- Where a window is parented follows from the method called, not from a runtime type check.

### Negative Consequences

- Every window costs an interface, and its public surface is declared twice — once on the interface and once on the class. Two windows exist today, so the machinery currently outnumbers what it manages.
- `Return(IWindow)` must cast to a `Component` at runtime to reach the `GameObject` it destroys. That cast is unchecked by the compiler and will throw for a window that is not a component — which, in an EditMode test, every window is.
- Two factories mean two places to look when something is not being created, and a new contributor must learn which population their type belongs to before they can ask for it.
- Interface resolution through `TryGetComponent` is invisible in the type system: nothing at compile time says a prefab implementing `IGameplayPanel` has been registered, and a missing one still fails only at runtime — the same weakness ADR 0006 already recorded for `CoroutineService`.
- `UIFactoryService` sits one word away from `UIService`, and the two will be confused in conversation before they are confused in code.

## Pros and Cons of the Options

### Two factories; windows asked for by interface

`ComponentFactoryService` keeps `where T : Component` for scene furniture and spawned views. `UIFactoryService` creates windows by interface, parents them by layer, and constructs them.

- Good, because it satisfies ADR 0009 without weakening any constraint.
- Good, because it tightens ADR 0008's compile-time guarantee.
- Good, because the prefab array remains the only registry, so no registration code is written.
- Good, because each factory serves one population and each double is honest.
- Bad, because every window costs an interface for an app that has two.
- Bad, because destroying a window needs an unchecked runtime cast.

### One factory relaxed to `where T : class` and taught about layers

A single `FactoryService` resolves interfaces, holds `IUIRoot`, and parents a window according to whether the instance is an `IPanel` or an `IPopup`.

- Good, because there is exactly one factory type and one place creation happens.
- Good, because it needs no second interface and no second double.
- Bad, because `where T : class` lets `Get<string>()` compile and fail at runtime, weakening a guarantee that currently holds project-wide.
- Bad, because `Return` loses its compile-time promise of receiving a component.
- Bad, because the factory that spawns mark views would carry knowledge of UI layering, which nothing else needs.
- Bad, because layer choice becomes a runtime type test rather than a consequence of the call.

### Per-window interfaces resolved from concrete types by convention

The factory derives `GameplayPanel` from `IGameplayPanel` by name.

- Good, because it needs neither registration nor a prefab array entry per interface.
- Bad, because it is reflection — the exact mechanism ADR 0009 exists to remove — relocated from tests into production, where its failures reach players rather than a test runner.
- Bad, because a rename silently breaks resolution with nothing at compile time to catch it.

### Windows named by a `WindowId` enum

`ShowPanel(WindowId.Gameplay)`, with configuration passed as data.

- Good, because it introduces no per-window interfaces at all.
- Good, because the set of windows is enumerable in one place.
- Bad, because the typed `configure` callback dies, and with it `panel.Setup(session)` — configuration would have to travel as data through an untyped channel.
- Bad, because it discards a tested guarantee: that configuration runs before a window becomes visible.
- Bad, because the enum and the prefab array are two registries that can disagree.

### Windows created once at boot and only shown and hidden

`UIService` receives every window up front; nothing is instantiated on demand and the creation seam disappears.

- Good, because it removes the problem entirely rather than solving it — there is nothing to create, so nothing to name.
- Good, because it makes window construction cost nothing at navigation time.
- Bad, because it contradicts ADR 0008 and `CONTEXT.md`, which both state that a window "is created when it is first shown and destroyed when it is taken off its stack". Taking it means amending a decision and a glossary entry to serve a test constraint.
- Bad, because every window in the app stays resident for the app's lifetime, whether or not it is ever shown.

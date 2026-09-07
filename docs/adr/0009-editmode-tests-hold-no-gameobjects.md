# EditMode tests hold no `GameObject`s; anything that needs one is a PlayMode scene test

## Status

Accepted

## Context and Problem Statement

`FakeComponentFactoryService` handed back components built with `AddComponent`, then used `System.Reflection` to walk every declared field up the type hierarchy, pick out the ones typed `TMP_Text`, create a child `GameObject` for each, and assign it through `FieldInfo.SetValue`. Its own comment said why: "the real factory hands back a prefab instance whose children are already wired to its serialized fields. A bare component has none."

That reflection served exactly one field in the entire project — `GameplayPanel._turnText`, the only serialized `TMP_Text` in `TicTacToeLab.Runtime`. It existed because a test that shows a `GameplayPanel` eventually reaches `Setup`, which writes to that field, and a bare component's copy is null.

The reflection was a symptom, not the disease. `IComponentFactoryService.Get<T>()` is `where T : Component` and `IUIService.ShowPanel<TPanel>()` is `where TPanel : Panel`, so every window an EditMode test touches is forced to be a real component on a real `GameObject` whose prefab wiring nothing supplies. The fake's only options were to guess at that wiring or to let tests crash.

The same pressure has already surfaced twice more, both times accommodated rather than fixed. Five test files construct `BoardPresenter` with `factoryService: null`, because the presenter demands a factory it only forwards. And `GameCompleteStateTests` builds nine objects to assert that a one-second pause resets the game — a pause which, as it turns out, no test asserts at all.

What may an EditMode test require of the types it exercises?

## Decision Drivers

- Reflection over private serialized fields couples tests to names no compiler checks. It is a guess about how a prefab is wired, expressed as though it were a statement of fact.
- An unwired serialized field is silent until something dereferences it. `MarkView._oSprite` and `_xSprite` are never supplied by any test and nothing fails, because assigning a null sprite is harmless. `_turnText` differs only in that `SetText` throws. The hazard is invisible right up until it isn't, and the next `[SerializeField]` decides which kind it is.
- `factoryService: null`, five times over, is the same accommodation wearing different clothes: the design demands a collaborator the test cannot cheaply satisfy, so the test lies to it.
- One concrete dependency drags in an entire graph. `GameCompleteState` names `AppStateMachine` concretely and calls `ChangeState<GameplayState>()`, which throws unless a real `GameplayState` is registered, which needs a real `UIService`, which needs the factory — which is exactly where the reflection arrived.
- `UnityEngine.WaitForSeconds` keeps its duration in a private `m_Seconds` with no public accessor. A fake coroutine service taught to honour it would need reflection of its own, so the shape that leaves the pause untested is also the shape that would reintroduce what we are removing.
- Moving a test to PlayMode is not by itself a fix. PlayMode can `AddComponent` as freely as EditMode, and a bare `GameplayPanel` built that way has the identical null field. Whoever writes that test next meets the same wall and reaches for the same tool.
- [ADR 0008](./0008-panels-and-popups-are-two-stacks.md) already claims as a positive consequence that `UIService` is "a plain class holding two stacks of live instances, so every rule here is exercisable in EditMode with no scene and no play mode". That was nearly true. This makes it literally true.

## Considered Options

- EditMode holds no `GameObject`s; anything that needs one is a PlayMode scene test
- `GameObject`s allowed in EditMode, but no type may require prefab wiring
- EditMode tests instantiate the real prefabs

## Decision Outcome

Chosen option: "EditMode holds no `GameObject`s; anything that needs one is a PlayMode scene test", because it is the only option under which the reflection cannot come back — no EditMode test can ever again need a serialized field filled in, because no EditMode test has a component to fill in.

An EditMode test constructs plain C# objects and nothing else: no `GameObject`, no `AddComponent`, no `MonoBehaviour`, no `Component`. A type that cannot be exercised under that rule is not exercised in EditMode.

PlayMode tests load the real scene. `SceneWiringTests` already documents why every scene-loading test must share one fixture — the app enables input actions the moment it comes up, and a test outside the sandbox leaves those actions bound for the *next* test to trip over. Making every PlayMode test a scene test means that hazard cannot arise from a mixed suite.

The scene is the fixture, not the subject. A PlayMode test loads `Main.unity`, pulls real, fully wired collaborators out of it — `ComponentFactoryService`, `UIRoot`, `CoroutineService` — and constructs its subject against them where the subject is a plain class that happens to make `GameObject`s. A component under test is obtained from the scene's factory rather than assembled by hand, which is what makes its wiring real instead of simulated.

Four changes follow from the rule directly. How things are *created* is the subject of [ADR 0010](./0010-two-factories-and-windows-by-interface.md); these are the rest.

### `BoardModel` owns cell placements

`BoardPresenter` holds `IComponentFactoryService` only to forward it to `IBoardView.Construct`. Dropping it removes all five `factoryService: null` arguments at a stroke.

Placement-building goes with it. `CONTEXT.md` casts **BoardPresenter** as the type "that turns a press into a placed mark" and says nothing about laying out a board, while **BoardModel** is defined as owning "the board's dimensions and the geometry that resolves a local point to the cell containing it". Cell placements are geometry derived from dimensions, so they are already the model's by the glossary's own account; `BoardPresenter.BuildCellPlacements` was the drift. `IBoardView` then shrinks to `ToLocalPoint`, `ShowMark` and `Clear` — exactly what the presenter uses — and `FakeBoardView` becomes an honest double rather than a class implementing a method it ignores.

### The pause becomes a scheduled callback

`ICoroutineService` gains `RunAfter` — a delay in seconds and a callback — alongside `Run`. `GameCompleteState` stops writing a coroutine and stops naming a `UnityEngine` type: entering the state schedules the reset for one second later and keeps the handle, so leaving cancels it.

The fake records the delay and the callback, so a test asserts both that the pause is one second and when it fires. `Run(IEnumerator)` stays on the interface for `SafeAreaRect`, which is a `MonoBehaviour` and therefore PlayMode's business.

### States depend on seams, not on the graph

`IStateMachine` is deliberately narrow — a state may change to another state, and that is all it may do — because adding states and disposing the machine belong to `Bootstrap`. `IBoardSession` mirrors the existing surface: `Turn`, `GameEnded`, `TurnChanged`, `Reset()`. Together they take `GameCompleteStateTests` from nine objects to three.

### The leak tests become deliberate

`Re_entering_gameplay_after_each_game_does_not_accumulate_subscriptions` and `Many_games_in_a_row_leave_exactly_one_active_subscription` derive their entire value from the real object graph; faked out, they assert nothing. They move into an explicitly named integration test class that wires the real graph on purpose, so what they cover becomes a stated intent rather than an accident of how a setup grew.

`FakeComponentFactoryService` and `FakeUIRoot` are deleted. Neither has anything left to fake.

### Positive Consequences

- The reflection cannot return. Not by discipline — there is simply nothing in an EditMode test for it to act on.
- `factoryService: null` disappears from five files, and with it the pattern of satisfying a constructor with a lie.
- The one-second pause becomes an assertion for the first time.
- EditMode setups shrink to the collaborators a test actually reasons about, so what a test is *about* is legible from its `SetUp`.
- Every PlayMode test extends one fixture, closing the input-sandbox hazard `SceneWiringTests` warns about.
- Components under test are the components the game ships, because they come from the scene rather than from `AddComponent`.

### Negative Consequences

- **PlayMode tests are slow.** Every one pays a scene load plus a full `Bootstrap.Awake()`. The suite that used to run in milliseconds now has a tier that runs in seconds, and that tier will only grow.
- `AssetDatabase` and `EditorSceneManager` are editor-only, so PlayMode tests compile away on device behind `#if UNITY_EDITOR`, exactly as `UIWiringTests` already does. Running them on a device build would need a different loading strategy.
- The rule is a convention. Nothing in the compiler or the test runner stops someone writing `new GameObject()` in an EditMode test, and the first one to do it will look perfectly reasonable.
- More interfaces exist than the app strictly needs at its current size, and each is a type to keep in sync with its implementation.
- A component test in PlayMode sits further from its subject than the EditMode test it replaces: reaching a `CellView` now means loading a scene and asking a factory, where it used to mean one line.

## Pros and Cons of the Options

### EditMode holds no `GameObject`s; anything that needs one is a PlayMode scene test

EditMode is plain C#. Components are exercised in PlayMode against the real scene.

- Good, because the property is structural — no EditMode test can need wiring when it has no component to receive it.
- Good, because it forces the seams that make the state and UI setups small, which is a benefit beyond testing.
- Good, because it strengthens ADR 0008's claim about EditMode testability instead of eroding it.
- Good, because PlayMode components come from prefabs, so their wiring is real rather than inferred.
- Bad, because a whole tier of the suite becomes seconds-slow and editor-only.
- Bad, because it costs interfaces the app would not otherwise need at this size.

### `GameObject`s allowed in EditMode, but no type may require prefab wiring

Tests keep `AddComponent`, so `CanvasGroup` visibility and layer parenting stay covered in EditMode. No type under test may depend on a serialized reference being assigned.

- Good, because it is a far smaller change and keeps fast coverage of real component behaviour.
- Good, because `Window.Show()`/`Hide()` and layer parenting keep their EditMode tests.
- Bad, because it is a convention with no enforcement, and the next `[SerializeField] TMP_Text` reopens the hole exactly as `_turnText` did.
- Bad, because it leaves the constraint chain intact, so the setups stay large and `factoryService: null` stays justified.
- Bad, because it treats the visible symptom while preserving every condition that produced it.

### EditMode tests instantiate the real prefabs

The fake becomes prefab-backed: resolve from `Assets/_TicTacToeLab/Prefabs`, `Instantiate`, hand it back. Wiring is real, not simulated.

- Good, because it removes the reflection immediately and raises fidelity — the panel under test really is the panel that ships.
- Good, because it requires no new interfaces at all.
- Bad, because it couples unit-level navigation tests to prefab assets, so editing a prefab can break a test about stack ordering.
- Bad, because it needs `AssetDatabase`, which makes EditMode tests editor-only and measurably slower.
- Bad, because it leaves the constraint chain and the nine-object setups untouched.

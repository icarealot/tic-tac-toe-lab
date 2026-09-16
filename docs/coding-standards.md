# Coding Standards

## Production Code

### Naming

- Constants use `SNAKE_UPPER_CASE`.
- Private fields use camelCase with an `_` prefix. Add `[SerializeField]` when a field is editable in the Unity Editor.
- Production methods returning `IEnumerator` use an `IE_` prefix.
- Use `string.Empty` instead of `""`.
- During review, flag names that do not reveal what a function, variable, or type does or holds (**Mysterious Name**). Rename them; if no honest name emerges, clarify the design.

### Layout

Order class members as follows:

1. Events
2. Properties
3. Fields
4. Methods

### Unity

- Never use null propagation (`?.`, `??`, or `??=`) on Unity objects such as `MonoBehaviour`, `ScriptableObject`, and `Component`.
- Serialize button fields and wire `onClick` with `AddListener` in `Awake()` and `RemoveListener` in `OnDestroy()`; keep prefab `On Click ()` lists empty.
- Use TextMesh Pro's `SetText(...)` method instead of assigning through the `text` property.

### Design review

Use these grouped Fowler code smells (_Refactoring_, chapter 3) as review heuristics. Apply judgment rather than reporting them as hard violations, skip anything tooling already enforces, and defer to the standards above if they conflict.

- **Model concepts once** (**Duplicated Code**, **Data Clumps**, **Primitive Obsession**) — Remove repeated logic, group values that repeatedly travel together, and introduce a domain type when primitives obscure a meaningful concept.
- **Put behavior with its owner** (**Feature Envy**, **Message Chains**, **Middle Man**) — Keep behavior near the data it uses, hide long navigation chains behind an owner, and remove layers that merely delegate.
- **Localize change** (**Repeated Switches**, **Shotgun Surgery**, **Divergent Change**) — Centralize recurring type-based branches, gather code that changes together, and split modules that change for unrelated reasons.
- **Earn abstractions** (**Speculative Generality**, **Refused Bequest**) — Keep only abstractions required by current behavior, and prefer composition when a subtype cannot honor most inherited behavior.

## Testing

### Core principles

Design and review tests according to behavior risk, not test volume. Prefer the cheapest test that proves a caller-visible rule or contract.

### Test structure and naming

- Use `Assert.That`; name tests as plain-English descriptions of observable behavior, with underscores as spaces.
- Arrange state in each test. Use fixture fields and Unity setup or teardown only for centralized lifecycle ownership and reliable cleanup.
- Name a local primary subject `sut` and a fixture-held subject `_sut`; name collaborators descriptively. Tests without one honest subject are exempt.
- Separate Arrange, Act, and Assert with blank lines. Add the complete comment set only when a phase has multiple steps, loops, or unclear setup.
- Add behavior-named headers only for fixtures with multiple meaningful behavior groups.
- Parameterize scenarios or extract helpers only to remove meaningful duplication, never for formatting uniformity.

### What to test

- Retain a test only when it protects a named game rule, calculation, meaningful branch, state change, lifecycle behavior, infrastructure contract, external-service contract, or critical player journey. Test count, coverage, and bug history are not sufficient justification.
- Prioritize deterministic business logic. Test failure-sensitive infrastructure and external contracts, but do not unit-test trivial construction or logic-free pass-through wrappers.
- Test glue only when it owns meaningful branching, coordination, state, or lifecycle behavior.
- Use human playtests—not automation as proof—for feel, visuals, audio, controls, camera behavior, usability, and level design.
- Treat missing automation as a gap only when the behavior clears this risk threshold. Name the unprotected behavior and its cheapest sufficient fixture instead of requesting tests generically.

### How to observe behavior

- Verify behavior through a public seam: the boundary where a caller observes the result. Tests should read as specifications and survive behavior-preserving rewrites.
- Prefer an existing seam. Add a public seam for testing only when no existing boundary can express the behavior.
- Assert each rule primarily at its owning seam. Do not repeat the same outcome across layers unless another test protects a distinct integration, configuration, or wiring risk.
- Derive expected values from an independent rule, worked example, or known-good literal. Do not recreate the production calculation in the assertion.
- Exclude private sequencing, internal collaborator calls, and private-method behavior unless they are part of an agreed public contract.
- Assert ordering only when ordering is behavior. Use one lifecycle round trip when it establishes the invariant, and parameterize equivalent cases.
- Compare floating-point values with a tolerance. Compare `Vector3` and `Quaternion` values component by component with the same tolerance.
- Do not use mutable copy, styling, hierarchy, unrelated control counts, or another side channel as evidence of behavior.
- For uGUI behavior that does not involve pointer or raycast wiring, invoke the instantiated production button's `onClick` event. Use full pointer-to-EventSystem input only for a distinct integration risk.

### Choosing a fixture

Choose the least expensive fixture that proves the behavior:

1. **Plain EditMode test** — Use for deterministic behavior without `GameObject`s, scenes, prefabs, assets, frames, coroutine timing, or Unity lifecycle. Unity value types are acceptable. Test exact delays through an injected or fake scheduling boundary.
2. **Isolated PlayMode `GameObject`** — Use for focused component behavior, lifecycle, physics, or another engine integration. Create only the required objects and clean them up.
3. **Production prefab** — Use only when serialized configuration is part of the behavior.
4. **Production scene** — Use only when bootstrap or cross-object production wiring is part of the behavior.

Do not load a production scene merely to obtain a component. Identify production fixtures by stable domain role, and require every production-scene test to protect a distinct bootstrap or wiring risk. Keep scene journeys short by moving branches already established at deterministic seams into EditMode tests.

Reserve player-build checks for platform-sensitive behavior.

### Boundaries and synchronization

#### System boundaries

- Mock only at system boundaries. Common Unity boundaries include `Time`, `UnityEngine.Random`, `Input`, `PlayerPrefs`, `SceneManager`, `Application`, file IO, and backend calls for leaderboards, analytics, IAP, or ads.
- Construct owned code directly: instantiate plain rule classes, add `MonoBehaviour`s to isolated PlayMode objects, and create `ScriptableObject` configurations with `ScriptableObject.CreateInstance`. Build seams in code rather than adding assets to make dependencies reachable.
- Push deterministic rules into plain C# and keep `MonoBehaviour`s as thin adapters that provide engine values.
- Pass values before introducing interfaces: prefer `Advance(deltaSeconds)` over `IClock`, and a seeded `System.Random` over `IRandomSource`. Plain domain objects need no test doubles.
- When an API must remain behind the code under test, wrap it in a narrow, project-owned interface and inject that interface into the plain class that uses it. Give the boundary one method per operation rather than one generic request method.
- Do not dedicate tests to logic-free production wrappers. Test consuming rules with hand-written fakes that hold state and can supply inputs and failure paths.
- In unit tests, substitute third-party services through their service contract. Use a real integration or contract test only when a critical boundary behavior cannot be established with that substitute.
- Stub inputs and assert through the subject's public seam. Assert an outbound call only when the call itself is the contract, such as an analytics event or receipt submission, and verify it at that boundary.

```csharp
public interface ISaveStore
{
    string Read(string key);
    void Write(string key, string value);
}

public sealed class FakeSaveStore : ISaveStore
{
    public bool NextWriteFails { get; set; }
    private readonly Dictionary<string, string> _entries = new();

    public string Read(string key) =>
        _entries.TryGetValue(key, out var value) ? value : string.Empty;

    public void Write(string key, string value)
    {
        if (NextWriteFails)
            throw new IOException("Disk full");

        _entries[key] = value;
    }
}
```

#### PlayMode synchronization

- Synchronize on observable outcomes, not elapsed frames. `yield return null` advances the coroutine but does not prove that input, EventSystem processing, destruction, or a state transition completed.
- Send each simulated gesture exactly once, then wait for its public outcome with a bounded real-time deadline. A timeout fails the test; it never retries the gesture.
- Explicitly process queued Input System state changes when testing a project `InputAction` route.
- For uGUI pointer tests, use a shared helper that advances both Input System and EventSystem processing.
- Treat delayed presentation as an eventual outcome with a timeout rather than sleeping for the expected duration.

```csharp
Press(keyboard.escapeKey);
InputSystem.Update();
Release(keyboard.escapeKey);
InputSystem.Update();

yield return WaitUntilOrFail(
    () => IsPauseMenuVisible(),
    timeoutSeconds: 3f,
    "Back should open the pause menu.");
```

# Testing Standards

Review and design tests according to behavior risk, not test volume.

## Test value

- **Risk, not volume** — Every retained automated test protects a named game rule, calculation, meaningful branch, state change, lifecycle, infrastructure contract, external-service contract, or critical player journey. Test count, coverage, and a bug's history are not sufficient justification.
- **Priority** — Favor deterministic business logic. Test failure-sensitive infrastructure and external contracts, but do not unit-test trivial construction or logic-free pass-through wrappers. Test glue only when it owns meaningful branching, coordination, state, or lifecycle behavior. In unit tests, substitute third-party services through the service contract; use a real integration or contract test only when a critical boundary behavior cannot be established with that substitute.
- **Qualitative boundary** — Use human playtests for feel, visuals, audio, controls, camera behavior, usability, and level design rather than treating automation as proof of quality.

Treat missing automation as a gap only when the changed behavior clears the risk rule. Name the unprotected behavior and its cheapest sufficient fixture rather than requesting tests generically.

## Observable behavior

A seam is the public boundary where a caller observes behavior.

- Verify behavior through public interfaces rather than private methods or implementation details. A test should read as a specification and survive an internal rewrite that preserves behavior.
- Prefer an existing public seam. Add a new public seam for testing only when no existing boundary can express the behavior.
- Assert each rule primarily at its owning seam instead of repeating the same outcome through every architectural layer.
- Derive expected values from an independent rule, worked example, or known-good literal rather than reproducing the implementation calculation.
- Exclude private sequencing, incidental collaborator call counts, diagnostic text, styling, layout, and hierarchy unless they are the agreed contract. Screenshots and serialized-YAML snapshots qualify only when that exact output or structure is the contract.
- Assert exact ordering only when ordering is itself behavior. One lifecycle round trip is enough when it establishes the invariant. Parameterize equivalent cases.
- Compare floating-point values with a tolerance. Compare `Vector3` and `Quaternion` values component by component with the same tolerance.

Avoid these test-design anti-patterns:

- **Implementation-coupled** — The test mocks internal collaborators, tests private methods, or verifies through a side channel. It breaks during a behavior-preserving refactor.
- **Tautological** — The assertion recreates the production calculation or derives its expected result through the same logic, so implementation and oracle cannot disagree.
- **Duplicated assertion layers** — Several fixtures prove the same rule without protecting a distinct integration, configuration, or wiring risk.

## Smallest sufficient fixture

Choose the cheapest fixture that proves the behavior:

1. Use a plain EditMode test for deterministic behavior. Create no `GameObject`s and depend on no scene, prefab, asset, frame, coroutine timing, or Unity lifecycle. Unity value types are acceptable.
2. Use an isolated PlayMode `GameObject` for focused component behavior, lifecycle, physics, or another engine integration.
3. Use a production prefab only when its serialized configuration is part of the behavior.
4. Use a production scene only when bootstrap or cross-object production wiring is part of the behavior.

Do not load a production scene merely to obtain a component. Identify every production fixture by stable domain role, extend an existing scene-level critical journey when it remains readable, and require every production-scene test to identify a distinct bootstrap or wiring risk. Create only the objects a focused PlayMode test needs and clean them up.

Creating or modifying a scene, prefab, `.asmdef`, or other non-code asset merely to support a test requires task-specific user approval naming the exact file or narrow file group before editing.

Reserve player-build checks for platform-sensitive behavior and human playtests for qualitative behavior.

## Test doubles at system boundaries

Mock at system boundaries only. In Unity, boundaries that require a seam are commonly static, sealed, or engine-constructed APIs such as `Time`, `UnityEngine.Random`, `Input`, `PlayerPrefs`, `SceneManager`, `Application`, file IO, and backend calls for leaderboards, analytics, IAP, or ads.

Construct owned code directly in tests: plain rule classes, `MonoBehaviour`s through `AddComponent` in PlayMode, and `ScriptableObject` configurations through `ScriptableObject.CreateInstance`. Build seams in code rather than adding scenes, prefabs, or `.asmdef` files to make dependencies reachable.

### Push rules out of the engine

Keep deterministic game rules in plain C# and make `MonoBehaviour`s thin adapters that supply engine values.

```csharp
// Avoid: the rule only runs inside the engine.
public sealed class ManaRegenerator : MonoBehaviour
{
    private ManaPool _pool;
    private float _ratePerSecond;

    private void Update() => _pool.Add(_ratePerSecond * Time.deltaTime);
}

// Prefer: the adapter passes the engine value to a plain rule.
public sealed class ManaRegeneration
{
    private readonly ManaPool _pool;
    private readonly float _ratePerSecond;

    public void Advance(float deltaSeconds) =>
        _pool.Add(_ratePerSecond * deltaSeconds);
}
```

Pass an engine value as a parameter before introducing an interface: `Advance(deltaSeconds)` is preferable to `IClock`, and a seeded `System.Random` is preferable to `IRandomSource`. Plain domain objects such as `DamageCalculator` and `ManaPool` need no test doubles.

### Wrap APIs that cannot be substituted

When a boundary must remain behind the code under test, define a narrow interface owned by the project. A logic-free production wrapper needs no dedicated test; test the rule that consumes it with a fake.

```csharp
public interface ISaveStore
{
    string Read(string key);
    void Write(string key, string value);
}

public sealed class PlayerPrefsSaveStore : ISaveStore
{
    public string Read(string key) =>
        PlayerPrefs.GetString(key, string.Empty);

    public void Write(string key, string value) =>
        PlayerPrefs.SetString(key, value);
}
```

Inject the interface through the constructor of the plain class that uses it so the test selects the implementation.

Give a boundary one method per operation rather than one generic request method. This lets a fake return one shape without branching on endpoint arguments.

```csharp
public interface ILeaderboardClient
{
    Task<IReadOnlyList<ScoreEntry>> GetTopScores(int count);
    Task SubmitScore(string playerId, int score);
}
```

### Prefer hand-written fakes

A fake is a small sealed class that holds real state. It should supply inputs and failure paths while letting the test assert through the public boundary.

```csharp
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

Stub inputs rather than asserting collaborator calls. Assert an outbound call only when the call itself is the contract, such as an analytics event or a receipt submitted for validation, and assert it at that boundary's seam rather than as evidence about an internal collaborator.

## Examples

### EditMode rule

```csharp
using NUnit.Framework;

public sealed class ManaPoolTests
{
    [Test]
    public void Spending_more_mana_than_is_available_leaves_the_mana_pool_unchanged()
    {
        var sut = new ManaPool(available: 3);

        var spent = sut.TrySpend(cost: 5);

        Assert.That(spent, Is.False);
        Assert.That(sut.Available, Is.EqualTo(3));
    }
}
```

### Isolated PlayMode lifecycle

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class LifecycleTests
{
    private GameObject _gameObject;
    private DestroyOnStart _sut;

    [SetUp]
    public void SetUp()
    {
        _gameObject = new GameObject("Temporary object");
        _sut = _gameObject.AddComponent<DestroyOnStart>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_gameObject != null)
            Object.DestroyImmediate(_gameObject);
    }

    [UnityTest]
    public IEnumerator An_object_marked_as_temporary_on_start_is_removed_from_the_scene()
    {
        // Arrange
        // SetUp owns the subject and its cleanup.

        // Act
        yield return null;

        // Assert
        Assert.That(_sut == null, Is.True);
    }
}
```

### Verify through the seam

```csharp
sut.Save(new PlayerProfile(name: "Alice", level: 7));

var loaded = sut.Load();

Assert.That(loaded.Name, Is.EqualTo("Alice"));
Assert.That(loaded.Level, Is.EqualTo(7));
```

Reading a private serialized field or querying storage directly would bypass the caller-visible seam and couple the test to implementation.

### Use an independent oracle

```csharp
var attack = new Attack(power: 40f);
var target = new Combatant(armor: 0.25f);

var damage = DamageCalculator.Resolve(attack, target);

// The game rule's worked example says 40 power against 25% armor deals 30.
Assert.That(damage, Is.EqualTo(30f).Within(0.01f));
```

# Unity mocking

Mock at **system boundaries** only. In Unity the boundary has a mechanical tell: it is `static`, `sealed`, or constructed by the engine, so a substitute cannot stand in for it — `Time`, `UnityEngine.Random`, `Input`, `PlayerPrefs`, `SceneManager`, `Application`, file IO, and backend calls (leaderboards, analytics, IAP, ads).

Everything you own is constructed directly in the test: rules classes, `MonoBehaviour`s (`AddComponent` in PlayMode), and `ScriptableObject` configs (`ScriptableObject.CreateInstance`). Build the seam in code — never add a scene, prefab, or `.asmdef` to make a dependency reachable.

## Push the rule out of the engine

The cheapest fake is the one you never write. Keep the game rule in a plain C# class and leave the `MonoBehaviour` as a thin adapter that feeds it engine values.

```csharp
// Avoid: the rule only runs inside the engine, so the test needs the engine.
public sealed class ManaRegenerator : MonoBehaviour
{
    private ManaPool pool;
    private float ratePerSecond;

    private void Update() => pool.Add(ratePerSecond * Time.deltaTime);
}

// Prefer: the rule is a plain class the adapter drives.
public sealed class ManaRegeneration
{
    private readonly ManaPool pool;
    private readonly float ratePerSecond;

    public void Advance(float deltaSeconds) => pool.Add(ratePerSecond * deltaSeconds);
}
```

Pass the engine value as a parameter before reaching for an interface: `Advance(deltaSeconds)` beats an `IClock`, and a seeded `System.Random` beats an `IRandomSource`. `DamageCalculator` and `ManaPool` need no test doubles at all.

## Wrap what you cannot substitute

When the boundary must stay behind the code under test, define a narrow interface you own. The wrapper is a pass-through with no logic, so it carries no tests; the rule that depends on it is tested against a fake.

```csharp
public interface ISaveStore
{
    string Read(string key);
    void Write(string key, string value);
}

// Pass-through only — no branching, no tests.
public sealed class PlayerPrefsSaveStore : ISaveStore
{
    public string Read(string key) => PlayerPrefs.GetString(key, string.Empty);
    public void Write(string key, string value) => PlayerPrefs.SetString(key, value);
}
```

Inject the interface through the constructor of the class that uses it, so the test chooses the implementation.

## Name each operation

Give the boundary one method per operation rather than one generic call, so a fake returns one shape without branching on its arguments.

```csharp
// Prefer: each operation is independently fakeable.
public interface ILeaderboardClient
{
    Task<IReadOnlyList<ScoreEntry>> GetTopScores(int count);
    Task SubmitScore(string playerId, int score);
}

// Avoid: every fake needs conditional logic on the endpoint.
public interface ILeaderboardClient
{
    Task<string> Send(string endpoint, string payload);
}
```

## Write the fake by hand

A fake is a small `sealed` class holding real state. It reads as part of the test, needs no package, and lets you assert through the boundary instead of through call records.

```csharp
public sealed class FakeSaveStore : ISaveStore
{
    private readonly Dictionary<string, string> entries = new();

    public bool NextWriteFails { get; set; }

    public string Read(string key) =>
        entries.TryGetValue(key, out var value) ? value : string.Empty;

    public void Write(string key, string value)
    {
        if (NextWriteFails)
            throw new IOException("Disk full");

        entries[key] = value;
    }
}
```

Reach for a substitute library (NSubstitute) only to stub a wide interface you would otherwise implement member by member.

## Stub inputs, do not assert calls

A test double supplies **inputs**: a value to return, a failure path to force. Verifying that a collaborator was called — `Received(1)`, call counts, argument matchers — couples the test to the implementation (see the anti-patterns in [SKILL.md](SKILL.md)).

```csharp
var store = new FakeSaveStore { NextWriteFails = true };
var saveService = new SaveService(store);

Assert.Throws<IOException>(() =>
    saveService.Save(new PlayerProfile(name: "Alice", level: 7)));
```

Assert the outbound call itself only when that call *is* the contract — an analytics event, a receipt submitted for validation — and assert it at that boundary's own seam, never as evidence about an internal collaborator.

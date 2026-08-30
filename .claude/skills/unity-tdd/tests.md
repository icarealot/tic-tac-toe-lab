# Unity test examples

## EditMode

Use EditMode tests for deterministic game rules that do not need a running scene.

```csharp
using NUnit.Framework;

public sealed class ManaPoolTests
{
    [Test]
    public void Spending_more_mana_than_is_available_leaves_the_mana_pool_unchanged()
    {
        var mana = new ManaPool(available: 3);

        var spent = mana.TrySpend(cost: 5);

        Assert.That(spent, Is.False);
        Assert.That(mana.Available, Is.EqualTo(3));
    }
}
```

## PlayMode

Use PlayMode tests only when behavior depends on Unity lifecycle or engine integration.

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class LifecycleTests
{
    [UnityTest]
    public IEnumerator An_object_marked_as_temporary_on_start_is_removed_from_the_scene()
    {
        var gameObject = new GameObject("Temporary object");
        _ = gameObject.AddComponent<DestroyOnStart>();

        yield return null;

        var wasDestroyed = gameObject == null;
        if (gameObject != null)
            Object.Destroy(gameObject);

        Assert.That(wasDestroyed, Is.True);
    }
}
```

Prefer the narrowest public Unity boundary that proves the behavior. Clean up created objects when the behavior under test does not destroy them.

## Verify through the seam

Observe the result through the same public boundary a caller would use, not through a side channel into the object's internals or its serialized form.

```csharp
// Avoid: reaches past the seam into a private serialized field.
var field = typeof(SaveService).GetField("cachedJson", BindingFlags.NonPublic | BindingFlags.Instance);
Assert.That(field.GetValue(saveService), Does.Contain("Alice"));

// Prefer: round-trip through the public API.
saveService.Save(new PlayerProfile(name: "Alice", level: 7));

var loaded = saveService.Load();

Assert.That(loaded.Name, Is.EqualTo("Alice"));
Assert.That(loaded.Level, Is.EqualTo(7));
```

## Assert independent values

The expected value comes from the game rules, not from the production calculation re-run in the test.

```csharp
var attack = new Attack(power: 40f);
var target = new Combatant(armor: 0.25f);

// Avoid: recomputes the formula, so it passes whatever the formula becomes.
var expected = attack.Power * (1f - target.Armor);
Assert.That(DamageCalculator.Resolve(attack, target), Is.EqualTo(expected));

// Prefer: a worked example from the game rules.
// 40 power against 25% armor deals 30.
Assert.That(DamageCalculator.Resolve(attack, target), Is.EqualTo(30f).Within(0.01f));
```

Compare floats with a tolerance (`Is.EqualTo(x).Within(...)`), and `Vector3`/`Quaternion` component-wise with the same tolerance.

## Avoid implementation-coupled tests

Do not test private methods, internal call counts, or arbitrary frame-by-frame implementation sequences. Test externally observable gameplay behavior.

```csharp
// Avoid: verifies an internal collaborator.
audioService.Received(1).Play("hit");

// Prefer: verify the public result at the agreed seam.
Assert.That(target.Health, Is.EqualTo(7));
```

Do not treat screenshots, snapshots of serialized YAML, or exact hierarchy layouts as default assertions. Use them only when the serialized or visual structure is itself the agreed public contract.

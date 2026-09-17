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

### Select validation

For each behavior under consideration:

1. Name the caller-visible rule or contract and the risk that warrants validation.
2. Decide whether automation can prove it or human judgment is required.
3. Select the cheapest sufficient validation tier for that behavior. A feature may use multiple tiers, but each must address a distinct named risk.
4. Record the selected checks and the rationale for any deliberate omission. Request a missing test by naming the unprotected behavior and its cheapest sufficient fixture, not by appealing to test count, coverage, or bug history.

Retain an automated test only when it protects a named game rule, calculation, meaningful branch, state change, lifecycle behavior, infrastructure contract, external-service contract, or critical player journey. Prioritize deterministic business logic. Validate failure-sensitive infrastructure and external contracts, but omit trivial construction, logic-free pass-through wrappers, and glue that owns no meaningful branching, coordination, state, or lifecycle behavior.

Use human judgment for feel, visuals, audio, controls, camera behavior, usability, and level design. Automation is not proof of these qualities.

#### UI validation boundary

Automate UI behavior such as:

- Navigation and app-state transitions.
- Window stack and visibility rules.
- Button callbacks and input routing.
- Meaningful interaction branches.
- Bootstrap and production wiring risks.
- Deterministic layout calculations such as safe-area arithmetic.

Validate presentation manually, including:

- Colors, typography, spacing, alignment, and visual hierarchy.
- Whether layouts look correct across aspect ratios.
- Animation appearance, cadence, smoothness, and feel.
- Decorative text and image presentation.
- Exact `RectTransform`, prefab hierarchy, and serialized styling values.

Do not automate assertions about prefab styling, hierarchy, anchors, font sizes, colors, or presentation-only animation frames. Do not use real-time waits to prove visual animation timing. When UI combines behavior and presentation, automate the behavior through its cheapest nonvisual public seam and record the presentation requirements as a manual checklist.

#### Validation tiers

1. **Plain EditMode** — Deterministic behavior that needs no `GameObject`, scene, prefab, asset, frame, coroutine timing, or Unity lifecycle. Unity value types are acceptable. Test exact delays through an injected or fake scheduling boundary.
2. **Isolated PlayMode** — Focused component behavior, lifecycle, physics, or another engine integration. Create only the required `GameObject`s and clean them up.
3. **Production Prefab** — Serialized production configuration is part of the behavior. Identify the fixture by stable domain role and name the distinct configuration or runtime wiring risk.
4. **Production Scene** — Bootstrap or cross-object production wiring is part of the behavior. Identify the scene by stable domain role and name the distinct risk. Keep journeys short by proving branches at deterministic seams in EditMode; never load a scene merely to obtain a component.
5. **Player Build** — Platform-sensitive behavior that cannot be established in the Editor.
6. **Human Playtest** — Presentation, feel, or another quality that requires human judgment. Record concrete manual checks.

### Observe behavior

- Verify behavior through a public seam where a caller observes the result. Tests should read as specifications and survive behavior-preserving rewrites. Prefer an existing seam; add one for testing only when no existing boundary can express the behavior.
- Assert each rule primarily at its owning seam. Repeat an outcome at another layer only to protect a distinct integration, configuration, or wiring risk.
- Derive expected values from an independent rule, worked example, or known-good literal rather than recreating the production calculation.
- Exclude private sequencing, internal collaborator calls, and private-method behavior unless they are part of an agreed public contract.
- Assert ordering only when ordering is behavior. Use one lifecycle round trip when it establishes the invariant, and parameterize equivalent cases.
- Use the behavior's result as evidence. Mutable copies, styling, hierarchy, unrelated control counts, and other side channels do not prove behavior.
- For uGUI behavior that does not involve pointer or raycast wiring, invoke the instantiated production button's `onClick` event. Use full pointer-to-`EventSystem` input only for a distinct integration risk.

### Handle system boundaries

- Substitute only at system boundaries. Common Unity boundaries include `Time`, `UnityEngine.Random`, `Input`, `PlayerPrefs`, `SceneManager`, `Application`, file IO, and backend calls for leaderboards, analytics, IAP, or ads.
- Construct owned code directly: instantiate plain rule classes, add `MonoBehaviour`s to isolated PlayMode objects, and create `ScriptableObject` configurations with `ScriptableObject.CreateInstance`. Build seams in code rather than adding assets only to make dependencies reachable.
- Push deterministic rules into plain C# and keep `MonoBehaviour`s as thin adapters that provide engine values.
- Pass values before introducing interfaces: prefer `Advance(deltaSeconds)` over `IClock`, and a seeded `System.Random` over `IRandomSource`. Plain domain objects need no test doubles.
- When an API must remain behind the subject, wrap it in a narrow, project-owned interface and inject it into the plain class that uses it. Give the boundary one method per operation rather than one generic request method.
- Test consuming rules with hand-written, state-holding fakes that can supply inputs and failure paths; do not dedicate tests to logic-free production wrappers.
- Substitute third-party services through their service contracts. Use a real integration or contract test only when a critical boundary behavior cannot be established with the substitute.
- Stub inputs and assert through the subject's public seam. Assert an outbound call only when the call itself is the contract, such as an analytics event or receipt submission, and verify it at that boundary.

### Write the test

- Use `Assert.That`; name tests as plain-English descriptions of observable behavior, with underscores as spaces.
- Name a local primary subject `sut` and a fixture-held subject `_sut`; name collaborators descriptively. Tests without one honest subject are exempt.
- Arrange state in each test. Use fixture fields and Unity setup or teardown only for centralized lifecycle ownership and reliable cleanup.
- Separate Arrange, Act, and Assert with blank lines. Add the complete comment set only when a phase has multiple steps, loops, or unclear setup.
- Add behavior-named headers only for fixtures with multiple meaningful behavior groups.
- Parameterize scenarios or extract helpers only to remove meaningful duplication, never for formatting uniformity.

### Synchronize PlayMode behavior

- Synchronize on observable outcomes, not elapsed frames. `yield return null` advances the coroutine but does not prove that input, `EventSystem` processing, destruction, or a state transition completed.
- Send each simulated gesture exactly once, then wait for its public outcome with a bounded real-time deadline. A timeout fails the test; it never retries the gesture.
- Explicitly process queued Input System state changes when testing a project `InputAction` route.
- For uGUI pointer tests, use a shared helper that advances both Input System and `EventSystem` processing.
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

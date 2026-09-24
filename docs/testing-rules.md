# Testing Rules

Build the smallest sufficient validation portfolio: use the cheapest sufficient level and smallest fixture for each distinct caller-visible behavior or risk.

1. Name the caller-visible contract and the risk that warrants validation.
2. Decide whether automation can prove it or human judgment is required.
3. Select the smallest validation level that can prove it. Use several levels only when each covers a distinct risk.
4. Retain a test only while it supplies durable evidence not already established at a cheaper owning seam.
5. Communicate the selected checks and the reason for any deliberate omission. For human validation, explicit user confirmation in the current task is sufficient evidence; do not require a repository checklist or permanent validation artifact unless the user requests one.

Test quantity and production-to-test line ratios are not goals. When requesting missing coverage, name the unprotected behavior and its cheapest sufficient fixture rather than appealing to test count, coverage, or bug history.

Automate named game rules, calculations, meaningful branches, state changes, lifecycle behavior, infrastructure or external-service contracts, and critical player journeys through non-production seams. Prioritize deterministic business logic and failure-sensitive infrastructure or external contracts. Remove temporary probes, superseded cases, duplicate evidence, and tests for trivial construction, logic-free pass-through wrappers, or glue with no meaningful branching, coordination, state, or lifecycle behavior.

## Validation levels

### 1. Plain EditMode

Use for deterministic behavior that needs no `GameObject`, scene, prefab, asset, frame, coroutine timing, or Unity lifecycle. Unity value types are fine. Test exact delays through an injected or fake scheduling boundary.

### 2. Isolated Unity validation

- **Isolated PlayMode:** Use for focused component, lifecycle, physics, or other engine behavior. Create only the required `GameObject`s and clean them up.

### 3. Platform and human validation

- **Player Build:** Use for platform-sensitive behavior that cannot be established in the Editor.
- **Human Playtest:** Use for production prefab and scene wiring, presentation, feel, audio, controls, camera behavior, usability, level design, and other qualities assigned to human validation. Identify each production fixture by stable domain role and state the exact interaction and observable outcome to the user. The validation is complete when the user explicitly confirms that outcome is acceptable; no repository checklist is required.

An implementation may create an automated production-prefab or production-scene check when it materially helps establish wiring behavior. Treat it as a disposable implementation aid: remove its code before completion and leave the final production-asset check to a user-confirmed Human Playtest.

## UI validation boundary

Automate UI behavior such as navigation and state transitions, window-stack and visibility rules, button callbacks and input routing, meaningful interaction branches, bootstrap behavior through non-production seams, and deterministic layout calculations such as safe-area arithmetic.

Validate presentation manually, including colors, typography, spacing, alignment, visual hierarchy, aspect-ratio behavior, animation appearance, cadence, smoothness and feel, decorative text or images, and exact `RectTransform` or serialized styling values. Do not assert prefab styling, hierarchy, anchors, font sizes, colors, or presentation-only animation frames. Do not use real-time waits to prove visual animation timing. When behavior and presentation are combined, automate behavior through the cheapest nonvisual public seam and state the presentation requirements to the user for confirmation.

## Observe behavior

- Tests should read as specifications and survive behavior-preserving rewrites.
- Assert each rule at its owning seam. Repeat an outcome elsewhere only for a distinct integration, configuration, or wiring risk.
- Derive expected values from an independent rule, worked example, or known-good literal rather than copying the production calculation.
- Do not assert private sequencing, internal collaborator calls, or private methods unless they are part of an agreed public contract. Assert ordering only when ordering is behavior; use one lifecycle round trip when it establishes an invariant and parameterize equivalent cases.
- Use the behavior's result as evidence. Mutable copies, styling, hierarchy, unrelated control counts, and other side channels do not prove behavior.
- For uGUI behavior without pointer or raycast wiring, invoke the isolated fixture's button `onClick` event after `Awake()` has run. Use full pointer-to-`EventSystem` input only for that distinct integration risk.

## Handle system boundaries

- Substitute only at system boundaries, such as `Time`, `UnityEngine.Random`, `Input`, `PlayerPrefs`, `SceneManager`, `Application`, file IO, and backend services.
- Construct owned code directly: instantiate plain rule classes, add `MonoBehaviour`s to isolated PlayMode objects, and create `ScriptableObject` configurations with `ScriptableObject.CreateInstance`. Build seams in code instead of adding assets only to make dependencies reachable.
- Push deterministic rules into plain C# and keep `MonoBehaviour`s as thin adapters that provide engine values.
- Pass values before introducing interfaces: use `Advance(deltaSeconds)` instead of `IClock`, and a seeded `System.Random` instead of `IRandomSource`. Plain domain objects need no test doubles.
- When an API must remain behind the subject, wrap it in a narrow, project-owned interface and inject it into the plain class that uses it. Give each boundary one method per operation rather than one generic request method.
- Test consuming rules with hand-written, state-holding fakes that supply inputs and failure paths. Do not dedicate tests to logic-free production wrappers.
- Substitute third-party services through their service contracts. Use a real integration or contract test only when a critical boundary behavior cannot be established with the substitute.
- Stub inputs at system boundaries. Assert an outbound call only when the call itself is the contract, such as an analytics event or receipt submission, and verify it at that boundary.

## Synchronize PlayMode behavior

- Synchronize on observable outcomes, not elapsed frames. `yield return null` advances a coroutine but does not prove that input, `EventSystem` processing, destruction, or a state transition completed.
- Send each simulated gesture exactly once, then wait for its public outcome with a bounded real-time deadline. A timeout fails the test; it never retries the gesture.
- Explicitly process queued Input System state changes when testing a project `InputAction` route.
- For uGUI pointer tests, use a shared helper that advances both Input System and `EventSystem` processing.
- Treat delayed presentation as an eventual outcome with a timeout rather than sleeping for the expected duration.

```csharp
Press(keyboard.escapeKey);
InputSystem.Update();
Release(keyboard.escapeKey);
InputSystem.Update();

yield return IE_WaitUntilOrFail(
    () => IsPauseMenuVisible(),
    timeoutSeconds: 3f,
    "Back should open the pause menu.");
```

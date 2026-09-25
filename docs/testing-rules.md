# Testing Rules

Build the smallest sufficient validation portfolio. Every test must protect a named caller-visible behavior or risk at the cheapest level that can prove it.

## Choose the test level

### Unit tests

Use Plain EditMode for deterministic behavior that does not require a `GameObject`, scene, asset, frame, coroutine, or Unity lifecycle.

### Integration tests

- Use PlayMode for component, lifecycle, physics, input, or other engine behavior. Prefer isolated fixtures with only the required objects.
- Use production prefabs when testing distinct wiring risks. Prove wiring through observable behavior rather than exact hierarchy or serialized values.

### E2E tests

Use PlayMode with production scenes for critical player journeys. Keep these tests sparse and leave detailed rule and branch coverage to cheaper levels.

## Human playtesting

- Use human playtesting for presentation, feel, audio, controls, camera behavior, usability, level design, and other qualities that require human judgment.
- Use a Player Build when validating platform-specific behavior or the shipped player experience.

## Do

- Automate game rules, calculations, meaningful branches, state changes, lifecycle behavior, failure-sensitive boundaries, wiring risks, and critical player journeys.
- Test caller-visible behavior through the narrowest public seam.
- Assert each rule at its owning seam. Test it at another level only when that level covers a distinct risk.
- Derive expected values independently from the production calculation.
- Substitute Unity systems, external services, and other system boundaries. Construct owned code directly.
- Pass values before introducing interfaces. When substitution is necessary, use a narrow project-owned interface.
- Synchronize PlayMode tests on observable outcomes with bounded timeouts.

## Don’t

- Don’t automate presentation details such as styling, exact hierarchy, animation appearance, cadence, or feel.
- Don’t assert private methods, internal call sequences, or implementation details unless they are part of the public contract.
- Don’t use arbitrary frame counts or real-time delays to prove completion or visual timing.
- Don’t add tests for coverage targets, test counts, bug history alone, trivial construction, logic-free wrappers, or glue without meaningful behavior.
- Don’t retain duplicate tests when a cheaper owning seam already provides the same evidence.

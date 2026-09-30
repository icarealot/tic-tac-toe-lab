# Testing Rules

Build the smallest sufficient validation portfolio. Each test must protect a named, caller-visible behavior or risk at the cheapest level that can prove it.

## Test structure

Use Arrange, Act, Assert (AAA):

- **Arrange:** Put the SUT and its dependencies in the required state.
- **Act:** Make one call to the SUT and capture its result, if any.
- **Assert:** Verify an observable outcome or meaningful interaction.

Also:

- Separate AAA sections with blank lines. Add comments when that is not possible.
- Use `Assert.That`; do not assert implementation details.
- Keep scenarios explicit instead of branching inside tests.
- Extract large Arrange sections into factories, helpers, or base classes.
- Use setup and teardown only for shared lifecycle ownership and reliable cleanup.
- Keep assertions focused; split oversized tests or extract named assertion helpers.
- Name a primary local SUT `sut` and a fixture-held SUT `_sut`. Tests without an honest subject are exempt.
- Name tests after the scenario and expected behavior, using underscores and no MUT name.
- Parameterize only when it removes meaningful duplication without hiding the behavior; otherwise use separate positive and negative tests.

## Choose the test level

### Unit tests

Use Plain EditMode for deterministic behavior that does not require a `GameObject`, scene, asset, frame, coroutine, or Unity lifecycle.

- Test one unit's behavior and outcome or meaningful interaction.
- Keep tests fast and isolated; normally avoid setup and teardown.
- Construct owned code directly and substitute Unity or external boundaries with test doubles.
- Make unit tests the majority of the suite.

### Integration tests

Use PlayMode for component, lifecycle, physics, input, or other Unity-engine behavior.

- Use isolated fixtures with only the required objects.
- Use production prefabs when testing distinct wiring risks; prove wiring through observable behavior, not exact hierarchy or serialized values.
- Test shared dependencies, external services, or code owned by another team.
- Setup and teardown are acceptable; integration tests form the middle layer.

### End-to-end tests

Use PlayMode with production scenes for critical player journeys from the user's point of view, including most external dependencies.

Keep E2E tests sparse. Cover detailed rules and branches at cheaper levels; E2E tests should be the minority of the suite.

## Human playtesting

Use human playtesting for presentation, feel, audio, controls, camera behavior, usability, and level design. Use a Player Build for platform-specific behavior and the shipped player experience.

## Do

- Integrate test execution into the development cycle.
- Automate game rules, calculations, meaningful branches, state changes, lifecycle behavior, failure-sensitive boundaries, wiring risks, and critical player journeys.
- Test caller-visible behavior through the narrowest public seam.
- Assert each rule at its owning seam; test it at another level only for a distinct risk.
- Derive expected values independently from production calculations.
- Prefer passing values before introducing interfaces; when substitution requires one, use a narrow project-owned interface.
- Synchronize PlayMode tests on observable outcomes with bounded timeouts.

## Don't

- Don't automate presentation details, exact hierarchy, animation appearance, cadence, or feel.
- Don't assert private methods, internal call sequences, or implementation details unless they are part of the public contract.
- Don't use arbitrary frame counts or real-time delays to prove completion or visual timing.
- Don't add tests solely for coverage or test-count targets, bug history, trivial construction, logic-free wrappers, or glue without meaningful behavior.
- Don't retain duplicate tests when a cheaper owning seam provides the same evidence.

# Coding Standards

## Naming

- Constants use `SNAKE_UPPER_CASE`.
- Private fields use camelCase with an `_` prefix. Add `[SerializeField]` when a field is editable in the Unity Editor.
- Production methods returning `IEnumerator` use an `IE_` prefix.
- Use `string.Empty` instead of `""`.

## Layout

Order class members as follows:

1. Events
2. Properties
3. Fields
4. Methods

## Unity safety

Never use null propagation (`?.`, `??`, or `??=`) on Unity objects such as `MonoBehaviour`, `ScriptableObject`, and `Component`.

## Test code

- Use `Assert.That` for every assertion.
- Name tests as plain-English sentences describing observable behavior, with underscores as spaces.
- Prefer arranging state inside each test. Fixture fields and Unity setup or teardown are acceptable when centralized lifecycle ownership is necessary for reliable cleanup.
- Name a local primary production object `sut` and a fixture-held primary production object `_sut`. Keep collaborators descriptively named. Interaction tests and journeys with no honest single subject are exempt.
- Separate obvious Arrange, Act, and Assert phases with blank lines. When any phase contains multiple operations, loops, or setup whose role is not immediately clear, include the complete `// Arrange`, `// Act`, and `// Assert` comment set.
- Use descriptive headers named for observable behavior only when a fixture has multiple meaningful behavior groups.
- Use parameterized scenario data and helpers only when they remove meaningful duplication from the visible test body. Do not introduce static helpers, scenario data, or abstractions merely for formatting uniformity.

## Code-smell heuristics

Use this Fowler code-smell baseline (_Refactoring_, chapter 3) as review heuristics. The documented standards above override these heuristics where they conflict. Report each smell as a judgment call rather than a hard violation. Skip anything tooling already enforces.

- **Mysterious Name** — A function, variable, or type whose name does not reveal what it does or holds. Rename it; if no honest name emerges, clarify the design.
- **Duplicated Code** — The same logic shape appears in more than one changed hunk or file. Extract the shared shape and call it from both.
- **Feature Envy** — A method reaches into another object's data more than its own. Move the method onto the data it envies.
- **Data Clumps** — The same fields or parameters repeatedly travel together. Bundle them into one type.
- **Primitive Obsession** — A primitive or string stands in for a domain concept that warrants its own type. Introduce a small domain type.
- **Repeated Switches** — The same `switch` or `if` cascade on the same type recurs across the change. Replace it with polymorphism or a shared map.
- **Shotgun Surgery** — One logical change forces scattered edits across many files. Gather what changes together into one module.
- **Divergent Change** — One file or module is edited for several unrelated reasons. Split it so each module changes for one reason.
- **Speculative Generality** — Abstraction, parameters, or hooks serve needs absent from the spec. Delete them and inline until a real need emerges.
- **Message Chains** — A caller depends on a long navigation chain such as `a.b().c().d()`. Hide the walk behind one method on the first object.
- **Middle Man** — A class or function mostly delegates onward. Remove it and call the real target directly.
- **Refused Bequest** — A subclass or implementer ignores or overrides most inherited behavior. Replace inheritance with composition.

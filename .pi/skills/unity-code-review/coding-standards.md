# Coding Standards

## Naming
- Constants are in `SNAKE_UPPER_CASE`.
- Private fields are in camelCase with prefix `_`, if they're meant to be editable in the editor make it a [SerializeField].
- IEnumerator methods should be named with a `IE_` prefix in production code.
- Use `string.Empty` instead of `""`.

## Layout
- Events.
- Properties.
- Fields.
- Methods.

## Unity Safety
- Never use null propagation (`?.`, `??`, `??=`) on Unity objects (`MonoBehaviour`, `ScriptableObject`, `Component`, etc.).

## Testing
- Use `Assert.That` for all test assertions.
- Name tests as a plain-English sentence describing the observable behavior use underscores as spaces.

## Code Smells

Use this Fowler code-smell baseline (_Refactoring_, ch. 3) as review heuristics. The documented standards above override these heuristics where they conflict. Report each smell as a judgement call, such as “possible Feature Envy,” rather than a hard violation. Skip anything tooling already enforces.

- **Mysterious Name** — A function, variable, or type whose name does not reveal what it does or holds. → Rename it; if no honest name emerges, clarify the design.
- **Duplicated Code** — The same logic shape appears in more than one changed hunk or file. → Extract the shared shape and call it from both.
- **Feature Envy** — A method reaches into another object's data more than its own. → Move the method onto the data it envies.
- **Data Clumps** — The same fields or parameters repeatedly travel together. → Bundle them into one type.
- **Primitive Obsession** — A primitive or string stands in for a domain concept that warrants its own type. → Introduce a small domain type.
- **Repeated Switches** — The same `switch` or `if` cascade on the same type recurs across the change. → Replace it with polymorphism or a shared map.
- **Shotgun Surgery** — One logical change forces scattered edits across many files. → Gather what changes together into one module.
- **Divergent Change** — One file or module is edited for several unrelated reasons. → Split it so each module changes for one reason.
- **Speculative Generality** — Abstraction, parameters, or hooks serve needs absent from the spec. → Delete them and inline until a real need emerges.
- **Message Chains** — A caller depends on a long navigation chain such as `a.b().c().d()`. → Hide the walk behind one method on the first object.
- **Middle Man** — A class or function mostly delegates onward. → Remove it and call the real target directly.
- **Refused Bequest** — A subclass or implementer ignores or overrides most inherited behavior. → Replace inheritance with composition.

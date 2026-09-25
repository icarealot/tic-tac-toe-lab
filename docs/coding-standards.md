# Coding Standards

Make the smallest sufficient change. Preserve required behavior, clarity, and domain integrity with the least maintenance surface.

## Production code

### Naming

- Use `SNAKE_UPPER_CASE` for constants.
- Use `_camelCase` for private fields.
- Prefix production methods returning `IEnumerator` with `IE_`.
- Use `string.Empty` instead of `""`.
- Use complete names that reveal what a type, member, or value does or holds. If no honest name emerges, clarify the design.

### Unity

- Never use null propagation or coalescing (`?.`, `??`, or `??=`) on Unity objects such as `MonoBehaviour`, `ScriptableObject`, and `Component`.
- Serialize button fields. Add `onClick` listeners in `Awake()`, remove them in `OnDestroy()`, and keep prefab `On Click ()` lists empty.
- Use TextMesh Pro's `SetText(...)` instead of assigning through `text`.

## Test code

- Name a local primary subject `sut` and a fixture-held subject `_sut`. Tests without one honest subject are exempt.
- Use `Assert.That` and name tests as observable behavior with underscores between words.
- Arrange state in each test and keep Arrange, Act, and Assert distinct.
- Use fixture setup and teardown only for shared lifecycle ownership and reliable cleanup.
- Parameterize cases or extract helpers only to remove meaningful duplication.

## Do

- Prefer, in order: delete obsolete behavior, reuse an existing capability, change configuration, simplify the design, then add code.
- Require every addition and abstraction to serve a current requirement, invariant, or concrete risk.
- Optimize for understandable code rather than raw line count.
- Declare every concrete class `sealed` unless it is intentionally designed for inheritance.
- Keep behavior near the data it uses and expose purposeful methods or read-only observations.
- Introduce domain types or composition when they clarify real concepts and responsibilities.

## Don’t

- Don’t add speculative abstractions or task-created scaffolding.
- Don’t leave duplicated logic, dead paths, long navigation chains, or layers that only delegate.
- Don’t expose mutable state when a purposeful behavior or read-only observation is sufficient.
- Don’t use inheritance when a subtype cannot honor most inherited behavior.

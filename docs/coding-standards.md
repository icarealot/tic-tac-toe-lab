# Coding Standards

## Production code

### Naming

- Constants use `SNAKE_UPPER_CASE`.
- Private fields use `_camelCase`.
- Production methods returning `IEnumerator` use an `IE_` prefix.
- Use `string.Empty` instead of `""`.
- Names should reveal what a function, variable, or type does or holds. Do not abbreviate names. Flag mysterious names during review and rename them; if no honest name emerges, clarify the design.

### Member order

Order class members as follows:

1. Events
2. Properties
3. Fields
4. Methods

### Unity

- Never use null propagation or coalescing (`?.`, `??`, or `??=`) on Unity objects such as `MonoBehaviour`, `ScriptableObject`, and `Component`.
- Serialize button fields and wire `onClick` with `AddListener` in `Awake()` and `RemoveListener` in `OnDestroy()`. Keep prefab `On Click ()` lists empty.
- Use TextMesh Pro's `SetText(...)` method instead of assigning through the `text` property.

## Test code

- Use `Assert.That` and name tests in plain English as observable behavior, with underscores separating words.
- Arrange state in each test and keep Arrange, Act, and Assert distinct. Use fixture setup or teardown only for centralized lifecycle ownership and reliable cleanup.
- Parameterize scenarios or extract helpers only to remove meaningful duplication, not for formatting uniformity.
- Verify behavior through the narrowest public seam where a caller observes the result. If none exists, add a purposeful method or read-only observation that represents a real caller-visible contract. Do not add mutable getters or implementation-only APIs solely to expose test state.

## Design rules

- **Model concepts once.** Remove duplicated logic, group values that travel together, and introduce domain types when primitives obscure a meaningful concept.
- **Keep behavior with its owner.** Put behavior near the data it uses, keep one member-access step per line, hide long navigation chains, and remove layers that merely delegate.
- **Localize change.** Centralize recurring type-based branches, keep code that changes together, and split modules that change for unrelated reasons.
- **Earn abstractions.** Keep only abstractions required by current behavior. Use composition when a subtype cannot honor most inherited behavior or when a class has many instance variables.
- Keep methods and classes small and focused. Keep each method to one level of indentation. Use guard clauses or polymorphism when they make control flow clearer.
- Wrap primitives and strings when they represent meaningful domain concepts, but do not wrap every Unity value type mechanically. Give collections with rules a domain owner.
- Use behavior rather than mutable getters and setters. Use private serialized fields for Inspector configuration and expose read-only properties or purposeful methods when callers need access.

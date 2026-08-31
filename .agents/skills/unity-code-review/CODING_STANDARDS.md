# Coding Standards

## Naming
- Constants are in `SNAKE_UPPER_CASE`.
- Private fields are in camelCase with prefix `_`, if they're meant to be editable in the editor make it a [SerializeField].
- IEnumerator methods should be named with a `IE_` prefix.
- Use `string.Empty` instead of `""`.

## Layout
- Place properties at above fields.

## Unity Safety
- Never use null propagation (`?.`, `??`, `??=`) on Unity objects (`MonoBehaviour`, `ScriptableObject`, `Component`, etc.).

## Testing
- Use `Assert.That` for all test assertions.
- Name tests as a plain-English sentence describing the observable behavior use underscores as spaces.

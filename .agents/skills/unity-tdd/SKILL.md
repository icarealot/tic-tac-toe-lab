---
name: unity-tdd
description: Test-drive Unity features, fixes, or integration tests with a red-green loop.
---

Test-drive one behavior slice at a time through its public seam.

Use project coding and testing standards under `docs/` when present. Use `CONTEXT.md` domain language and respect applicable ADRs.

For each behavior selected by the testing standard:

1. **Red** — Write one standards-compliant test. Run it; require discovery and failure for the intended behavioral reason.
2. **Green** — Write only enough standards-compliant production code to pass it. Run the focused test; require discovery and success.
3. Start the next behavior only after green.

Work vertically. Do not bulk-write tests, anticipate later cases, or add speculative behavior. End each loop at green.

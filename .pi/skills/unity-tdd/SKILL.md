---
name: unity-tdd
description: Test-drive Unity features or fixes when the user requests test-first work, red–green, or integration tests.
---

Run a red → green loop for one behavior slice at a time.

## Standards preflight

- Discover the project standards if not already known.
- Read `CONTEXT.md` when present so names and interfaces use the project's domain language, and respect applicable ADRs.

## Loop

Work vertically. Bulk tests written ahead of their implementation describe imagined structure rather than proven behavior.

For each behavior selected under the discovered testing standard:

1. **Red** — Write one test through the selected public seam, following both standards. Run it, confirm it was discovered, and confirm it fails for the intended behavioral reason.
2. **Green** — Write only enough production code to satisfy that test, following the coding standard. Run the focused test again and confirm it passes.
3. Continue with the next behavior only after the current slice is green.

Do not anticipate later tests or add speculative production behavior. End the loop after green; handle refactoring as follow-up work informed by `/unity-code-review`.

## Report

Report each completed red–green cycle and its result.

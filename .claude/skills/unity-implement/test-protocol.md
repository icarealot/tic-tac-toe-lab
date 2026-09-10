# Unity test protocol

Apply this protocol to every focused, task-level, and complete EditMode or PlayMode test run.

1. When `unity status` reports a connected Editor, use its Pipeline commands to avoid contesting the project lock. Start each mode asynchronously:
   - EditMode: `unity command run_tests --mode editor --async_tests true --project-path . --format json`
   - PlayMode: `unity command run_tests --mode playmode --async_tests true --project-path . --format json`

   Add the appropriate `run_tests` filter for a focused run; omit it for the complete mode.

2. Poll `unity command test_status --project-path . --format json` with a fixed delay and hard attempt limit. Save each response and decode the JSON string in `data.result` once; do not grep escaped fields from the outer envelope. `data.result.status` is authoritative and terminal only at `completed` or `failed`. Never infer completion from elapsed time, a fixed sleep, the initial `run_tests` response, or the outer CLI status.

3. Run modes sequentially. Wait for EditMode to terminate before starting PlayMode. Do not request asynchronous `all`: entering Play Mode causes a domain reload.

4. At each terminal state, immediately record `summary.total`, `passed`, `failed`, `skipped`, and `inconclusive`, plus details for every non-passed test. Inspect both the nested command result and outer CLI envelope; outer `success` does not override a nested test error. Report the summary without waiting for or printing the complete per-test payload.

5. A run passes only when every requested mode terminates without test failures, test-run errors, or compile errors.

6. Attribute every failure before changing code:
   - Compare it with the baseline; a failure is pre-existing when it did not pass there.
   - Rerun the failing suite once to distinguish a one-off flake from a regression.
   - If the changed behavior cannot affect the failing test, compare sibling tests sharing its setup in the same run before attributing it.

Report every flake and pre-existing failure; never absorb one silently.

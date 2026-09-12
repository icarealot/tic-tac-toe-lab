# Unity test protocol

Use [`run_tests.py`](run_tests.py) for every Unity tests run against the connected Editor.

## Run

Run one mode at a time:

```bash
# Focused TDD run after a coherent C# change
python run_tests.py --project-path ../../.. --mode editor --filter BoardPresenterTests

# Complete final runs when both modes are in the validation tier
python run_tests.py --project-path ../../.. --mode editor
python run_tests.py --project-path ../../.. --mode playmode
```

`--filter-type` accepts `testName` (default), `assembly`, or `category`. The filter is a case-insensitive partial match. Pass `--include-explicit` only when the task intentionally validates explicit tests.

The helper:

1. Calls `UnityEditor.AssetDatabase.Refresh()` and waits until the Editor finishes updating and compiling.
2. For a focused run, calls `list_tests` and exits before testing unless the filter matches at least one discovered test.
3. Starts `run_tests` asynchronously and polls `test_status` through connection interruptions and domain reloads.
4. Decodes the nested result once and prints one compact JSON document containing the terminal status, summary, and every non-passed test.

A run passes only when it reaches `completed`, discovers at least one test, and reports no failed or inconclusive tests. Skipped tests are reported but do not fail the run. Exit code `0` means passed, `1` means a completed run did not pass, and `2` means compilation, discovery, connection, or helper failure.

When both modes are required, run EditMode and PlayMode sequentially; never request asynchronous `all` because entering Play Mode causes a domain reload.

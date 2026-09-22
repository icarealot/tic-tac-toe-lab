# Unity test protocol

Run Unity tests against the connected Editor only through [`run_tests.py`](run_tests.py), one mode at a time.

```bash
# Focused EditMode
python run_tests.py --project-path ../../.. --mode editor --filter BoardPresenterTests

# Focused PlayMode
python run_tests.py --project-path ../../.. --mode playmode --filter CriticalJourneyTests

# Complete suites
python run_tests.py --project-path ../../.. --mode editor
python run_tests.py --project-path ../../.. --mode playmode
```

`--filter-type` accepts `testName` (default), `assembly`, or `category`; filters are case-insensitive partial matches. Use `--include-explicit` only when intentionally validating explicit tests.

The helper:

1. Clears the console, calls `AssetDatabase.Refresh`, and waits for updating and compilation.
2. Calls `RequestScriptCompilation` when Unity's sticky `scriptCompilationFailed` flag is set, then rejects console compiler errors. The post-clear console is authoritative, not `recompile_status`.
3. Before a focused run, calls `list_tests` and rejects a filter matching none.
4. Calls `run_tests` asynchronously and polls `test_status` through connection interruptions and domain reloads.
5. Emits compact JSON with terminal status, summary, and every non-passed test.

A run passes only when status is `completed`, at least one test was discovered, and none failed or were inconclusive. Skips are reported but allowed. Exit codes: `0` passed, `1` completed but not passed, `2` compilation/discovery/connection/helper failure.

When both modes are required, run EditMode then PlayMode; asynchronous `all` is invalid because Play Mode reloads the domain. After changing tests, confirm from discovery counts and result names that those tests ran.

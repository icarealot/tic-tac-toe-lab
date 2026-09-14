# Unity test protocol

Use [`run_tests.py`](run_tests.py) for every Unity tests run against the connected Editor.

## Run

Run one mode at a time.

```bash
# Focused EditMode run (class lives in EditModeTests)
python run_tests.py --project-path ../../.. --mode editor --filter BoardPresenterTests

# Focused PlayMode run (class lives in PlayModeTests)
python run_tests.py --project-path ../../.. --mode playmode --filter CriticalJourneyTests

# Complete final runs when both modes are in the validation tier
python run_tests.py --project-path ../../.. --mode editor
python run_tests.py --project-path ../../.. --mode playmode
```

`--filter-type` accepts `testName` (default), `assembly`, or `category`. The filter is a case-insensitive partial match. Pass `--include-explicit` only when the task intentionally validates explicit tests.

The helper:

1. Clears the Editor console, calls `UnityEditor.AssetDatabase.Refresh()`, and waits until the Editor finishes updating and compiling. This clear is the baseline: every console error observed afterwards belongs to this run.
2. Checks Unity's sticky `scriptCompilationFailed` flag; when the last compile failed it forces `RequestScriptCompilation` — the pipeline never retries an attempted-but-failed compile on its own — and waits it out. Then it rejects the run when the console holds compiler errors: the console was cleared at step 1, so every error there belongs to this run, and the console, not `recompile_status`, is the authority on compile success.
3. For a focused run, calls `list_tests` and exits before testing unless the filter matches at least one discovered test.
4. Starts `run_tests` asynchronously and polls `test_status` through connection interruptions and domain reloads.
5. Decodes the nested result once and prints one compact JSON document containing the terminal status, summary, and every non-passed test.

A run passes only when it reaches `completed`, discovers at least one test, and reports no failed or inconclusive tests. Skipped tests are reported but do not fail the run. Exit code `0` means passed, `1` means a completed run did not pass, and `2` means compilation, discovery, connection, or helper failure.

When both modes are required, run EditMode and PlayMode sequentially; never request asynchronous `all` because entering Play Mode causes a domain reload.

After editing test code, confirm from the discovery count and result names that the new or changed tests actually ran; a run that passes without exercising them proves nothing.

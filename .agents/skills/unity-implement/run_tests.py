#!/usr/bin/env python3
"""Run one Unity Pipeline test mode with bounded discovery and polling."""

from __future__ import annotations

import argparse
import json
import re
import shutil
import subprocess
import sys
import time
from pathlib import Path
from typing import Any, Iterable


class RunnerError(RuntimeError):
    pass


def decode_json_string(value: Any) -> Any:
    if not isinstance(value, str):
        return value
    try:
        return json.loads(value)
    except json.JSONDecodeError:
        return value


def error_text(document: dict[str, Any]) -> str:
    messages = []
    for error in document.get("errors") or []:
        if isinstance(error, dict):
            messages.append(str(error.get("message") or error.get("code") or error))
        else:
            messages.append(str(error))
    return "; ".join(messages) or "Unity command failed"


class UnityPipeline:
    def __init__(self, project_path: Path, command_timeout: int = 20) -> None:
        if not project_path.is_dir():
            raise RunnerError(f"project path does not exist: {project_path}")
        executable = shutil.which("unity")
        if executable is None:
            raise RunnerError("unity CLI is not on PATH")
        self.executable = executable
        self.project_path = project_path
        self.command_timeout = command_timeout

    def command(self, name: str, *parameters: str) -> dict[str, Any]:
        command = [
            self.executable,
            "--format",
            "json",
            "--no-banner",
            "command",
            "--project-path",
            str(self.project_path),
            "--timeout",
            str(self.command_timeout),
            name,
            *parameters,
        ]
        try:
            completed = subprocess.run(
                command,
                cwd=self.project_path,
                capture_output=True,
                text=True,
                timeout=self.command_timeout + 5,
                check=False,
            )
        except subprocess.TimeoutExpired as error:
            raise RunnerError(f"unity command {name} timed out") from error

        try:
            document = json.loads(completed.stdout)
        except json.JSONDecodeError as error:
            detail = completed.stderr.strip() or completed.stdout.strip() or "no output"
            raise RunnerError(f"unity command {name} returned invalid JSON: {detail}") from error

        if completed.returncode != 0 or not document.get("success", False):
            raise RunnerError(error_text(document))
        data = document.get("data")
        if not isinstance(data, dict) or data.get("success") is False:
            raise RunnerError("Unity Pipeline returned an unsuccessful command result")
        return data


def result_from(data: dict[str, Any]) -> Any:
    return decode_json_string(data.get("result"))


def poll_command(
    pipeline: UnityPipeline,
    name: str,
    deadline: float,
    poll_interval: float,
) -> Any:
    last_error: RunnerError | None = None
    while time.monotonic() < deadline:
        try:
            return result_from(pipeline.command(name))
        except RunnerError as error:
            last_error = error
            time.sleep(poll_interval)
    if last_error is not None:
        raise RunnerError(f"{name} remained unavailable: {last_error}")
    raise RunnerError(f"timed out polling {name}")


def wait_for_editor_idle(
    pipeline: UnityPipeline,
    deadline: float,
    poll_interval: float,
    operation: str,
) -> None:
    idle_pattern = re.compile(r"compiling=False\s+updating=False", re.IGNORECASE)
    while time.monotonic() < deadline:
        try:
            evaluation = result_from(
                pipeline.command(
                    "eval",
                    'return "compiling=" + UnityEditor.EditorApplication.isCompiling + '
                    ' " updating=" + UnityEditor.EditorApplication.isUpdating;',
                )
            )
            value = evaluation.get("result") if isinstance(evaluation, dict) else evaluation
            if isinstance(value, str) and idle_pattern.search(value):
                return
        except RunnerError:
            pass
        time.sleep(poll_interval)
    raise RunnerError(f"Editor did not become idle after {operation}")


def refresh_assets(
    pipeline: UnityPipeline,
    timeout: int,
    poll_interval: float,
) -> None:
    print("assets: refresh requested", file=sys.stderr)
    deadline = time.monotonic() + timeout
    last_error: RunnerError | None = None

    while time.monotonic() < deadline:
        try:
            pipeline.command(
                "eval",
                "UnityEditor.AssetDatabase.Refresh(); return true;",
            )
            break
        except RunnerError as error:
            # Refresh can trigger a domain reload that temporarily drops the connection.
            last_error = error
            time.sleep(poll_interval)
    else:
        detail = f": {last_error}" if last_error is not None else ""
        raise RunnerError(f"asset refresh did not finish within {timeout}s{detail}")

    wait_for_editor_idle(pipeline, deadline, poll_interval, "asset refresh")
    print("assets: refresh completed", file=sys.stderr)


def listed_tests(pipeline: UnityPipeline, mode: str) -> list[dict[str, Any]]:
    result = result_from(pipeline.command("list_tests", "--mode", mode))
    if not isinstance(result, dict) or result.get("success") is False:
        raise RunnerError(f"unexpected list_tests result: {result!r}")
    tests = result.get("Tests")
    if not isinstance(tests, list):
        raise RunnerError("list_tests did not return a Tests array")
    return [test for test in tests if isinstance(test, dict)]


def matching_tests(
    tests: Iterable[dict[str, Any]],
    pattern: str,
    filter_type: str,
    include_explicit: bool,
) -> list[dict[str, Any]]:
    needle = pattern.casefold()
    matches = []
    for test in tests:
        if test.get("Explicit") and not include_explicit:
            continue
        if filter_type == "testName":
            values = [test.get("FullName", "")]
        elif filter_type == "assembly":
            values = [test.get("Assembly", "")]
        else:
            values = test.get("Categories") or []
        if any(needle in str(value).casefold() for value in values):
            matches.append(test)
    return matches


def normalize_summary(value: Any) -> dict[str, int]:
    source = value if isinstance(value, dict) else {}
    summary = {}
    for key in ("total", "passed", "failed", "skipped", "inconclusive"):
        raw = source.get(key, source.get(key.capitalize(), 0))
        summary[key] = int(raw or 0)
    return summary


def non_passed_results(value: Any) -> list[dict[str, Any]]:
    if not isinstance(value, list):
        return []
    non_passed = []
    for result in value:
        if not isinstance(result, dict):
            continue
        status = str(result.get("Status", result.get("status", "unknown")))
        if status.casefold() == "passed":
            continue
        non_passed.append(
            {
                "name": result.get("FullName", result.get("fullName", "unknown")),
                "status": status,
                "message": result.get("Message", result.get("message")),
                "stackTrace": result.get("StackTrace", result.get("stackTrace")),
            }
        )
    return non_passed


def run_tests(
    pipeline: UnityPipeline,
    mode: str,
    test_filter: str | None,
    filter_type: str,
    include_explicit: bool,
    timeout: int,
    poll_interval: float,
) -> tuple[dict[str, Any], bool]:
    parameters = ["--mode", mode, "--async_tests", "true"]
    if test_filter is not None:
        parameters.extend(["--filter", test_filter, "--filter_type", filter_type])
    if include_explicit:
        parameters.extend(["--include_explicit", "true"])

    started = result_from(pipeline.command("run_tests", *parameters))
    if not isinstance(started, dict) or started.get("success") is False:
        raise RunnerError(f"tests did not start: {started!r}")

    deadline = time.monotonic() + timeout
    while time.monotonic() < deadline:
        status = poll_command(pipeline, "test_status", deadline, poll_interval)
        if not isinstance(status, dict):
            raise RunnerError(f"unexpected test status: {status!r}")
        state = str(status.get("status", "")).lower()
        if state not in {"completed", "failed"}:
            time.sleep(poll_interval)
            continue

        summary = normalize_summary(status.get("summary", status.get("Summary")))
        report: dict[str, Any] = {
            "mode": mode,
            "filter": test_filter,
            "filterType": filter_type if test_filter is not None else None,
            "status": state,
            "duration": status.get("duration", status.get("Duration")),
            "summary": summary,
            "nonPassed": non_passed_results(status.get("results", status.get("Results"))),
        }
        message = status.get("message") or status.get("error")
        if message:
            report["message"] = message
        passed = (
            state == "completed"
            and summary["total"] > 0
            and summary["failed"] == 0
            and summary["inconclusive"] == 0
        )
        return report, passed
    raise RunnerError(f"tests did not finish within {timeout}s")


def positive_int(value: str) -> int:
    number = int(value)
    if number <= 0:
        raise argparse.ArgumentTypeError("must be greater than zero")
    return number


def positive_float(value: str) -> float:
    number = float(value)
    if number <= 0:
        raise argparse.ArgumentTypeError("must be greater than zero")
    return number


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--mode", required=True, choices=("editor", "playmode"))
    parser.add_argument("--filter", dest="test_filter")
    parser.add_argument(
        "--filter-type",
        choices=("testName", "assembly", "category"),
        default="testName",
    )
    parser.add_argument("--include-explicit", action="store_true")
    parser.add_argument("--project-path", type=Path, default=Path.cwd())
    parser.add_argument("--timeout", type=positive_int, default=330)
    parser.add_argument("--refresh-timeout", type=positive_int, default=180)
    parser.add_argument("--poll-interval", type=positive_float, default=1.0)
    return parser


def main() -> int:
    arguments = build_parser().parse_args()
    if arguments.test_filter is not None and not arguments.test_filter.strip():
        print(json.dumps({"status": "error", "message": "filter cannot be empty"}, indent=2))
        return 2

    try:
        pipeline = UnityPipeline(arguments.project_path.resolve())
        refresh_assets(pipeline, arguments.refresh_timeout, arguments.poll_interval)

        if arguments.test_filter is not None:
            matches = matching_tests(
                listed_tests(pipeline, arguments.mode),
                arguments.test_filter,
                arguments.filter_type,
                arguments.include_explicit,
            )
            print(
                f"discovery: {len(matches)} test(s) match "
                f"{arguments.filter_type}={arguments.test_filter!r}",
                file=sys.stderr,
            )
            if not matches:
                raise RunnerError("focused filter matched zero discovered tests")

        report, passed = run_tests(
            pipeline,
            arguments.mode,
            arguments.test_filter,
            arguments.filter_type,
            arguments.include_explicit,
            arguments.timeout,
            arguments.poll_interval,
        )
        print(json.dumps(report, indent=2))
        return 0 if passed else 1
    except RunnerError as error:
        print(json.dumps({"status": "error", "message": str(error)}, indent=2))
        return 2


if __name__ == "__main__":
    raise SystemExit(main())

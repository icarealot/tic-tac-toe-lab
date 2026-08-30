---
name: unity-implement
description: Implement an approved Unity task end to end.
disable-model-invocation: true
---

Implement the work described by the user in the spec or tickets.

## 1. Boundary

- Confirm there is an opened editor by running `unity pipeline list`. If there is no opened editor, use `unity open --args "-automated"` to open one.
- Always use `/unity-cli` to recompile after editing C# script and test files (`.cs`).
- Always use `/unity-cli` to edit any non-code file (e.g. `.unity` scenes, `.prefab`, `.asset` ScriptableObjects, `.meta` files, `ProjectSettings/*`) or any `.asmdef` file. DO NOT edit YAML file directly.

## 2. Implement one task

- Use `/unity-tdd` where possible, at pre-agreed seams.
- Run typechecking regularly, single test files regularly, and the full test suite once at the end by using `/unity-cli`.
- Never trigger a Player build yourself.

## 3. Report

Report to the user: Changed files, checks run, results, and any validation that remains unavailable.

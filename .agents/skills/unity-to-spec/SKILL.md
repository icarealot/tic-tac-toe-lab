---
name: unity-to-spec
description: Turn the current conversation into a spec. No interview, just synthesis of what you've already discussed.
disable-model-invocation: true
---

This skill takes the current conversation context and codebase understanding and produces a spec. Do NOT interview the user; just synthesize what you already know

## Process

1. Explore the repo to understand the current state of the codebase if you haven't already. Use the project's domain glossary vocabulary throughout the spec, and respect any ADRs in the area you're touching.

2. Map validation to risk. Every proposed automated test must protect a named game rule, calculation, meaningful branch, state change, lifecycle, infrastructure contract, or critical player journey. Do not propose tests for trivial forwarding or construction, incidental hierarchy, diagnostic text, styling, or the same outcome repeated at every layer.

Choose the **smallest sufficient fixture** for each behavior:

1. EditMode for deterministic behavior without scenes, assets, frames, or Unity object lifecycle.
2. An isolated PlayMode `GameObject` for component, lifecycle, physics, or focused engine integration.
3. A production prefab only when its serialized configuration is part of the behavior.
4. A production scene only when bootstrap or cross-object production wiring is the behavior.

Assert each rule primarily at its owning layer. Prefer an existing seam that proves the behavior; propose a new public seam only when none does. Use player-build checks for platform-sensitive behavior and human playtests for feel, visuals, audio, controls, camera behavior, and usability. Extend an existing critical journey when readable instead of proposing another production-scene load.

Make these decisions from the agreed conversation and codebase evidence. Record unresolved validation assumptions in the spec rather than opening a new interview.

3. Write the spec using the template below and save it to `docs/<feature-name>/SPEC.md`.

<spec-template>

## Problem Statement

The problem that the user is facing, from the user's perspective.

## Solution

The solution to the problem, from the user's perspective.

## User Stories

A LONG, numbered list of user stories. Each user story should be in the format of:

1. As an <actor>, I want a <feature>, so that <benefit>

<user-story-example>
1. As a player, I want an aiming indicator before releasing an ability, so that I can judge its direction and range
</user-story-example>

This list of user stories should be extremely extensive and cover all aspects of the feature.

## Implementation Decisions

A list of implementation decisions that were made. This can include:

- The modules that will be built/modified
- The interfaces of those modules that will be modified
- Technical clarifications from the developer
- Architectural decisions
- Schema changes
- API contracts
- Specific interactions

Do NOT include specific file paths or code snippets. They may end up being outdated very quickly.

## Testing Decisions

A list of testing decisions that were made. Include:

- The named behavior or risk protected by each proposed automated check
- The owning public seam and why the selected fixture is the smallest one that can prove it
- Which checks belong in EditMode, isolated PlayMode, a production prefab, a production scene, a player build, or a human playtest
- Which changed behavior intentionally has no dedicated automated test and why
- Prior art for the tests (i.e. similar types of tests in the codebase)

Tests observe public behavior and use independent expected values rather than mirroring implementation calculations.

## Out of Scope

A description of the things that are out of scope for this spec.

## Further Notes

Any further notes about the feature.

</spec-template>

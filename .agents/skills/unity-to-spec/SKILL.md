---
name: unity-to-spec
description: Turn the current conversation into a spec. No feature interview, just synthesis of what has already been discussed.
disable-model-invocation: true
---

Turn the current conversation and codebase evidence into a spec. Do not interview the user; synthesize what is already known.

## Process

1. Explore the repository, project standards in `docs/` if needed.

2. Use the project's domain glossary vocabulary, apply project standards throughout the spec and respect applicable ADRs.

3. Derive the feature's validation decisions from the discovered standards and the agreed conversation. Record unresolved validation assumptions in the spec rather than opening a feature interview.

4. Write the spec to `docs/<feature-name>/SPEC.md` using this template:

<spec-template>

## Problem Statement

The problem that the user is facing, from the user's perspective.

## Solution

The solution to the problem, from the user's perspective.

## User Stories

A long numbered list in this format:

1. As an <actor>, I want a <feature>, so that <benefit>

Cover every agreed aspect of the feature.

## Implementation Decisions

List agreed implementation decisions, including relevant modules, interfaces, technical clarifications, architecture, schemas, contracts, and interactions.

Do not include file paths or code snippets; they become stale quickly.

## Testing Decisions

Record the feature-specific validation decisions derived from the project standards, including the rationale for each selected check or deliberate omission and relevant codebase prior art.

## Out of Scope

Describe what this spec excludes.

## Further Notes

Record other relevant feature notes.

</spec-template>

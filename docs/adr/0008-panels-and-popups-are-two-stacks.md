# Panels and popups use separate stacks, and panels cannot be shown over popups

## Status

Accepted

## Context and Problem Statement

Panels cover the display, while popups float above panels. Both participate in visibility and back navigation. What structure preserves their ordering, and what should happen when a caller requests a panel while a popup is present?

## Decision Drivers

- Every popup must render above every panel.
- Structural invariants are safer than call-order conventions.
- Independent gameplay and navigation events can race.
- States, rather than UIService, own navigation meaning under ADR 0007.
- Requests that cannot be honored coherently should fail loudly.

## Considered Options

- Separate stacks; reject showing a panel while a popup is present
- Separate stacks; silently ignore the panel request
- Separate stacks; automatically close popups first
- One mixed window stack

## Decision Outcome

Chosen option: “separate stacks; reject showing a panel while a popup is present,” because separate hierarchy layers make visual ordering structural and rejection exposes missing state cleanup immediately.

UIService keeps one stack for panels and one for popups. Their hierarchy layers guarantee that every popup renders above every panel. Within each stack, only the top window is visible. A popup does not hide the panel beneath it.

Showing a panel while any popup remains is considered an invalid request and raises an error. States can inspect whether a popup exists and close their popups before transitioning. UIService does not guess by closing windows automatically.

Back considers the popup stack before the panel stack, but states decide whether back should invoke either mechanism.

A covered window remains alive and retains its configuration. Removing the top window returns it and reveals the previous window in that stack.

### Positive Consequences

- Popup-above-panel ordering is guaranteed by hierarchy and type distinctions.
- The topmost interactive window is unambiguous.
- Hidden windows preserve their configuration.
- Missing transition cleanup fails visibly rather than producing a wedged interface.
- Stack rules remain independently testable.

### Negative Consequences

- A player-triggered race can surface as a runtime error if a state forgets cleanup.
- States must deliberately clear popups before incompatible transitions.
- Two stacks and two layers increase conceptual and runtime structure.
- Hidden windows continue to exist and incur some runtime cost.

## Pros and Cons of the Options

### Separate stacks with rejection

- Good, because visual ordering is structural.
- Good, because invalid state transitions fail loudly.
- Good, because panels and popups remain distinct roles.
- Bad, because missing cleanup becomes a runtime failure.

### Separate stacks with a silent refusal

- Good, because no exception reaches the player.
- Bad, because the app can enter a new state while displaying stale user interface.
- Bad, because the failure has no visible explanation.

### Separate stacks with automatic popup closure

- Good, because transitions cannot fail for leftover popups.
- Bad, because UIService guesses navigation intent.
- Bad, because a meaningful player decision can disappear without resolution.

### One mixed stack

- Good, because topmost-window and back behavior are simple.
- Bad, because a panel can appear above a popup.
- Bad, because changing the panel beneath a popup cannot be represented cleanly.
- Bad, because ordering depends on service correctness rather than structure.

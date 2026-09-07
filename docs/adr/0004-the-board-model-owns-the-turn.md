# The board model owns the turn and advances it on placement

## Status

Superseded by [ADR 0006](./0006-a-game-is-reset-rather-than-recreated.md).

## Context and Problem Statement

Marks must alternate, and the turn must advance only when a mark is actually placed. Which part of the board owns this fact, and should advancement be a separate responsibility?

## Decision Drivers

- Invalid or rejected presses must not pass the turn.
- Rules that depend on callers performing several steps in order are fragile.
- Alternation should be testable without presentation collaborators.
- The turn is game state, though presenters remain responsible for deciding whether a press is legal.
- Future app phases need a stable place from which to read and reset the turn.

## Considered Options

- The board model owns the turn and advances it with placement
- The board presenter owns and advances the turn
- A separate turn model owns it

## Decision Outcome

Chosen option: “the board model owns the turn and advances it with placement,” because it binds the mark placed and the turn advancement into one domain operation.

The board starts with X’s turn. A legal placement uses the current turn and advances it. The presenter decides whether a press is legal and only then asks the model to place, so rejected presses cannot advance the turn.

Empty cells represent the absence of a mark. Empty is not a possible turn.

### Positive Consequences

- Placement cannot disagree with turn advancement.
- Alternation has direct, scene-free tests.
- The turn is readable by user-interface and future opponent features.
- Illegal empty-turn state is absent.

### Negative Consequences

- Presentation must capture the placed mark before the model advances the turn.
- The placement operation can appear incomplete because callers do not name a mark.
- The board model combines several aspects of board state and behavior.
- Preview or secondary boards carry turn state whether they need it or not.

## Pros and Cons of the Options

### The board model owns the turn

- Good, because placement and advancement cannot be separated.
- Good, because callers cannot choose a mark inconsistent with the turn.
- Bad, because the board model becomes broader.

### The board presenter owns the turn

- Good, because rule enforcement stays in presentation coordination.
- Bad, because a later edit can advance on a rejected press or omit advancement.
- Bad, because other features cannot naturally read the turn.

### A separate turn model

- Good, because turn state has a focused home.
- Bad, because callers must coordinate two models correctly.
- Bad, because one enum value adds another lifecycle dependency.

# The board model owns the turn and advances it on placement

## Status

Superseded by [ADR 0004](./0004-the-board-model-owns-the-turn.md).

## Context and Problem Statement

Marks must alternate. Something must own whose turn it is, and advancement must happen exactly when a mark is placed. Should that rule belong to the board model, the presenter or a separate turn model?

## Decision Drivers

- Rejected presses must not advance the turn.
- Placement and advancement should not be separable operations that callers can order incorrectly.
- Alternation should be testable directly against domain state.
- The turn is a fact about the game rather than its presentation.
- Assigning rules to the model is a deliberate qualification of the project’s model-presenter boundary.

## Considered Options

- The board model owns the turn and advances it with placement
- The board presenter owns and advances the turn
- A separate turn model owns it

## Decision Outcome

Chosen option: “the board model owns the turn and advances it with placement,” because it makes placing a mark without advancing the turn inexpressible through the model’s normal operation.

The turn starts as X. A successful placement uses the current turn and then advances it. The presenter remains responsible for deciding whether a press is allowed to become a placement, so rejected presses never reach the operation that advances the turn.

Empty is represented by the absence of a mark, not as a third mark. The turn can therefore always be X or O.

### Positive Consequences

- Callers cannot place one mark while advancing another.
- Alternation is testable as model behavior.
- Rejected presses leave the turn untouched by construction.
- Turn state is available to future game features.

### Negative Consequences

- The presenter must remember which mark was current before placement if it needs to display that mark afterward.
- Placement may look unusual because the caller does not supply a mark.
- The board model owns geometry, marks and turn state rather than one narrow concern.
- A second board also carries its own turn, even if used only as a preview.

## Pros and Cons of the Options

### The board model owns the turn

- Good, because placement and advancement are indivisible.
- Good, because alternation is directly testable.
- Bad, because the model’s responsibility grows.

### The board presenter owns the turn

- Good, because game rules remain with the presenter.
- Bad, because placement and advancement become separate steps.
- Bad, because turn state is difficult for other features to observe.

### A separate turn model

- Good, because each model remains narrow.
- Good, because the turn is independently observable.
- Bad, because placement and advancement cross object boundaries and can drift apart.

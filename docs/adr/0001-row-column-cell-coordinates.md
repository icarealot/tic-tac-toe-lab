# Cells are addressed as (row, column) with row growing downwards

## Status

Accepted

## Context and Problem Statement

Every cell on the board needs an address, and that address appears in GameObject names, in position arithmetic, and in the signature of any future lookup API. Unity's world Y axis grows *upwards*, but people read a tic-tac-toe board top-to-bottom, so the top-left cell is naturally "the first one". Which convention should the project commit to, and in which order are the two numbers written?

## Decision Drivers

- The board is read and discussed by humans top-to-bottom, left-to-right.
- Cell addresses appear in the Unity Hierarchy, where they must be scannable against what is visible in the Scene view.
- Position arithmetic must be readable by someone who did not write it, in a codebase where the Y axis grows the other way.
- The convention is referenced by every future feature that touches a cell (input, marks, win detection), so churn is expensive.

## Considered Options

- `(row, column)` with row growing downwards from the top
- `(x, y)` with y growing upwards, matching Unity world axes
- A single flat index `0..8`

## Decision Outcome

Chosen option: "`(row, column)` with row growing downwards", because it matches how a person reads a tic-tac-toe board aloud, which is how the team already talks about it. Cell `(0, 0)` is top-left and `(2, 2)` is bottom-right.

The consequence is that a row number and a world Y coordinate move in opposite directions. The project absorbs this in one place — the board's layout arithmetic negates the row term:

```
localPosition = origin + (column * step, -row * step, 0)
```

To keep that inversion legible, the identifiers in code are named `row` and `column`, never `x` and `y`. A reader who sees `-row * step` in the Y term can tell at a glance that the negation is deliberate; a reader who saw `-x * step` in a Y term would reasonably suspect a bug.

### Positive Consequences

- Spoken vocabulary, Hierarchy names, and code identifiers all agree.
- Reading the Hierarchy top-to-bottom matches reading the board top-to-bottom.
- Future APIs take `(int row, int column)`, an order nobody has to look up.
- The axis inversion is confined to a single expression rather than being spread across the codebase.

### Negative Consequences

- Row-to-world-Y is inverted, which will surprise anyone who assumes the first coordinate is horizontal.
- The convention disagrees with Unity's own `Vector2`/`Vector3` ordering, so converting between a cell coordinate and a world position always involves a swap as well as a negation.
- Reversing the decision later means touching every cell name, every position expression, and every lookup call site.

## Pros and Cons of the Options

### `(row, column)` with row growing downwards

The convention used by matrices, spreadsheets, and most board-game notation.

- Good, because it matches how people read and describe the board.
- Good, because row-major spawn order (left to right, top to bottom) falls out naturally.
- Good, because `(0, 0)` being top-left is the near-universal expectation for a grid.
- Bad, because the first coordinate is vertical, which is the opposite of `Vector2`.
- Bad, because row and world Y grow in opposite directions.

### `(x, y)` with y growing upwards

Matching Unity's world axes exactly, so a cell coordinate scales directly into a position.

- Good, because position arithmetic needs no negation at all.
- Good, because it agrees with `Vector2`/`Vector3` ordering.
- Bad, because `(0, 0)` becomes the *bottom*-left cell, which contradicts how the board is read.
- Bad, because the Hierarchy would list cells bottom-up relative to the Scene view.

### A single flat index `0..8`

Cells numbered sequentially in reading order.

- Good, because it is the most compact form and maps directly onto a flat array.
- Bad, because `4` carries no visible meaning, where `(1, 1)` obviously names the center.
- Bad, because win detection has to reconstruct rows and columns via division and modulo.

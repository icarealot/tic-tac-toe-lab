# Cells use row-column coordinates with rows growing downward

## Status

Accepted

## Context and Problem Statement

Every cell needs one stable address shared by game rules, layout, tests, and conversation. Unity's vertical world axis grows upward, while a board is naturally read from top to bottom. Which convention should define cell addresses?

## Decision Drivers

- Addresses should match how people read and describe the board.
- Row and column ordering must remain consistent across every layer.
- Conversion between cell addresses and spatial points needs one owner.
- Reversing the convention later would affect rules, layout, views, tests, and saved examples.

## Considered Options

- Row and column, with rows growing downward from the top
- Horizontal and vertical coordinates matching Unity world axes
- One flat cell number

## Decision Outcome

Chosen option: "row and column, with rows growing downward from the top," because it matches reading order and common matrix language.

The top-left cell is `(0, 0)` and the bottom-right cell is `(2, 2)`. Row is always written before column. An immutable `CellCoordinate` carries the pair through the codebase, and board layout owns conversion between coordinates and spatial points, including the inverted vertical axis.

### Positive Consequences

- Spoken language, tests, and visible board order agree.
- A single value prevents accidental row-column argument reversal.
- Game rules can describe rows and columns without spatial translation.
- Vertical-axis inversion is confined to board layout.

### Negative Consequences

- The first coordinate is vertical rather than horizontal.
- Rows and Unity's world vertical axis grow in opposite directions.
- Every caller must use the project convention rather than a preferred `(x, y)` convention.

## Pros and Cons of the Options

### Row and column, with rows growing downward

- Good, because it matches reading order and matrix notation.
- Good, because the top-left origin is familiar for grids.
- Good, because game rules naturally reason about rows and columns.
- Bad, because spatial conversion must invert the vertical axis.

### Coordinates matching Unity world axes

- Good, because conversion to and from world axes is direct.
- Bad, because the board would be addressed bottom-to-top.
- Bad, because game terminology would repeatedly translate from spatial terminology.

### One flat cell number

- Good, because one primitive can identify any cell.
- Bad, because it hides the board's two-dimensional relationships.
- Bad, because row and column rules would repeatedly reconstruct those relationships.

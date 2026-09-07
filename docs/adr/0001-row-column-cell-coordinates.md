# Cells are addressed as row and column, with rows growing downward

## Status

Accepted

## Context and Problem Statement

Every cell needs a stable address shared by conversation, board layout and game rules. Unity’s vertical world axis grows upward, while people naturally read a tic-tac-toe board from top to bottom. Which convention should define cell addresses?

## Decision Drivers

- The board is read and discussed top-to-bottom and left-to-right.
- Addresses must be easy to compare with the visible board.
- The vertical-axis inversion should be explicit and confined.
- Every future feature touching a cell will depend on this convention, making later reversal expensive.

## Considered Options

- Row and column, with rows growing downward from the top
- Horizontal and vertical coordinates matching Unity world axes
- One flat cell number

## Decision Outcome

Chosen option: “row and column, with rows growing downward from the top,” because it matches how people read and describe the board.

The top-left cell is row zero, column zero. The bottom-right cell is row two, column two. Row is always spoken and written before column.

Rows and world vertical coordinates therefore grow in opposite directions. The board’s geometry owns that inversion so the rest of the project can use one consistent cell-address language.

### Positive Consequences

- Spoken vocabulary and visible board order agree.
- The center and corners have immediately understandable addresses.
- Future rules can refer to rows and columns without translating from another convention.
- The axis inversion has one conceptual home.

### Negative Consequences

- The first coordinate is vertical rather than horizontal.
- Translating between a cell address and a Unity point requires both an ordering change and a vertical inversion.
- Reversing the convention later would affect every feature that addresses a cell.

## Pros and Cons of the Options

### Row and column, with rows growing downward

- Good, because it matches reading order and common matrix language.
- Good, because the top-left origin is familiar for grids.
- Bad, because it disagrees with Unity’s upward vertical axis.

### Coordinates matching Unity world axes

- Good, because spatial conversion is direct.
- Good, because coordinate order matches Unity points.
- Bad, because the board would be addressed bottom-to-top.
- Bad, because conversation about rows would require translation.

### One flat cell number

- Good, because it is compact.
- Bad, because a number does not visibly communicate a row and column.
- Bad, because board rules would repeatedly reconstruct two-dimensional relationships.

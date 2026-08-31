# Tic Tac Toe Lab

A sandbox Unity project for building a tic-tac-toe game. This document is the shared vocabulary — the words we use for the things in the game, and the words we have deliberately rejected.

## Language

### The board

**Board**:
The 3x3 arrangement of cells that a game of tic-tac-toe is played on.
_Avoid_: Grid, playfield

**Cell**:
One of the nine positions on the board. A cell is addressed by its row and column.
_Avoid_: Tile, square, slot, space

**Spacing**:
The visible gap between two adjacent cells. Distinct from the size of a cell itself — the distance between two cell centres is a cell's size plus the spacing.
_Avoid_: Gap, margin, padding, pitch

### Addressing

**Row**:
A horizontal line of three cells. Rows are numbered from the top of the board downwards, starting at zero — so row 0 is the top row.
_Avoid_: Line, y

**Column**:
A vertical line of three cells. Columns are numbered from the left of the board rightwards, starting at zero — so column 0 is the leftmost column.
_Avoid_: File, x

**Cell coordinate**:
A cell's address, written and spoken as `(row, column)` — row always first. `(0, 0)` is the top-left cell and `(2, 2)` is the bottom-right. Note that row numbers grow downwards, which is the opposite direction to Unity's world Y axis; see [ADR 0001](./docs/adr/0001-row-column-cell-coordinates.md).
_Avoid_: (x, y), index, position

### Presentation

**View**:
A suffix marking a type as the visual representation of a domain concept, as opposed to the concept itself. A view knows how something looks and where it sits on screen; it does not own game state.
_Avoid_: Renderer, display, widget

**BoardView**:
The visual representation of the board. Owns the arrangement of cell views on screen.

**CellView**:
The visual representation of a single cell.

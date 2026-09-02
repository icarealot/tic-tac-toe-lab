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
The visible gap between two adjacent cells. Distinct from the size of a cell itself — the distance between two cell centers is a cell's size plus the spacing.
_Avoid_: Gap, margin, padding, pitch

### Addressing

**Row**:
A horizontal line of three cells — one of the three kinds of line. Rows are numbered from the top of the board downwards, starting at zero — so row 0 is the top row.
_Avoid_: y

**Column**:
A vertical line of three cells — one of the three kinds of line. Columns are numbered from the left of the board rightwards, starting at zero — so column 0 is the leftmost column.
_Avoid_: File, x

**Line**:
Three cells in a straight run — a row, a column, or a diagonal. There are eight lines on the board. A line is won when all three of its cells hold the same mark.
_Avoid_: Run, triple, streak, three-in-a-row

**Diagonal**:
A line of three cells running corner to corner — either `(0, 0)` to `(2, 2)` or `(0, 2)` to `(2, 0)`. Unlike rows and columns, the two diagonals are not numbered.
_Avoid_: Cross, slant

**Cell coordinate**:
A cell's address, written and spoken as `(row, column)` — row always first. `(0, 0)` is the top-left cell and `(2, 2)` is the bottom-right. Note that row numbers grow downwards, which is the opposite direction to Unity's world Y axis; see [ADR 0001](./docs/adr/0001-row-column-cell-coordinates.md).
_Avoid_: (x, y), index, position

**Point**:
A location in space, in Unity units — the center of a cell, or the place a player pressed. Distinct from a cell coordinate: a coordinate names *which* cell, a point names *where*. A point is always qualified by the space it is in: a world point or a local point.
_Avoid_: Position, location, coordinate

### Marks

**Mark**:
The thing a player puts into a cell. A mark is placed once and never moves or changes; a cell either holds a mark or is empty.
_Avoid_: Piece, symbol, token, move

**Empty**:
Said of a cell that holds no mark. The only kind of cell a player may press.
_Avoid_: Free, blank, available, unoccupied

**O**:
One of the two kinds of mark, drawn as a circle. Spoken as "an O".
_Avoid_: Nought, circle, zero

**X**:
One of the two kinds of mark, drawn as a cross. Spoken as "an X".
_Avoid_: Cross, ex

### Turns

**Turn**:
The mark that the next mark placed on the board will be. The turn is always an X or an O, never empty. It advances to the other mark each time a mark is placed while the game is in progress, and only then — a press that places nothing leaves the turn where it was. X has the first turn of a game. When a game ends the turn stops advancing, so once the outcome is a win the turn names the winner; after a draw it names nobody meaningful. Resetting a game returns the turn to X. See [ADR 0006](./docs/adr/0006-a-game-is-reset-rather-than-recreated.md).
_Avoid_: Player, side, current mark, go

### Outcome

**Outcome**:
How a game stands — in progress, won, or drawn. Every game has an outcome at every moment; it is not something a game acquires only at the end.
_Avoid_: Status, state, result, game over

**In progress**:
Said of a game that has not yet been won or drawn. The only outcome under which a mark may be placed.
_Avoid_: Playing, active, ongoing, unfinished

**Win**:
The outcome of a game in which some line holds three of the same mark. The winner is not part of the outcome — it is read from the turn, which stops advancing when the game ends. A win on the last empty cell is a win, not a draw.
_Avoid_: Victory, won, winner

**Draw**:
The outcome of a game in which every cell holds a mark and no line was won.
_Avoid_: Tie, stalemate, deadlock, full board

### Games

**Game**:
One playthrough, from an empty board to a win or a draw. A game is not an object that is created and thrown away — it is a phase the board passes through, and the board outlives it. See [ADR 0006](./docs/adr/0006-a-game-is-reset-rather-than-recreated.md).
_Avoid_: Round, match, play, session

**Board session**:
The long-lived pairing of one board's model, view and presenter, on which a series of games is played. A board session is created once, when the app starts, and is never replaced; when a game ends it resets that game after a pause and play continues. Its boundary is the board and the types that make it work — a scoreboard, a menu or an opponent is not part of it.
_Avoid_: Game session, match, board manager

**Reset**:
Returning a game to its starting state — every cell empty, X to play, in progress — without creating anything new. A reset undoes what a game put into the board; the model, the view, the cells and the presenter all survive it. Distinct from recreation, which this project deliberately does not do. The word *replay* is not used for this: it is reserved for a player-initiated restart that does not exist yet.
_Avoid_: Restart, clear, new game, replay

### Interaction

**Press**:
A complete pointer gesture over the board — down and then up — reported at the moment it completes. A press that never completes is not a press, so a player may push down on a cell, slide away, and release without placing a mark.
_Avoid_: Click, tap, touch, input

### Architecture

**Model**:
A suffix marking a type as the state of the game itself, independent of how it is shown. A model answers questions about what is true; it never talks to a view.
_Avoid_: State, data, entity

**View**:
A suffix marking a type as the visual representation of a domain concept, as opposed to the concept itself. A view knows how something looks and where it sits on screen; it does not own game state.
_Avoid_: Renderer, display, widget

**Presenter**:
A suffix marking a type that drives a view from a model. A presenter is where a rule about what the player may do is enforced; it is the only thing that both reads the model and commands the view.
_Avoid_: Controller, manager, mediator

**State**:
A suffix marking a type as one phase of the app — what the app is doing now, what it does on being entered, and what it undoes on being left. Exactly one state is current at a time, and a state is entered and left rather than created and destroyed.
_Avoid_: Screen, mode, phase, scene

**BoardModel**:
The state of the board — the mark in each cell or its emptiness, whose turn it is, and the outcome of the game — together with the board's dimensions and the geometry that resolves a local point to the cell containing it. It can be reset, which returns all three pieces of game state to their starting values. See [ADR 0006](./docs/adr/0006-a-game-is-reset-rather-than-recreated.md).

**BoardView**:
The visual representation of the board. Owns the arrangement of cell views on screen, and can be cleared of every mark shown on it without losing its cells.

**BoardPresenter**:
The type that turns a press into a placed mark: it refuses every press once the game is over, resolves the pressed point to a cell, rejects the press if that cell already holds a mark, and otherwise places the turn's mark on the board model and tells the board view to show it. It is also what announces how a game ended, and what resets the board model and the board view together.

**CellView**:
The visual representation of a single cell. It owns the mark view shown inside it: it is what puts a mark there and what takes it away again.

**MarkView**:
The visual representation of a single placed mark — the circle or the cross drawn inside a cell.

**BoardSession**:
The long-lived pairing of the board's model, view and presenter. It announces when a game has ended and resets the game on request; it is the only thing the states know about the board.

**StateMachine**:
The type that holds the app's states and makes one of them current, always leaving the state it is in before entering the next.

**GameplayState**:
The state the app is in while a game is being played. It listens for the game to end.

**GameCompleteState**:
The state the app enters when a game has ended. It waits, resets the game, and returns to gameplay.

**CoroutineService**:
The service that runs a coroutine on behalf of a type that is not a `MonoBehaviour`, and hands back a handle for stopping it.

**RoutineHandle**:
The handle to a running coroutine. Disposing it stops the coroutine; disposing it twice, or after the coroutine has finished, does nothing.

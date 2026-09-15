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
A cell's address, written and spoken as `(row, column)` — row always first. `(0, 0)` is the top-left cell and `(2, 2)` is the bottom-right. Note that row numbers grow downwards, which is the opposite direction to Unity's world Y axis; see ADR 0001.
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
The mark that the next mark placed on the board will be. The turn is always an X or an O, never empty. It advances to the other mark each time a mark is placed while the game is in progress, and only then — a press that places nothing leaves the turn where it was. X has the first turn of a game. When a game ends the turn stops advancing, so once the outcome is a win the turn names the winner; after a draw it names nobody meaningful. Resetting a game returns the turn to X. See ADR 0013.
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
One playthrough, from an empty board to a win or a draw. A game is not an object that is created and thrown away — it is a phase the board passes through, and the board outlives it. See ADR 0013.
_Avoid_: Round, match, play, session

**Board session**:
The long-lived pairing of one board's model, view and presenter, on which a series of games is played. A board session is created once when the app starts and is never replaced; starting a game resets it in place. Its boundary is the board and the types that make it work — a scoreboard, a menu or an opponent is not part of it.
_Avoid_: Game session, match, board manager

**Reset**:
Returning a game to its starting state — every cell empty, X to play, in progress — without creating anything new. A reset undoes what a game put into the board; the model, the view, the cells and the presenter all survive it. Distinct from recreation, which this project deliberately does not do. The word *replay* is not used for this: it is reserved for a player-initiated restart that does not exist yet.
_Avoid_: Restart, clear, new game, replay

### Interaction

**Press**:
A complete pointer gesture over the board — down and then up — reported at the moment it completes. A press that never completes is not a press, so a player may push down on a cell, slide away, and release without placing a mark.
_Avoid_: Tap, touch, input

**Click**:
An activation of a button through the Unity event system. A click is delivered by the event system rather than resolved by arithmetic hit-testing, so it belongs to buttons and other uGUI interactables — never to the board. It is a distinct term from **Press**, not a synonym: the two words name different mechanisms, and collapsing them would blur which one a test or a component exercises.
_Avoid_: Activate, invoke, button press

### User interface

**Window**:
A piece of user interface that the app shows, hides and stacks as a unit. Every window is either a panel or a popup; there is no third kind. A window is created when it is first shown and destroyed when it is taken off its stack, and it is hidden — rather than deactivated — while something covers it. See ADR 0008.
_Avoid_: Screen, view, widget, UI element, dialog

**Panel**:
The kind of window that covers the whole display. Only the topmost panel is visible; showing a panel hides the one beneath it, and it fills the display edge to edge so that a background reaches behind a notch.
_Avoid_: Screen, page, menu, layout

**Popup**:
The kind of window that sits above the panels without hiding them. Only the topmost popup is visible; showing a popup hides the popup beneath it, but never the panel beneath it. A popup is what asks a question or interrupts, and it is the first thing back closes. See ADR 0008.
_Avoid_: Dialog, modal, overlay, prompt, toast

**Outcome popup**:
The popup presented when a game ends. It names the winning mark or says Draw, and Continue or Back takes the player to the main menu.
_Avoid_: Result popup, game-over popup

**Layer**:
One of the two places a window lives on screen — the panel layer or the popup layer. Every popup is drawn above every panel, because the popup layer sits above the panel layer. Layers fill the whole display and are never inset.
_Avoid_: Sorting order, canvas, tier, z-order

**Safe area**:
The part of the display that a notch, a rounded corner or a system gesture bar does not intrude on. Anything a player is meant to read or press belongs inside it; a background does not. It is not fixed for the life of the app — it changes with orientation and with system settings — so a window follows it rather than reading it once.
_Avoid_: Inset, padding, notch area, margin

**Hidden**:
Said of a window that still exists but is not shown — because something covers it, not because it was taken off its stack. A hidden window keeps everything it had; it is faded out and stops receiving presses rather than being deactivated.
_Avoid_: Inactive, disabled, closed, off

**Back**:
The request to undo the last piece of navigation — the Android hardware back gesture, Escape, or the on-screen back button. It closes the topmost popup if there is one, and otherwise takes the topmost panel off its stack. What back actually does is decided by the state the app is in, not by the UI service; see ADR 0007.
_Avoid_: Cancel, escape, return, dismiss

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

**Service**:
A suffix marking a type that provides a mechanism to the rest of the app rather than owning domain state. The project uses it for camera, input, logging, coroutine, UI and factory collaborators. A service hides an external system or shared operation behind an interface; it does what it is asked and does not decide what the app means.
_Avoid_: Manager, helper, utility

**FactoryService**:
The single service that instantiates and destroys runtime Unity objects from the prefab registry. It does not configure the objects it creates.
_Avoid_: ComponentFactoryService, UIFactoryService, WindowFactoryService

**BoardModel**:
The state of the board — the mark in each cell or its emptiness, whose turn it is, and the outcome of the game — together with the board's dimensions, the cell placements derived from those dimensions, and the geometry that resolves a local point to the cell containing it. It can be reset, which returns all three pieces of game state to their starting values. See ADR 0013.

**BoardView**:
The visual representation of the board. Owns the arrangement of cell views on screen, and can be cleared of every mark shown on it without losing its cells.

**BoardPresenter**:
The type that turns a press into a placed mark: it refuses every press once the game is over, resolves the pressed point to a cell, rejects the press if that cell already holds a mark, and otherwise places the turn's mark on the board model and tells the board view to show it. It does not create the board's cell placements; those are derived by **BoardModel** and handed to **BoardView**. It is also what announces how a game ended, and what resets the board model and the board view together.

**CellView**:
The visual representation of a single cell. It owns the mark view shown inside it: it is what puts a mark there and what takes it away again.

**MarkView**:
The visual representation of a single placed mark — the circle or the cross drawn inside a cell.

**BoardSession**:
The long-lived pairing of the board's model, view and presenter. It announces when a game has ended and resets the game on request; it is the only thing the states know about the board.

**StateMachine**:
The type that holds the app's states and makes one of them current, always leaving the state it is in before entering the next.

**MainMenuState**:
The state the app is in while the main menu is shown. The menu is the app's entry point: it presents the game's title and a start control, and starting always begins a fresh game — a half-played board on the menu is never resumed. Back does nothing there: the menu is where navigation ends.

**GameplayState**:
The state the app is in while a game is being played. It listens for the game to end.

**GameCompleteState**:
The state the app enters when a game has ended. It leaves the completed board visible for a brief pause, then presents the outcome; once presented, Continue or Back takes the player to the main menu.

**CoroutineService**:
The service that runs a coroutine or schedules a callback after a delay on behalf of a type that is not a `MonoBehaviour`, and hands back a handle for cancelling the work.
_Avoid_: DelayService, WaitService

**CoroutineHandle**:
The handle to a running coroutine. Disposing it stops the coroutine; disposing it twice, or after the coroutine has finished, does nothing.

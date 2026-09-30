# Tic Tac Toe Lab

A glossary of the game and player-interaction language used throughout Tic Tac Toe Lab.

## Board

**Board**:
The 3×3 arrangement of cells on which a game is played.
_Avoid_: Grid, playfield

**Cell**:
One of the nine places on the board. A cell is identified by its cell coordinate and is either empty or holds one mark.
_Avoid_: Tile, square, slot, space

**Cell coordinate**:
A cell address written as `(row, column)`, with row first. `(0, 0)` is the top-left cell and `(2, 2)` is the bottom-right.
_Avoid_: Position, index, `(x, y)`

**Row**:
A horizontal group of three cells. Rows are numbered from top to bottom starting at zero.
_Avoid_: y

**Column**:
A vertical group of three cells. Columns are numbered from left to right starting at zero.
_Avoid_: File, x

**Line**:
Three cells in one row, column, or diagonal. A line is complete when all three cells hold the same mark.
_Avoid_: Run, triple, streak, three-in-a-row

**Spacing**:
The area separating adjacent cells. A board-local point in spacing belongs to no cell.
_Avoid_: Gap, margin, padding

**Board-local point**:
A point expressed relative to the board and used to determine which cell, if any, was pressed.
_Avoid_: Cell coordinate, screen point, world point

## Play

**Mark**:
An X or O placed permanently in an empty cell for the remainder of a game.
_Avoid_: Piece, symbol, token, move

**Empty**:
Said of a cell that holds no mark and can therefore receive one.
_Avoid_: Free, blank, available, unoccupied

**Turn**:
The mark that may be placed next while a game is in progress. X has the first turn, and a successful nonterminal placement passes the turn to the other mark; a terminal placement leaves it on the mark just placed.
_Avoid_: Player, side, current mark, go

**Game**:
One playthrough from an empty board until an X win, an O win, or a draw.
_Avoid_: Round, match, session

**Reset**:
Returning the board to the start of a game: every cell empty, X's turn, and the outcome in progress.
_Avoid_: Restart, clear, replay

## Outcome

**Outcome**:
How a game stands: in progress, won by X, won by O, or drawn. A game always has exactly one outcome.
_Avoid_: Status, state, result, game over

**In progress**:
The outcome while marks may still be placed.
_Avoid_: Playing, active, ongoing, unfinished

**X win**:
A terminal outcome in which a line holds three X marks.
_Avoid_: X victory, X won

**O win**:
A terminal outcome in which a line holds three O marks.
_Avoid_: O victory, O won

**Draw**:
A terminal outcome in which every cell holds a mark and no line is complete. A line completed by the final placement is a win, not a draw.
_Avoid_: Tie, stalemate, deadlock

## Interaction

**Home**:
The application's starting destination, from which the player chooses PvP or PvE.
_Avoid_: Main menu, main screen

**PvP**:
A game in which two human players take turns placing marks on the same device.
_Avoid_: Multiplayer, competitive mode

**PvE**:
A game in which one human player plays against a bot. The human always places X, the bot always places O, and therefore the human takes the first turn.
_Avoid_: Single-player, human-versus-bot

**Bot**:
The automated PvE participant. The bot always places O marks.
_Avoid_: AI player, computer player, enemy

**Bot turn**:
The interval after the human places X and before the bot places O. The bot waits for a brief, randomly chosen duration before placing its mark.
_Avoid_: AI turn, computer turn, thinking time

**Difficulty**:
The behavior used by the bot when choosing a cell. A PvE game has either Amateur or Professional difficulty.
_Avoid_: Level, bot mode

**Amateur**:
A difficulty at which the bot takes an immediate winning placement when one exists, otherwise blocks an immediate X win when possible, and otherwise chooses among empty cells. When multiple placements satisfy the highest available priority, it chooses randomly among them.
_Avoid_: Easy, beginner

**Professional**:
A difficulty at which the bot chooses a placement leading to the best achievable outcome. Among otherwise equal outcomes, it prefers an earlier win and a later loss; among equally optimal placements, it chooses randomly. Professional cannot lose when an outcome avoiding defeat remains possible.
_Avoid_: Hard, expert

**Press**:
A completed pointer gesture over the board that may place a mark in the targeted cell.
_Avoid_: Tap, touch, click

**Click**:
Activation of a user-interface button. A click is distinct from a board press.
_Avoid_: Press, tap

**Back**:
A request to leave or dismiss the current player-facing flow.
_Avoid_: Escape, return, cancel

**Outcome screen**:
The screen that reports an X win, O win, or draw and lets the player return Home.
_Avoid_: Outcome popup, result popup, game-over popup

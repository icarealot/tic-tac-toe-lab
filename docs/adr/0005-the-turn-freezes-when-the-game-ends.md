# The board model owns the outcome, and the turn freezes when the game ends

## Status

Superseded by [ADR 0006](./0006-a-game-is-reset-rather-than-recreated.md).

## Context and Problem Statement

A game of tic-tac-toe ends when a line holds three of the same mark, or when the board fills with no line won. Something has to notice that this has happened, and something has to say *who* won. The board already holds the marks and the turn, and `PlaceMark` is already the single operation through which the board changes.

## Decision Drivers

- The winner must not be derivable incorrectly. If `Turn` is readable after a win and names the loser, someone will eventually read it and ship the bug.
- Illegal states should be unrepresentable. A design in which "won, but by nobody" or "won and drawn" can be expressed is a design with a test that has to police it.
- Whatever answers "is the game over?" has to be readable by things that did not witness the move: the planned `GameComplete` state, an "X wins" label, a future AI. A fact known only to `BoardPresenter` as a local variable is not readable by any of them.
- `PlaceMark` has to evaluate the outcome anyway. Whether the turn advances now depends on whether the game just ended, so the evaluation cannot be deferred to a caller without reintroducing the "call these in this order" fragility 0004 exists to eliminate.
- The board is nine cells. There is no performance argument anywhere in this decision, so clarity wins every tie.
- `CONTEXT.md` already models emptiness as the absence of a mark rather than a third kind of mark. A new domain enum should not casually contradict that instinct — but nor should it be nullable just to look consistent.

## Considered Options

- The outcome is `InProgress`/`Win`/`Draw`, and the turn freezes when the game ends so `Turn` names the winner
- The outcome is `InProgress`/`Win`/`Draw` alongside a separate `Mark? Winner` property
- The outcome names the winner itself: `InProgress`/`XWins`/`OWins`/`Draw`
- The model exposes only `InProgress`/`Win`/`Draw`, and the presenter reports the winner from the mark it just placed

## Decision Outcome

Chosen option: "the outcome is `InProgress`/`Win`/`Draw`, and the turn freezes when the game ends", because it is the only option in which the winner is a fact the model already holds rather than a second fact that has to be kept consistent with the first.

`BoardModel` gains:

```
public Outcome Outcome { get; private set; }   // starts as Outcome.InProgress

public void PlaceMark(int row, int column)     // places Turn, evaluates the outcome,
                                               // and advances Turn only if still InProgress
```

`Outcome` is an enum of `InProgress`, `Win`, `Draw`, living at `Runtime/Board/Outcome.cs` — beside the type that exposes it, rather than in a folder of its own, because the folders in this project track concepts that have a view and an outcome does not.

The rule, stated once: **the turn advances only while the game is in progress.** Not "unless someone won" — a drawn board has no next mark either, so the same rule covers both endings without an exception. The consequence is that once `Outcome` is `Win`, `Turn` still names the mark that was just placed, which is the winner. After a `Draw`, `Turn` names whichever mark sealed the board (always X, which places the first, third, fifth, seventh and ninth marks) and means nothing; the outcome already says nobody won.

Evaluation walks every line — three rows, three columns, both diagonals — expressed in terms of `Dimension` rather than a hardcoded 3 or a fixed table of eight coordinate triples, because "a line is won when every cell in it holds the same mark" is the domain sentence and the loop is that sentence. The win check runs before the draw check, so a win on the last empty cell is a `Win`, never a `Draw`. Fullness is read by scanning the cells, not by a placement counter, for the same reason the whole board is scanned rather than just the lines through the last cell: the marks are the only fact, and a cached count is a second fact that can drift.

`PlaceMark` gains no guard of its own. Called on a finished game it will place another mark, and because the turn is frozen that mark will be a second consecutive X — a state no sequence of legal presses can reach. That is deliberate and consistent with the model's existing willingness to overwrite an occupied cell if asked: `BoardModel` does what it is told, and `BoardPresenter` decides what to tell it.

`BoardPresenter` therefore grows the guard, as the first thing `OnPressed` does — before resolving the point to a cell, because "the game is over, nothing you press matters" is true of presses that land nowhere near the board too. Like the occupied-cell rejection, it logs, so that a refused press always says why and a silent press is always a bug. After a successful placement the presenter reads `Outcome` and announces the ending: `$"{Turn} wins"` on a win, `"Draw"` on a draw.

### Positive Consequences

- The winner cannot disagree with the outcome, because there is only one place it is written.
- "Decide who won from the current turn" — the intuition a reader will arrive with — becomes *true* rather than subtly wrong. The trap 0004 warned about is closed rather than documented.
- The outcome is model state, so the planned `GameComplete` state, a result label, and any future AI can all read it without watching a placement happen.
- Every rule of a move is inside one method. A caller cannot place without evaluating, evaluate without placing, or advance the turn past the end of a game.
- Wins, draws and the freeze are all tested in `EditModeTests` against the model alone, with no fakes and no scene.
- The check being a loop over `Dimension` keeps `DIMENSION` an honest constant — the model's geometry and its rules would both survive a change to it.

### Negative Consequences

- **`Turn` now means two things.** While a game runs it is "the mark that will be placed next"; once a game is won it is "the mark that won". One word, two readings, disambiguated only by `Outcome`. Reading `Turn` without checking `Outcome` is the new version of the old ordering bug.
- **After a draw, `Turn` is meaningless but still readable.** It will always be X, and nothing in the type says not to trust it. A reader who assumes symmetry with the win case gets a plausible, wrong answer.
- **The freeze looks like a missing line.** `PlaceMark` will contain a branch that deliberately does *not* advance the turn, which reads as a bug against 0004's headline rule. This ADR exists largely to answer the reader who tries to "fix" it.
- **`PlaceMark` on a finished game produces an impossible board** — two consecutive marks from the same player — and nothing complains. The presenter's guard is the only defence, so any future caller of the model must remember the same rule.
- `BoardModel` takes on a fourth concern. It was the marks, the geometry and the turn; it is now also the rules of winning. Its `CONTEXT.md` entry lists all four, and `CONTEXT.md`'s definition of **Presenter** as the home of rules drifts a little further from the truth.
- A rejected press on a finished game logs a line no matter where on screen it landed, so an idle player clicking around a finished board fills the log.

## Pros and Cons of the Options

### `InProgress`/`Win`/`Draw` with the turn frozen at the end

The outcome says *that* the game ended; the turn says *who by*, because it stopped moving.

- Good, because the winner is not stored twice and so cannot be stored inconsistently.
- Good, because the obvious way to ask "who won?" is also the correct way.
- Good, because the freeze rule covers wins and draws with one sentence and no exception.
- Good, because it needs no new state beyond the outcome itself.
- Bad, because `Turn` acquires a second meaning that only `Outcome` disambiguates.
- Bad, because `Turn` after a draw is readable, stable, and meaningless.
- Bad, because a branch that skips advancing the turn looks like an omission to anyone who has read 0004 and not this.

### `InProgress`/`Win`/`Draw` plus a separate `Mark? Winner`

The model sets `Winner` at the moment the winning mark is placed, and leaves it null otherwise.

- Good, because `Turn` keeps exactly one meaning and stops mattering once the game ends.
- Good, because the winner is explicit and named, with no caveat about which property to trust when.
- Bad, because `Win` with a null `Winner`, and `Draw` with a non-null one, are both expressible and both nonsense.
- Bad, because answering "who won" takes two reads that can be combined wrongly.
- Bad, because it adds a property whose value is always derivable from state the model already has.

### `InProgress`/`XWins`/`OWins`/`Draw`

The winner is a property of the outcome value itself.

- Good, because one read answers everything and no illegal combination exists.
- Good, because `Turn` needs no second meaning and no freeze — the rule from 0004 stays exactly as written.
- Good, because a `switch` over the outcome naturally covers every ending.
- Bad, because the enum grows with the board: three marks would mean five values, and each new value has to be spelled out.
- Bad, because "did anyone win?" becomes a two-value test rather than one comparison.
- Bad, because it splits one concept — winning — across two enum members that must be kept in step.

### The presenter reports the winner from the mark it just placed

The model says only `InProgress`/`Win`/`Draw`; the presenter, which already holds the placed mark in a local to pass to `ShowMark`, logs the winner from that.

- Good, because the model gains no ambiguity at all and `Turn` keeps one meaning.
- Good, because it requires no new state anywhere.
- Bad, because the winner exists only for the duration of one method call, so nothing that did not witness the move can ever learn it.
- Bad, because the `GameComplete` state and any result display would each have to be handed the winner by whoever happened to be watching.
- Bad, because it makes the presenter the sole authority on a fact about the game, which is precisely the split 0004 rejected for the turn.

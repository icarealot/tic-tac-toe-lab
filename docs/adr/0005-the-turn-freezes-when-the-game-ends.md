# The board model owns the outcome, and the turn freezes when the game ends

## Status

Superseded by [ADR 0006](./0006-a-game-is-reset-rather-than-recreated.md).

## Context and Problem Statement

A game ends in a win or draw. The app must know that it ended and, after a win, which mark won. Where should outcome live, and how should winner information relate to the turn?

## Decision Drivers

- Outcome and winner information must not contradict each other.
- Illegal combinations such as a win without a winner should be difficult to represent.
- Features that did not witness the final move must still be able to read the outcome.
- Outcome evaluation belongs with the board state it examines.
- Clarity is more important than optimization for a nine-cell board.

## Considered Options

- Outcome has in-progress, win and draw values; the turn freezes at the end
- Outcome is paired with a separate optional winner
- The outcome distinguishes X wins from O wins
- The presenter reports the winner only when the final mark is placed

## Decision Outcome

Chosen option: “outcome has in-progress, win and draw values, and the turn freezes at the end,” because the winner then remains derivable from state the board already owns.

The board always has an outcome. After each placement it checks for a win before checking for a draw. The turn advances only while the outcome remains in progress. After a win, the frozen turn names the winner. After a draw, the turn has no meaningful interpretation.

The board evaluates every row, column and diagonal from its marks rather than maintaining duplicate summary state. The presenter prevents further presses from reaching a completed game.

### Positive Consequences

- Winner and outcome cannot drift as separately written facts.
- Any feature can read the current outcome from the board.
- A final-cell win is correctly distinguished from a draw.
- Win and draw behavior remain directly testable.

### Negative Consequences

- Turn means the next mark during play but the winning mark after a win.
- Turn remains readable but meaningless after a draw.
- Freezing the turn can look like an omitted advancement.
- The board model grows to include outcome evaluation.
- The presenter remains the guard against attempts to play after completion.

## Pros and Cons of the Options

### Outcome with a frozen turn

- Good, because winner information is derived rather than duplicated.
- Good, because one rule covers both wins and draws.
- Bad, because turn has context-dependent meaning.
- Bad, because draw leaves a meaningless turn value.

### Outcome with a separate optional winner

- Good, because turn keeps one meaning.
- Good, because winner is explicit.
- Bad, because contradictory outcome and winner combinations are representable.

### Winner-specific outcomes

- Good, because one value communicates the complete result.
- Good, because no frozen-turn interpretation is needed.
- Bad, because winner-neutral questions become less direct.
- Bad, because the outcome set grows with the set of marks.

### Presenter-only winner reporting

- Good, because no additional board state is needed.
- Bad, because the winner disappears after the final interaction.
- Bad, because other features cannot independently read who won.

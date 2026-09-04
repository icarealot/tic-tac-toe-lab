# The board model owns the turn and advances it on placement

## Status

Superseded by [ADR 0004](./0004-the-board-model-owns-the-turn.md).

## Context and Problem Statement

Marks must alternate: a press places an X, the next places an O, and so on. Something has to hold whose turn it is, and something has to advance it. `BoardPresenter` already resolves a press to a cell and decides whether a mark may be placed there, so it is the obvious candidate — but the turn is a fact about the game rather than about presentation, and the rule that binds it to placement is the one most likely to be broken by a later edit. Which type owns the turn, and is advancing it a separate step or part of placing a mark?

## Decision Drivers

- Alternation is only correct if the turn advances on a placed mark and on nothing else. A press that resolves to no cell, or lands on an occupied cell, must leave the turn untouched — otherwise a player can pass their turn by pressing an occupied cell, which is a legal-move exploit disguised as a no-op.
- A rule expressed as "call these two methods in this order" is a rule waiting to be broken. A rule expressed as "these cannot be called separately" is not.
- The project is built test-first, and `EditModeTests` is where the cheapest tests live. Alternation asserted directly on a model needs no fake view, no fake input service and no fake camera.
- The planned app state machine (`Loading → MainMenu → Gameplay → GameComplete`) will own the game's models and dispose them between rounds. Whichever type holds the turn is a type that machine must own and reset.
- `CONTEXT.md` defines **Presenter** as the place "where a rule about what the player may do is enforced", so moving any rule into a model is a deliberate deviation that needs recording.

## Considered Options

- `BoardModel` owns the turn, and `PlaceMark` advances it
- `BoardPresenter` holds the turn in a private field and advances it after placing
- A separate `TurnModel` injected alongside `BoardModel`

## Decision Outcome

Chosen option: "`BoardModel` owns the turn, and `PlaceMark` advances it", because it is the only option in which placing a mark without advancing the turn is not expressible.

`PlaceMark` loses its mark parameter entirely. The model exposes the turn as a read-only property that starts at X, and placing takes only a row and a column: it places whichever mark the turn names, then advances it.

A caller cannot place the wrong mark, cannot place without advancing, and cannot advance without placing. The alternation rule is a property of the type rather than a sequence a caller must remember.

`BoardPresenter` keeps the rule that actually belongs to it — deciding *whether* a press becomes a placement at all. It resolves the pressed point to a cell, rejects presses on occupied cells, and only then calls `PlaceMark`. Rejected presses never reach the model, so the turn is untouched by construction rather than by a guard.

This decision is paired with the removal of `Mark.Empty`. A cell's emptiness is now the absence of a mark (`Mark?`), not a third kind of mark, which is what `CONTEXT.md` always said it was. That removal is what allows `Turn` to be typed as `Mark`: with `Empty` gone, there is no unrepresentable state for the turn to fall into, and no guard needed to keep it out of one.

### Positive Consequences

- The alternation rule cannot be broken by a caller, only by editing `BoardModel` itself — which is where a reader would look for it.
- Alternation is tested in `EditModeTests` against the model alone, with no fakes and no scene.
- `MarkView.Show` loses its domain guard. Its throwing default arm survives only because C# requires one on an enum switch expression; it no longer defends against a state the domain forbids.
- The app state machine gets a single object to own and reset per round: a fresh `BoardModel` is an empty board with X to play.

### Negative Consequences

- **`BoardPresenter` must read `Turn` before calling `PlaceMark`**, because afterwards it is already the other mark, and the presenter needs the placed mark to pass to `BoardView.ShowMark`. Reading it after the call shows the wrong mark on every placement. This is the failure most likely to be introduced later; the test that presses twice and expects an X then an O is what catches it.
- **`PlaceMark(row, column)` looks incomplete.** A method that places a mark without taking one invites a future reader to "fix" it by restoring the parameter, which would silently reintroduce the ability to place without advancing. This ADR exists largely to answer that reader.
- `BoardModel` stops being one idea. It is now the marks, the geometry *and* the turn, and its `CONTEXT.md` entry has to list all three.
- The model enforces a rule that `CONTEXT.md` assigns to presenters. The division still holds for the rule that matters — the presenter decides what is allowed — but the boundary is no longer clean, and a future rule will have to be placed by judgement rather than by the glossary.
- A second board, or a board rendered as a preview, cannot exist without also carrying a turn.

## Pros and Cons of the Options

### `BoardModel` owns the turn, and `PlaceMark` advances it

Placement and alternation are a single indivisible operation on the model.

- Good, because placing without advancing is not expressible.
- Good, because alternation is testable with no fakes at all.
- Good, because a caller cannot place the wrong mark by passing the wrong argument.
- Bad, because `PlaceMark` no longer names what it places, which reads as an omission.
- Bad, because the presenter must read `Turn` first, an ordering dependency the type cannot enforce.
- Bad, because `BoardModel` becomes two concerns in one type.

### `BoardPresenter` holds the turn in a private field

The presenter keeps `private Mark _turn = Mark.X;` and flips it after a successful `PlaceMark`/`ShowMark` pair.

- Good, because `BoardModel` stays exactly what its glossary entry says it is.
- Good, because rule enforcement stays where `CONTEXT.md` puts it.
- Bad, because alternation can only be tested through presses, a fake view, a fake input service and a fake camera.
- Bad, because the flip is a statement that can be moved above an early return by a well-meaning edit, turning a rejected press into a passed turn.
- Bad, because nothing else can read the turn — a "X to play" label or an AI would have to reach into a presenter for game state.

### A separate `TurnModel`

A small model holding only the turn, constructed in `Bootstrap` and injected into the presenter alongside `BoardModel`.

- Good, because each model stays a single idea.
- Good, because the turn is model state, readable by anything that needs it.
- Bad, because placement and alternation land in different objects, so nothing binds them together and the presenter is back to sequencing two calls correctly.
- Bad, because it adds a type and a constructor argument to express one enum value.

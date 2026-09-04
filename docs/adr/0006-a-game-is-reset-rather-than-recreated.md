# The board model owns the turn and the outcome, and a game is reset rather than recreated

## Status

Accepted.

## Context and Problem Statement

Three questions are settled together here, because the third cannot be answered without the first two.

Marks must alternate: a press places an X, the next places an O. Something has to hold whose turn it is, and something has to advance it.

A game ends when a line holds three of the same mark, or when the board fills with no line won. Something has to notice, and something has to say who won.

And when a game has ended, the next one must begin — automatically, one second later — without the player doing anything. Something has to wait, something has to put the board back to its starting state, and a decision has to be made about whether "putting it back" means creating a new board or clearing the one that already exists.

## Decision Drivers

- Alternation is only correct if the turn advances on a placed mark and on nothing else. A press that resolves to no cell, or lands on an occupied cell, must leave the turn untouched — otherwise a player can pass their turn by pressing an occupied cell, which is a legal-move exploit disguised as a no-op.
- A rule expressed as "call these two methods in this order" is a rule waiting to be broken. A rule expressed as "these cannot be called separately" is not.
- The winner must not be derivable incorrectly. If `Turn` is readable after a win and names the loser, someone will eventually read it and ship the bug.
- Illegal states should be unrepresentable. A design in which "won, but by nobody" or "won and drawn" can be expressed is a design with a test that has to police it.
- Whatever answers "is the game over?" has to be readable by things that did not witness the move: `GameCompleteState`, an "X wins" label, a future AI. A fact known only to `BoardPresenter` as a local variable is not readable by any of them.
- The project is built test-first, and `EditModeTests` is where the cheapest tests live. A rule asserted directly on a model needs no fake view, no fake input service and no fake camera. A rule that can only be observed after a real second has passed needs a play-mode test and a real second.
- The board is nine cells. There is no performance argument anywhere in this decision, so clarity wins every tie.
- `CONTEXT.md` defines **Presenter** as "the only thing that both reads the model and commands the view", and as the place "where a rule about what the player may do is enforced". Any design that adds a second type doing either of those things costs a definition the project has already written down and defended.
- A restart must not flicker. The finished board stays on screen for the full second; only then does it clear. Whatever performs the reset must therefore be separable in time from whatever detects the ending.
- A player-initiated replay button is planned. Whatever restarts a game automatically must be the same operation a button would call, or the button will arrive as a second, subtly different restart path.

## Considered Options

**Ownership of the turn**

- `BoardModel` owns the turn, and `PlaceMark` advances it
- `BoardPresenter` holds the turn in a private field and advances it after placing
- A separate `TurnModel` injected alongside `BoardModel`

**Ownership of the outcome, and how the winner is known**

- The outcome is `InProgress`/`Win`/`Draw`, and the turn freezes when the game ends so `Turn` names the winner
- The outcome is `InProgress`/`Win`/`Draw` alongside a separate `Mark? Winner` property
- The outcome names the winner itself: `InProgress`/`XWins`/`OWins`/`Draw`
- The model exposes only `InProgress`/`Win`/`Draw`, and the presenter reports the winner from the mark it just placed

**What happens between one game and the next**

- The board's model, view and presenter are long-lived, and a game is reset in place
- The board's model, view and presenter are disposed and recreated for each game
- The model is reset and the view is rebuilt by re-running its construction

**Where the restart is driven from**

- An app state machine, with `GameplayState` and `GameCompleteState`, driving a long-lived `BoardSession`
- `BoardPresenter` waits and restarts itself
- The full planned state machine — `Loading → MainMenu → Gameplay → GameComplete` — built out now

**How the delay is waited**

- An injected `ICoroutineService`, faked in tests
- `Awaitable.WaitForSecondsAsync` called directly from the state
- A per-frame tick accumulating `Time.deltaTime`

## Decision Outcome

### The board model owns the turn, and `PlaceMark` advances it

Chosen because it is the only option in which placing a mark without advancing the turn is not expressible.

The model exposes the turn as a read-only property that starts at X, and `PlaceMark` takes only a row and a column: it places the turn's mark, evaluates the outcome, and advances the turn only if the game is still in progress.

A caller cannot place the wrong mark, cannot place without advancing, and cannot advance without placing. The alternation rule is a property of the type rather than a sequence a caller must remember.

`BoardPresenter` keeps the rule that actually belongs to it — deciding *whether* a press becomes a placement at all. It resolves the pressed point to a cell, rejects presses on occupied cells, and only then calls `PlaceMark`. Rejected presses never reach the model, so the turn is untouched by construction rather than by a guard.

This is paired with the absence of `Mark.Empty`. A cell's emptiness is the absence of a mark (`Mark?`), not a third kind of mark, which is what `CONTEXT.md` always said it was. That is what allows `Turn` to be typed as `Mark`: with no `Empty`, there is no unrepresentable state for the turn to fall into, and no guard needed to keep it out of one.

### The board model owns the outcome, and the turn freezes when a game ends

Chosen because it is the only option in which the winner is a fact the model already holds rather than a second fact that has to be kept consistent with the first.

The model exposes the outcome as a read-only property that starts as in progress.

`Outcome` is an enum of `InProgress`, `Win`, `Draw`, living at `Runtime/Board/Outcome.cs` — beside the type that exposes it, rather than in a folder of its own, because the folders in this project track concepts that have a view and an outcome does not.

The rule, stated once: **the turn advances only while the game is in progress.** Not "unless someone won" — a drawn board has no next mark either, so the same rule covers both endings without an exception. The consequence is that once `Outcome` is `Win`, `Turn` still names the mark that was just placed, which is the winner. After a `Draw`, `Turn` names whichever mark sealed the board (always X, which places the first, third, fifth, seventh and ninth marks) and means nothing; the outcome already says nobody won.

Evaluation walks every line — three rows, three columns, both diagonals — expressed in terms of `Dimension` rather than a hardcoded 3 or a fixed table of eight coordinate triples, because "a line is won when every cell in it holds the same mark" is the domain sentence and the loop is that sentence. The win check runs before the draw check, so a win on the last empty cell is a `Win`, never a `Draw`. Fullness is read by scanning the cells, not by a placement counter, for the same reason the whole board is scanned rather than just the lines through the last cell: the marks are the only fact, and a cached count is a second fact that can drift.

`PlaceMark` gains no guard of its own. Called on a finished game it will place another mark, and because the turn is frozen that mark will be a second consecutive X — a state no sequence of legal presses can reach. That is deliberate and consistent with the model's existing willingness to overwrite an occupied cell if asked: `BoardModel` does what it is told, and `BoardPresenter` decides what to tell it.

`BoardPresenter` therefore holds the guard, as the first thing `OnPressed` does — before resolving the point to a cell, because "the game is over, nothing you press matters" is true of presses that land nowhere near the board too. Like the occupied-cell rejection, it logs, so that a refused press always says why and a silent press is always a bug. After a successful placement the presenter reads `Outcome` and announces the ending: `$"{Turn} wins"` on a win, `"Draw"` on a draw.

### A game is reset, not recreated

**A game is a phase the same objects pass through, not an object lifetime.** One `BoardModel`, one `BoardView` and one `BoardPresenter` are constructed at startup and live for the whole run of the app. Ending a game does not destroy them; it returns them to their starting state.

Resetting the model empties every cell, returns the turn to X and the outcome to in progress. Clearing the view has every cell view clear the mark view it spawned, the cell views themselves surviving untouched. Resetting the presenter calls both, in that order.

The alternative — a fresh `BoardModel` and a fresh `BoardPresenter` per game — has one genuine advantage this design gives up: a newly constructed object cannot forget to clear a field, whereas `Reset()` can be left behind when a fifth piece of state is added to `BoardModel`. That risk is accepted and named below, because recreation costs more than it saves here. The nine `CellView`s and the `BoardView` are pure scaffolding that no game ever changes, so destroying and re-instantiating them every game is work done to produce an object identical to the one thrown away — and every subscription in the app, the presenter's to `IInputService.Pressed` and the session's to the presenter, would have to be torn down and rebuilt on the same cadence.

`CellView` owns the `MarkView` it spawns. `ShowMark(IFactoryService, Mark)` gains its mirror, `ClearMark(IFactoryService)`, which returns the mark view through `IFactoryService.Return`. `BoardView.Clear()` is then a loop over cells, and no type holds a reference to an object parented under another type's transform.

### `BoardSession` owns the board, and a two-state machine drives it

`BoardSession` is constructed in `Bootstrap`, holds the `BoardModel`, `BoardView` and `BoardPresenter`, and is the only thing the states talk to. It forwards the presenter's ending as a `GameEnded` event of its own, and exposes a `Reset` that delegates to `BoardPresenter.Reset`.

It is named for the board and not for the game deliberately. Its boundary is one board and the three types that make it work; a scoreboard, a round counter or an AI opponent are not board things, and must not be added to it on the strength of its name.

`Reset` delegates rather than resetting the parts itself, so `BoardPresenter` remains the only type that both reads the model and commands the view, and only one type knows that a board is a model plus a view.

The app state machine is introduced with exactly two states, not the full `Loading → MainMenu → Gameplay → GameComplete` sequence the app will eventually need. `IAppState` is entered and left. `AppStateMachine` holds its states by type and makes one current, leaving the state it is in before entering the next. `GameplayState` subscribes to the session's ending on entry, drops the subscription on leaving, and enters `GameCompleteState` when it fires. `GameCompleteState` runs a routine on entry and disposes it on leaving.

`Loading` and `MainMenu` are not built. They are still expected, but a state that does nothing is a box drawn to match a diagram. They land in `Runtime/State/` when they have something to do, without changing anything decided here.

`GameplayState` learns of the ending from an event rather than by polling `Outcome`, so nothing in the app needs a per-frame tick. `CONTEXT.md` already casts the presenter as the thing that "announces how a game ended"; `event Action GameEnded` makes that sentence literal. It carries no payload: the presenter continues to log `"X wins"` and `"Draw"` itself, and nothing else yet needs to know how a game ended. A payload would have to be `(Outcome, Mark)` — the winner lives in the frozen turn, not in the outcome — which is a domain type invented to serve a log line that already works.

### The wait is an injected coroutine service

`CoroutineService` is a `MonoBehaviour` obtained through `IFactoryService`, and `Run` returns a `CoroutineHandle` — its own type, in its own file, holding the host `MonoBehaviour` and the `Coroutine` so it can stop it on `Dispose`. The handle is what makes cancellation a first-class operation: `GameCompleteState.Leave()` disposes it, and a test can assert that it was disposed. Unity's own `Coroutine` type has no public constructor, so a faked service could only ever return `null` from `Run`, and "the pending reset was cancelled" would be an assertion against a null.

The restart itself waits for the pause, resets the session, and only then changes back to the gameplay state.

`WaitForSeconds` rather than `WaitForSecondsRealtime`, so a future pause that sets `Time.timeScale = 0` freezes the pending restart along with everything else. The constant lives on `GameCompleteState`, next to the only code that reads it, as `DIMENSION` and `CELL_SIZE` live on `BoardModel`.

`Reset()` is called at the *end* of the routine, immediately before returning to `GameplayState`, rather than on entering `GameplayState`. The finished board therefore stays on screen for the whole second, and — more importantly — `GameplayState` stays a state that can be entered without destroying the game in progress, which a future `Paused` or `Settings` state will need.

The service is injected rather than the state calling `Awaitable.WaitForSecondsAsync` directly, because every other collaborator that touches the world outside the model — input, camera, logging, instantiation — is already behind an interface for exactly this reason. A fake service captures the routine and pumps `MoveNext()`; the yielded `WaitForSeconds` is ignored, so the whole restart cycle becomes an edit-mode test that runs in microseconds.

### Teardown is explicit

`AppStateMachine` gains a disposal that calls `Leave()` on the current state, so a pending routine is stopped and a live subscription dropped. `BoardSession.Dispose()` unsubscribes from and disposes the presenter. `Bootstrap.OnDestroy` disposes the machine, the session and the input service.

Destroying the `CoroutineService` game object would stop its coroutines anyway, and every subscription here is between objects that die together — so today this code is belt and braces. It is written anyway because "nothing leaks" is currently true by accident of destruction order, and that accident expires the first time a scene is unloaded while a board session is meant to survive it.

### Positive Consequences

- The alternation rule cannot be broken by a caller, only by editing `BoardModel` itself — which is where a reader would look for it.
- The winner cannot disagree with the outcome, because there is only one place it is written. "Decide who won from the current turn" — the intuition a reader arrives with — is *true* rather than subtly wrong.
- The outcome is model state, so `GameCompleteState`, a future result label and any future AI can all read it without watching a placement happen.
- Every rule of a move is inside one method. A caller cannot place without evaluating, evaluate without placing, or advance the turn past the end of a game.
- Alternation, wins, draws and the freeze are all tested in `EditModeTests` against the model alone, with no fakes and no scene.
- The line check being a loop over `Dimension` keeps `DIMENSION` an honest constant — the model's geometry and its rules would both survive a change to it.
- Restarting allocates nothing but the marks of the next game. No cell views are destroyed, no subscriptions are rebuilt, and no object graph is re-wired between games.
- The planned replay button is additive rather than a second restart path: it calls the same `BoardSession.Reset()`, and because `GameCompleteState.Leave` disposes the pending routine, pressing it during the delay cannot produce two restarts.
- The states are small enough to test individually with a fake session and a fake coroutine service, and the whole one-second cycle is asserted without a second passing.
- `ICoroutineService` is a general-purpose seam, not a delay: the first mark-placement animation can use it without redesigning anything.

### Negative Consequences

- **`Reset()` can be forgotten.** This is the price of not recreating. A fifth piece of state added to `BoardModel` will be initialised in its declaration and cleared nowhere, and every game after the first will carry it over. A newly constructed model could not have this bug. The defence is a test asserting that a reset model is indistinguishable from a new one, and it must be extended whenever the model grows.
- **`BoardModel.Reset` is a second writer of `Turn`.** The alternation rule above makes placement and advancement one indivisible operation; `Reset` now also assigns `Turn`, without a placement. It restores the starting state rather than advancing, so the rule holds — but "only `PlaceMark` touches the turn" is no longer literally true, and the next writer of turn-related state has one more precedent to reason about.
- **`Turn` means two things.** While a game runs it is "the mark that will be placed next"; once a game is won it is "the mark that won". One word, two readings, disambiguated only by `Outcome`. Reading `Turn` without checking `Outcome` is the standing trap.
- **After a draw, `Turn` is meaningless but still readable.** It will always be X, and nothing in the type says not to trust it.
- **The freeze looks like a missing line.** `PlaceMark` contains a branch that deliberately does *not* advance the turn, which reads as a bug against the headline alternation rule. This ADR exists partly to answer the reader who tries to "fix" it.
- **`PlaceMark(row, column)` looks incomplete.** A method that places a mark without taking one invites a future reader to restore the parameter, which would silently reintroduce the ability to place without advancing.
- **`BoardPresenter` must read `Turn` before calling `PlaceMark`**, because afterwards it is already the other mark. Reading it after the call shows the wrong mark on every placement. The test that presses twice and expects an X then an O is what catches this.
- **`PlaceMark` on a finished game produces an impossible board** — two consecutive marks from the same player — and nothing complains. The presenter's guard is the only defence, so any future caller of the model must remember the same rule.
- `BoardModel` is not one idea. It is the marks, the geometry, the turn *and* the rules of winning, and its `CONTEXT.md` entry lists all four. `CONTEXT.md`'s definition of **Presenter** as the home of rules is correspondingly less than the whole truth.
- The app now has a state machine with two states and a session, where before it had a presenter constructed in `Bootstrap`. That is three new types and an interface to serve one second of delay, justified only by the states expected to follow.
- A second board, or a board rendered as a preview, cannot exist without also carrying a turn, an outcome and a session.

### The refusal log is now bounded

A press on a finished game is refused and logged no matter where on screen it landed, so an idle player clicking around a finished board fills the log. That window is now one second long. The behaviour is unchanged; its cost is capped.

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
- Bad, because nothing else can read the turn — an "X to play" label or an AI would have to reach into a presenter for game state.

### A separate `TurnModel`

A small model holding only the turn, constructed in `Bootstrap` and injected into the presenter alongside `BoardModel`.

- Good, because each model stays a single idea.
- Good, because the turn is model state, readable by anything that needs it.
- Bad, because placement and alternation land in different objects, so nothing binds them together and the presenter is back to sequencing two calls correctly.
- Bad, because it adds a type and a constructor argument to express one enum value.

### `InProgress`/`Win`/`Draw` with the turn frozen at the end

The outcome says *that* the game ended; the turn says *who by*, because it stopped moving.

- Good, because the winner is not stored twice and so cannot be stored inconsistently.
- Good, because the obvious way to ask "who won?" is also the correct way.
- Good, because the freeze rule covers wins and draws with one sentence and no exception.
- Good, because it needs no new state beyond the outcome itself.
- Bad, because `Turn` acquires a second meaning that only `Outcome` disambiguates.
- Bad, because `Turn` after a draw is readable, stable, and meaningless.
- Bad, because a branch that skips advancing the turn looks like an omission.

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
- Good, because `Turn` needs no second meaning and no freeze.
- Good, because a `switch` over the outcome naturally covers every ending.
- Bad, because the enum grows with the board: three marks would mean five values.
- Bad, because "did anyone win?" becomes a two-value test rather than one comparison.
- Bad, because it splits one concept — winning — across two enum members that must be kept in step.

### The presenter reports the winner from the mark it just placed

The model says only `InProgress`/`Win`/`Draw`; the presenter, which already holds the placed mark in a local to pass to `ShowMark`, logs the winner from that.

- Good, because the model gains no ambiguity at all and `Turn` keeps one meaning.
- Good, because it requires no new state anywhere.
- Bad, because the winner exists only for the duration of one method call, so nothing that did not witness the move can ever learn it.
- Bad, because `GameCompleteState` and any result display would each have to be handed the winner by whoever happened to be watching.
- Bad, because it makes the presenter the sole authority on a fact about the game.

### A game is reset in place

The board's model, view and presenter are constructed once and returned to their starting state between games.

- Good, because nothing is allocated or destroyed to produce an object identical to the one thrown away.
- Good, because no subscription is torn down and rebuilt on a per-game cadence.
- Good, because the restart is one call, `Reset()`, which a replay button can call identically.
- Good, because `Bootstrap`'s wiring runs exactly once and is therefore exercised exactly once.
- Bad, because `Reset()` must be maintained in step with the model's fields, and a forgotten field leaks state from one game into the next.
- Bad, because "fresh" becomes a claim the code makes rather than a fact construction guarantees.

### A game is disposed and recreated

Each game gets a new `BoardModel`, `BoardView` and `BoardPresenter`, disposed when it ends.

- Good, because a new object cannot carry stale state; freshness is guaranteed by construction.
- Good, because there is no `Reset()` to keep in step with the fields.
- Bad, because it destroys and re-instantiates nine cell views and a board view that no game ever modifies.
- Bad, because every subscription in the app must be rebuilt per game, multiplying the chances of a leak or a double subscription.
- Bad, because whatever holds the presenter must swap the reference at exactly the right moment, which is the ordering fragility this project has twice designed away.
- Bad, because `Bootstrap`'s wiring would have to be split into "once" and "per game" halves.

### The model is reset and the view rebuilt by re-running construction

`BoardModel.Reset()` plus a second call to `BoardView.Construct`.

- Good, because it needs no new `Clear` method on the view.
- Good, because the view's rebuild path is exercised every game rather than only at startup.
- Bad, because `Construct` spawns a fresh set of cell views each time, so the cells are recreated even though only the marks changed.
- Bad, because it leaves the old cell views to be cleaned up, or leaks them.
- Bad, because it mixes two lifetimes — the board's and a game's — in one method.

### An app state machine with `GameplayState` and `GameCompleteState`

The machine owns the app's phase; `BoardSession` owns the board.

- Good, because "the game is over" becomes a named state rather than a local fact.
- Good, because `Loading` and `MainMenu` land later with nothing to restructure.
- Good, because the delay lives in the state named for the ending, which is also where a result banner belongs.
- Good, because each state is testable alone with two fakes.
- Bad, because it is three types and an interface introduced to serve one second of delay.
- Bad, because the app gains a level of indirection between `Bootstrap` and the board.

### `BoardPresenter` waits and restarts itself

The presenter detects the ending it already detects, waits, and clears its own model and view.

- Good, because it adds no new type at all.
- Good, because everything about a game stays in the type that already runs one.
- Bad, because the presenter would own the app's phase as well as the board's rules, and "the game is over" would again be a fact only it can see.
- Bad, because it must be handed a coroutine host, or become a `MonoBehaviour`, to wait at all.
- Bad, because the planned `MainMenu` and `Loading` phases would have nowhere to live but around it.

### The full four-state machine now

`Loading`, `MainMenu`, `Gameplay` and `GameComplete` all built.

- Good, because it matches the shape the app is eventually expected to have.
- Good, because the eventual states never need to be retrofitted.
- Bad, because two of the four would be empty pass-throughs existing only to match a diagram.
- Bad, because designing a menu flow is a separate decision that this feature does not force.

### An injected `ICoroutineService`

A `MonoBehaviour`-backed service behind an interface, returning a disposable handle.

- Good, because the one-second cycle becomes an edit-mode test that takes microseconds.
- Good, because cancellation is an object with observable state rather than a null.
- Good, because it matches how input, camera, logging and instantiation are already injected.
- Good, because it is general: any `IEnumerator`, not just a delay.
- Bad, because it is two types and an interface for something `StartCoroutine` does in one call.
- Bad, because the service must be registered as a prefab with `FactoryService`, which can be forgotten and fails only at runtime.

### `Awaitable.WaitForSecondsAsync` called directly

Unity 6's native async, awaited in the state with a `CancellationTokenSource`.

- Good, because it needs no new service, no prefab and no handle type.
- Good, because cancellation is a standard, well-understood token.
- Bad, because it needs the player loop, so the restart cycle can only be tested in play mode, one real second at a time.
- Bad, because it puts an untestable dependency on real time inside a state whose entire behaviour is about time.

### A per-frame tick accumulating `Time.deltaTime`

`GameCompleteState.Tick(deltaTime)` sums until the second elapses.

- Good, because it is fakeable in edit mode by ticking once with a large value.
- Good, because it introduces no service and no coroutine.
- Bad, because it forces every state to have a tick and a `MonoBehaviour` to drive it.
- Bad, because it re-reads unchanged state every frame to catch an event that happens once per game.

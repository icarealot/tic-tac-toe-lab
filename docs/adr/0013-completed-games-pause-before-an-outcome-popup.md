# The board owns turn and outcome, games reset in place, and completion requires acknowledgement

## Status

Accepted

## Context and Problem Statement

The app must alternate marks, determine when a game ends, preserve the completed board briefly, present the outcome, and prepare a fresh game when the player later starts one. These decisions are connected: ownership of turn and outcome determines what must be reset, while lifecycle determines who coordinates the pause, acknowledgement, navigation, and reset.

## Decision Drivers

- A rejected press must never advance the turn.
- Placement, turn advancement, and outcome evaluation should not rely on callers sequencing separate operations.
- Winner information must remain consistent with the outcome.
- A win on the last empty cell must remain a win rather than becoming a draw.
- The final mark and completed board should remain unobstructed long enough to be understood.
- A player should explicitly acknowledge whether X won, O won, or the game drew.
- Gameplay controls should not remain visible after the game has ended.
- Continue and Back should have one predictable destination once the outcome is presented.
- Starting from the main menu should be the single operation that prepares a fresh game.
- Board scaffolding does not change between games and should not be rebuilt unnecessarily.
- Reset behavior should remain reusable by future player-initiated replay.
- Timing, state transitions, and cancellation must be testable without waiting in real time.
- Long-lived subscriptions and scheduled work need explicit cleanup.

## Considered Options

**Turn and outcome ownership**

- The board model owns turn and outcome
- The presenter owns turn and reports outcomes
- Separate models own turn and outcome

**Between games**

- Reset the existing board session
- Dispose and recreate the board graph
- Reset the model but rebuild the view

**Coordination**

- App states drive a long-lived board session
- The presenter coordinates completion and restart itself
- Windows become app states

**Completed-game flow**

- Pause, then reset and return directly to gameplay
- Present the outcome popup immediately, then return to the main menu
- Pause, then present the outcome popup and return to the main menu after acknowledgement

**Waiting**

- Use an injected coroutine service
- Use Unity's asynchronous timing directly
- Accumulate time through per-frame updates

## Decision Outcome

Chosen outcome: the board model owns turn and outcome; one board session survives across games and is reset in place; app states coordinate a delayed outcome popup and navigation; starting from the main menu resets the game; and an injected coroutine service provides the delay.

### Turn and outcome belong to the board model

The turn starts as X. A legal placement uses the current turn, evaluates the board, and advances the turn only if the outcome remains in progress. After a win, the frozen turn names the winner. After a draw, the turn has no meaningful interpretation.

The presenter remains responsible for deciding whether a press may become a placement. Rejected presses and presses after completion never reach the model's placement behavior.

The board checks wins before draws, so a win on the final empty cell remains a win. It derives outcomes from the marks rather than maintaining duplicate counts or winner state.

### A game is reset in place

A game is a phase through which the same board model, view, and presenter pass. Ending a game does not end those objects' lifetimes.

Reset returns every cell to empty, the turn to X, and the outcome to in progress. The board view removes mark views while preserving its board and cell views. The presenter coordinates model and view reset so they remain synchronized.

This accepts the risk that future state can be omitted from reset. Tests must continue to establish that a reset board is equivalent to a new board.

### A board session and app states coordinate lifecycle

BoardSession is the long-lived boundary around the board model, view, and presenter. App states know the session rather than its internal parts.

GameplayState listens for the game ending and enters GameCompleteState. GameCompleteState owns the delay, outcome-popup presentation, popup cleanup, and navigation to MainMenuState. MainMenuState resets the board session only when Start is clicked, immediately before returning to gameplay.

### Completion pauses before acknowledgement

GameCompleteState leaves the completed board visible without gameplay controls for one second. Back has no effect during this pause. After the pause, an outcome popup appears over a 50% black scrim and reports `X Wins!`, `O Wins!`, or `Draw!`.

The popup remains until Continue is clicked or Back is requested. Either action closes the popup and enters MainMenuState. Clicking the scrim has no effect.

Returning to the main menu does not reset the board. The completed board remains internally populated until Start resets the existing board session, so every game begins fresh without recreating the board graph.

### Waiting is an injected mechanism

CoroutineService schedules the completed-game pause and returns a cancellation handle. This makes duration and cancellation observable in tests while keeping states independent of real elapsed time.

Scaled game time is used so a future pause of game time also pauses the pending outcome popup. Leaving GameCompleteState before the callback runs cancels the pending presentation.

### Teardown is explicit

Leaving a state cancels its scheduled work, closes windows it owns, and releases its subscriptions. Disposing the app state machine leaves the current state, and disposing the board session detaches the board collaborators. Explicit teardown avoids relying on Unity destruction order.

### Positive Consequences

- Placement, turn advancement, and outcome evaluation remain consistent.
- Winner information is derived from existing board state.
- The final move remains unobstructed during the pause.
- The outcome remains visible until acknowledged.
- Players control when they leave the completed game.
- Continue and Back converge on the same navigation behavior after the popup appears.
- Starting a fresh game remains owned by the main menu.
- Board and cell scaffolding survive between games.
- Core rules, state timing, and cancellation remain directly testable.
- Reset and future replay can share one operation.
- Lifecycle cleanup remains deliberate rather than accidental.

### Negative Consequences

- Beginning another game requires returning through the main menu.
- Reset must be maintained whenever board state grows.
- Turn has a second meaning after a win and no meaningful interpretation after a draw.
- The presenter must prevent placement after completion.
- The board model combines marks, geometry, turn, and outcome rules.
- GameCompleteState must coordinate delayed presentation and popup cleanup.
- Outcome information must cross the board-session boundary.
- The completed board remains internally populated while the main menu is shown.
- State coordination adds architectural types for a small flow.

## Pros and Cons of the Options

### Board-owned turn and outcome

- Good, because related facts change together.
- Good, because callers cannot place a mark inconsistent with the turn.
- Good, because outcome facts are available to interested features.
- Bad, because the board model has broad responsibility.
- Bad, because turn meaning depends on outcome after completion.

### Presenter-owned turn and outcome reporting

- Good, because the model remains narrower.
- Bad, because rules depend on several ordered presenter actions.
- Bad, because outcome facts are difficult for other features to inspect.

### Separate turn and outcome models

- Good, because each model has a narrow responsibility.
- Bad, because one placement must keep multiple owners synchronized.
- Bad, because inconsistent combinations become representable.

### Reset the existing board session

- Good, because unchanged board scaffolding survives.
- Good, because subscriptions do not churn each game.
- Good, because replay can use the same behavior.
- Bad, because reset can omit newly added state.

### Recreate the board graph

- Good, because construction guarantees fresh state.
- Bad, because unchanged views and subscriptions are repeatedly destroyed and rebuilt.
- Bad, because composition must be repeated correctly for every game.

### Reset the model but rebuild the view

- Good, because game state remains long-lived.
- Bad, because model and view follow different lifecycle strategies.
- Bad, because unchanged cell views are rebuilt without need.

### App states drive a board session

- Good, because app phase, completed-game timing, and navigation have explicit owners.
- Good, because future states can be added without moving board rules.
- Bad, because the architecture is larger than the immediate behavior alone requires.

### The presenter coordinates completion and restart

- Good, because fewer coordinating types are needed.
- Bad, because board presentation would also own app phase, timing, user-interface presentation, and navigation.

### Windows become app states

- Good, because one structure controls visibility and app flow.
- Bad, because a popup does not replace the underlying app phase.
- Bad, because the meaning of State would become ambiguous.

### Pause, reset, and return directly to gameplay

- Good, because another game begins without player interaction.
- Bad, because the outcome is not explicitly presented.
- Bad, because players cannot choose when to leave the completed game.

### Present the outcome popup immediately

- Good, because feedback appears with no delay.
- Good, because no scheduled presentation is needed.
- Bad, because the popup competes visually with the final mark at the instant it appears.
- Bad, because it removes the established pause on the completed board.

### Pause, present the popup, and return to the main menu

- Good, because the final move remains unobstructed during the pause.
- Good, because the outcome is explicit and persists until acknowledged.
- Good, because starting a fresh game remains owned by the main menu.
- Bad, because the flow adds an acknowledgement and menu step before another game can begin.

### Injected coroutine timing

- Good, because waiting and cancellation are observable and replaceable.
- Good, because tests need no real delay.
- Bad, because a separate service and lifecycle handle are required.

### Direct asynchronous timing

- Good, because it uses Unity's built-in mechanism directly.
- Bad, because tests depend on the player loop and real timing behavior.

### Per-frame timing

- Good, because tests can advance time explicitly.
- Bad, because every state would need update plumbing for an event that occurs once per game.

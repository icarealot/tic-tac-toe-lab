# The board owns turn and outcome, and a game is reset rather than recreated

## Status

Accepted

## Context and Problem Statement

The app must alternate marks, determine when a game ends, pause on the completed board, and begin the next game. These decisions are connected: ownership of turn and outcome determines what must be reset, while lifecycle determines who coordinates that reset.

## Decision Drivers

- A rejected press must never advance the turn.
- Placement, turn advancement and outcome evaluation should not rely on callers sequencing separate operations.
- Winner information must remain consistent with outcome.
- The completed board must stay visible during the pause before the next game.
- Board scaffolding does not change between games and should not be rebuilt unnecessarily.
- Reset behavior must be reusable by future player-initiated replay.
- Timing, state transitions and cancellation must be testable without waiting in real time.
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
- The presenter waits and restarts itself
- Build the full anticipated app-state graph immediately

**Waiting**

- Use an injected coroutine service
- Use Unity’s asynchronous timing directly
- Accumulate time through per-frame updates

## Decision Outcome

Chosen outcome: the board model owns turn and outcome; one board session survives across games and is reset in place; app states coordinate the completed-game pause through an injected coroutine service.

### Turn and outcome belong to the board model

The turn starts as X. A legal placement uses the current turn, evaluates the board and advances the turn only if the outcome remains in progress. After a win, the frozen turn names the winner. After a draw, the turn has no meaningful interpretation.

The presenter remains responsible for deciding whether a press may become a placement. Rejected presses and presses after completion never reach the model’s placement behavior.

The board checks wins before draws, so a win on the final empty cell remains a win. It derives results from the marks rather than maintaining duplicate counts or winner state.

### A game is reset in place

A game is a phase through which the same board model, view and presenter pass. Ending a game does not end those objects’ lifetimes.

Reset returns every cell to empty, the turn to X and the outcome to in progress. The board view removes mark views while preserving its board and cell views. The presenter coordinates model and view reset so they remain synchronized.

This accepts the risk that future state can be omitted from reset. Tests must continue to establish that a reset board is equivalent to a new board.

### A board session and app states coordinate lifecycle

BoardSession is the long-lived boundary around the board model, view and presenter. App states know the session rather than its internal parts.

GameplayState listens for the game ending. GameCompleteState owns the pause, requests a reset and returns the app to gameplay. Only states needed by current behavior are introduced; anticipated menu and loading states are deferred until they have responsibilities.

### Waiting is an injected mechanism

CoroutineService schedules the completed-game pause and returns a cancellation handle. This makes duration and cancellation observable in tests while keeping states independent of real elapsed time.

The reset happens after the pause, immediately before gameplay resumes, so the finished board remains visible for the entire delay. Scaled game time is used so a future pause of game time also pauses the pending reset.

### Teardown is explicit

Leaving a state cancels its scheduled work and releases its subscriptions. Disposing the app state machine leaves the current state, and disposing the board session detaches the board collaborators. Explicit teardown avoids relying on Unity destruction order.

### Positive Consequences

- Placement, turn advancement and outcome evaluation remain consistent.
- Winner information is derived from existing board state.
- Core rules are directly testable without a scene.
- Board and cell scaffolding survive between games.
- Automatic reset and future replay can share one operation.
- State timing and cancellation are testable without real delays.
- Lifecycle cleanup is deliberate rather than accidental.

### Negative Consequences

- Reset must be maintained whenever board state grows.
- Turn has a second meaning after a win and no meaningful interpretation after a draw.
- The presenter must prevent placement after completion.
- The board model combines marks, geometry, turn and outcome rules.
- State coordination adds architectural types for a small current flow.
- A secondary or preview board carries game state unless modeled separately.

## Pros and Cons of the Options

### Board-owned turn and outcome

- Good, because related facts change together.
- Good, because callers cannot place a mark inconsistent with the turn.
- Good, because outcome is readable by any interested feature.
- Bad, because the board model has broad responsibility.
- Bad, because turn meaning depends on outcome after completion.

### Presenter-owned turn and outcome reporting

- Good, because the model remains narrower.
- Bad, because rules depend on several ordered presenter actions.
- Bad, because outcome facts are difficult for other features to inspect.

### Reset the existing board session

- Good, because unchanged board scaffolding survives.
- Good, because subscriptions do not churn each game.
- Good, because replay can use the same behavior.
- Bad, because reset can omit newly added state.

### Recreate the board graph

- Good, because construction guarantees fresh state.
- Bad, because unchanged views and subscriptions are repeatedly destroyed and rebuilt.
- Bad, because composition must be repeated correctly for every game.

### App states drive a board session

- Good, because app phase and completed-game timing have explicit owners.
- Good, because future states can be added without moving board rules.
- Bad, because the architecture is larger than the immediate behavior alone requires.

### The presenter restarts itself

- Good, because fewer coordinating types are needed.
- Bad, because board presentation would also own app phase and timing.

### Injected coroutine timing

- Good, because waiting and cancellation are observable and replaceable.
- Good, because tests need no real delay.
- Bad, because a separate service and lifecycle handle are required.

### Direct asynchronous timing

- Good, because it uses Unity’s built-in mechanism directly.
- Bad, because tests depend on the player loop and real timing behavior.

### Per-frame timing

- Good, because tests can advance time explicitly.
- Bad, because every state would need update plumbing for an event that occurs once per game.

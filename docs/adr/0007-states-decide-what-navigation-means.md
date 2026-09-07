# States decide what navigation means; UIService only performs it

## Status

Accepted

## Context and Problem Statement

The app has user-interface stacks and mobile back input. Something must hold windows, decide what back means in the current situation and prevent board interaction while blocking user interface is present. Should navigation meaning belong to app states, UIService or a unified state-and-window system?

## Decision Drivers

- The state machine is already the authority on what the app is doing.
- Back means different things in different app phases.
- A popup over gameplay does not replace gameplay as the current phase.
- Arithmetic board hit-testing does not receive automatic user-interface blocking.
- Services in this project provide mechanisms rather than app policy.

## Considered Options

- States own navigation meaning; UIService provides mechanism
- UIService owns navigation policy
- Windows become app states

## Decision Outcome

Chosen option: “states own navigation meaning; UIService provides mechanism,” because it keeps one authority on app phase while allowing each phase to interpret back differently.

UIService shows, hides, stacks and returns windows when instructed. It does not listen for back input or decide why a window should close.

The state machine forwards back input to the current state. Gameplay closes an existing popup first; without a popup, it opens a quit confirmation. The gameplay panel itself is not removed by back unless gameplay policy explicitly chooses to do so.

Blocking user interface is paired with disabling gameplay input. User-interface input remains active, so buttons still work while board presses stop. The state that opens blocking user interface also owns restoring input and cleaning up its windows when leaving.

Popups report choices through callbacks supplied by the state. They do not navigate independently.

### Positive Consequences

- App phase has one authority.
- Back can have a meaning appropriate to each state.
- Board interaction remains unaware of user-interface concerns.
- UIService remains a mechanism whose stack rules are independently testable.
- Popups do not acquire hidden navigation authority.

### Negative Consequences

- Back behavior is distributed across states.
- Every state must define or inherit a back response.
- Forgetting to restore gameplay input can leave the board unresponsive.
- Forgetting window cleanup can leak user interface into the next state.
- Correct cleanup depends on state discipline rather than structural enforcement.

## Pros and Cons of the Options

### States own navigation meaning

- Good, because current app phase remains authoritative.
- Good, because different states can interpret back differently.
- Good, because UIService stays a mechanism.
- Bad, because navigation behavior is not centralized.
- Bad, because input and window cleanup require discipline.

### UIService owns navigation policy

- Good, because back behavior is centralized.
- Good, because states contain less navigation logic.
- Bad, because one universal rule cannot express every state’s intent.
- Bad, because exceptions would make UIService a second authority on app phase.

### Windows become app states

- Good, because one structure controls both visibility and back.
- Bad, because a popup does not actually replace the app phase beneath it.
- Bad, because purely visual changes would become state transitions.
- Bad, because the meaning of State would become ambiguous.

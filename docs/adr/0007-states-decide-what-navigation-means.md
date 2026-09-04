# The states decide what navigation means; the UI service only shows and hides

## Status

Accepted

## Context and Problem Statement

The project is growing a UI service that shows, hides and stacks windows, and the app targets mobile, where a hardware back gesture must do something sensible at every moment. Something has to hold the stacks, something has to decide what back means, and something has to stop a press landing on the board while a window covers it.

The app already has an `AppStateMachine` in which "exactly one state is current at a time, and a state is entered and left rather than created and destroyed" — structurally the same shape as a window stack, minus the stack and minus back. Adding a UI service that also knows what the app is doing would create a second authority on the same question. Which of the two decides?

## Decision Drivers

- The state machine is already the app's answer to "what is the app doing now?", and `CONTEXT.md` defines a **State** as "one phase of the app". A second thing claiming that role would make the answer ambiguous.
- Back is not one behaviour. Back during a game should ask before abandoning it; back on a settings popup should just close it; back at the root should quit the app. Any rule fixed inside a service is wrong for at least one of these.
- A popup over gameplay is not a new phase of the app. The game is still in `GameplayState` — it is merely covered — so window visibility and app phase are genuinely different things and cannot be merged without lying about one of them.
- [ADR 0002](./0002-arithmetic-hit-testing-without-colliders.md) resolves presses arithmetically with no `EventSystem` in the play path, and explicitly records that "the first overlay this project grows will need an explicit guard". That guard has to be placed somewhere, and its placement is part of this decision.
- The existing services — `CoroutineService`, `FactoryService`, `CameraService`, `InputService`, `LogService` — are uniformly mechanism. None of them decides anything about the app.

## Considered Options

- States own navigation; `UIService` is mechanism
- `UIService` owns navigation; states cover game flow only
- Windows are states; the state machine grows a stack and a back handler

## Decision Outcome

Chosen option: "states own navigation; `UIService` is mechanism", because it is the only option that keeps one authority on what the app is doing while still letting each phase of the app give back a different meaning.

`UIService` creates, shows, hides, stacks and destroys windows and does nothing else. It never listens to input, never decides what back means, and never closes a window on its own. Its surface is imperative and total: show a panel, show a popup, try to close the top popup, try to close the top panel, close all popups, and ask whether a popup is up.

`IAppState` gains `Back()`. `AppStateMachine` subscribes to `IInputService.BackPressed` and forwards it to whichever state is current. A state then composes the service's mechanism into the meaning it wants:

```csharp
// GameplayState — back asks before abandoning the game
public void Back()
{
    if (_uiService.TryClosePopup())
    {
        return;
    }

    _uiService.ShowPopup<ConfirmQuitPopup>(popup => popup.Setup(
        "Quit the game?",
        onYes: GoToMenu,
        onNo: CloseTopPopup));
}
```

`GameplayPanel` is never popped by back, because `GameplayState` simply never calls `TryClosePanel`.

The same principle places ADR 0002's missing guard. `IInputService` gains `Enable()` and `Disable()` over the `Player` action map, and the state that puts up blocking UI disables gameplay input for as long as it is up. Because uGUI's `InputSystemUIInputModule` drives itself from its own action set, disabling `Player` stops board presses while leaving buttons fully working. `BoardPresenter` does not change and gains no knowledge of the UI.

Popups report their outcome through callbacks handed to them at configure time; they never call the UI service themselves. A popup that could close itself would be navigating, and navigation belongs to the state.

### Positive Consequences

- There is exactly one authority on what the app is doing, and it is the one that was already there. `CONTEXT.md`'s definition of **State** survives intact.
- Back is expressive at no cost. Intercepting it, ignoring it, or letting it pop are all ordinary code in the one place that knows which is right.
- ADR 0002's outstanding guard is closed, and closed without coupling: the board keeps resolving presses arithmetically and simply stops being told about them, so `BoardPresenter` and `BoardPresenterTests` are untouched.
- Disabling the action map is robust in a way an `EventSystem.IsPointerOverGameObject` check is not — no pointer-id subtleties under touch, and no leak through a popup's transparent margins.
- `UIService` stays a plain class with no input dependency, so its stack rules are testable in EditMode with no scene.

### Negative Consequences

- Every state implements `Back()`, including states with nothing to say. A default interface implementation reduces this to a formality but does not remove it.
- Back behaviour is distributed across states rather than readable in one file. Answering "what does back do here?" means knowing which state is current.
- A state that forgets to re-enable gameplay input leaves the board dead with no visible cause. The symptom — presses silently doing nothing — is the hardest kind to trace, and nothing detects it.
- Whether a state disables input is a per-state judgement, so two states can reasonably disagree about the same situation and neither is wrong by the rules.
- A state that shows a window and forgets to clean up on `Leave()` leaks it into the next phase. Nothing enforces the pairing.

## Pros and Cons of the Options

### States own navigation; `UIService` is mechanism

The state machine forwards back to the current state, which calls mechanism on the service. The service holds the stacks and knows nothing about why.

- Good, because the app has one authority on its own phase.
- Good, because each phase can give back a different meaning, which is what mobile back actually requires.
- Good, because it matches every other service in the project, all of which are mechanism.
- Good, because it places ADR 0002's guard without coupling the board to the UI.
- Bad, because back behaviour is spread across states rather than centralised.
- Bad, because input re-enabling is a discipline with no enforcement.

### `UIService` owns navigation; states cover game flow only

Back is handled inside the service: close the top popup, else pop the top panel, else quit. States are untouched.

- Good, because back is one rule in one place, easy to read and easy to test.
- Good, because states need no `Back()` and no navigation code at all.
- Bad, because "back always pops" is wrong for gameplay, where abandoning a game unconfirmed is a worse outcome than any implementation cost.
- Bad, because the first exception to the rule pushes state-awareness into the service, at which point there are two authorities anyway and one of them is hidden.
- Bad, because the service would have to listen to input, making it the only service in the project that observes the world rather than being told about it.

### Windows are states; the state machine grows a stack and a back handler

Every window is an `IAppState`; the state machine gains a stack and back semantics. One authority, one type.

- Good, because there is only one concept and one stack to reason about.
- Good, because back is uniform and defined once.
- Bad, because a pause popup over gameplay is not a new phase — the game is still in `GameplayState`, so the merge misrepresents what is happening.
- Bad, because it forces a state transition for every visual change, including ones with no phase meaning at all.
- Bad, because `CONTEXT.md` would have to redefine **State** to mean two different things, which is exactly the ambiguity the glossary exists to prevent.

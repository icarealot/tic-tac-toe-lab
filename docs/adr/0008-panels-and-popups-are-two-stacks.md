# Panels and popups are two stacks, and showing a panel over a popup throws

## Status

Accepted

## Context and Problem Statement

Windows come in two kinds. A **panel** covers the whole display; a **popup** floats above the panels without hiding them. Both are shown, hidden and closed by `UIService`, and both take part in back navigation, which needs a single unambiguous answer to "what is on top?".

If panels and popups share one stack, showing a panel while a popup is open puts a full-covering window above a dialog that was meant to float. If they are separate stacks, that ordering is impossible by construction — but the combination is still meaningless, and the service has to do something when asked for it. What structure holds the windows, and what happens when a caller asks for something the structure cannot express?

## Decision Drivers

- Every popup renders above every panel. This is not a preference; it is what makes a popup a popup, and it holds for every window the project will ever have.
- A structural invariant cannot be violated by calling methods in the wrong order. A documented rule can.
- The race is real and is not a coding mistake: a player opens a pause popup, and while it is up the game ends, so `GameplayState` transitions to `GameCompleteState`, which wants to show a panel. Two independent things happened at once.
- [ADR 0007](./0007-states-decide-what-navigation-means.md) puts navigation in the states. Anything the service does automatically on a caller's behalf takes a decision away from the state that owns it.
- The project already treats a call that cannot be honoured as a programmer error: `ComponentFactoryService.ResolveComponent` throws for an unregistered prefab, and `AppStateMachine.ChangeState` throws for an unknown state. Both throw `InvalidOperationException`.

## Considered Options

- Two stacks, and showing a panel over a popup throws
- Two stacks, and showing a panel over a popup is a logged no-op
- Two stacks, and showing a panel closes the popups first
- One mixed stack of windows

## Decision Outcome

Chosen option: "two stacks, and showing a panel over a popup throws", because the structure then makes the ordering invariant unbreakable, and the throw makes the one remaining incoherent request impossible to miss.

`UIService` holds a panel stack and a popup stack. The two live under separate layer transforms beneath a single root canvas, with the popup layer a later sibling than the panel layer, so every popup is drawn above every panel as a fact about the hierarchy rather than as arithmetic the service has to get right. `Panel` and `Popup` both derive from `Window`, and the generic constraints on `ShowPanel<TPanel>` and `ShowPopup<TPopup>` make pushing a popup onto the panel stack a compile error.

Within each stack only the top window is shown. Showing a panel hides the panel beneath it; showing a popup hides the popup beneath it but never the panel beneath it. Hiding means the window stays alive with its `CanvasGroup` faded out and its raycasts blocked, keeping its configuration so that a revealed window needs no reconfiguring; closing destroys it through `UIFactoryService.Return`.

Back consults the popup stack first and the panel stack only if the popup stack is empty — but, per ADR 0007, back is a state's decision, and the service only offers `TryClosePopup` and `TryClosePanel` as mechanism.

Calling `ShowPanel` while the popup stack is non-empty throws `InvalidOperationException`. Because a throw a caller cannot avoid is hostile, the service also exposes `HasPopup` and `CloseAllPopups`, and the intended discipline is that a state clears its popups in `Leave()` before a transition completes, so the throw never fires in the race above.

### Positive Consequences

- The ordering invariant is enforced by the hierarchy and the type system rather than by the service's own correctness, so no bug in `UIService` can produce a panel drawn over a popup.
- "What is on top?" has one answer at all times, which is what makes back navigation well defined.
- Showing a panel while a popup is open is well defined for the *other* direction too: a panel can swap in underneath while the popup floats on, which single-stack designs cannot express.
- The throw surfaces missing cleanup at the moment it happens, in the editor, rather than as a wedged screen on a device.
- The service is a plain class holding two stacks of live instances, so every rule here is exercisable in EditMode with no scene and no play mode.

### Negative Consequences

- **The race can crash the app.** If a state neglects to clear its popups before a transition that shows a panel, the throw happens at runtime, in a situation the player caused rather than the programmer. The failure is loud, which is the point, but it is a failure.
- The discipline the throw depends on — clear popups in `Leave()` — is a convention with no enforcement, exactly the kind ADR 0007 already relies on for input re-enabling.
- The service surface is larger than the minimum: `HasPopup` and `CloseAllPopups` exist only because the throw obliges them.
- Two stacks means two of everything to reason about, and "the topmost window" is a two-step answer rather than a lookup.
- Every window below the top of its stack stays alive and faded rather than deactivated, so a deep stack holds every window in it. At `alpha = 0` a window still runs and still submits geometry; the saving is the layout rebuild on reveal, not the per-frame cost.

## Pros and Cons of the Options

### Two stacks, and showing a panel over a popup throws

Separate panel and popup stacks under separate layers; the illegal combination raises `InvalidOperationException`.

- Good, because the ordering invariant is structural and cannot be broken by a bug.
- Good, because it matches how `ComponentFactoryService` and `AppStateMachine` already treat unhonourable calls.
- Good, because a wedged UI is never produced silently.
- Bad, because a legitimate player-caused race becomes a runtime exception if a state forgot its cleanup.
- Bad, because it forces two extra members onto the service so callers can avoid the throw.

### Two stacks, and showing a panel over a popup is a logged no-op

The service refuses via `ILogService` and continues.

- Good, because nothing crashes, whatever the timing.
- Good, because the service surface stays minimal.
- Bad, because the app ends up in the new state with a stale popup on screen and no new panel — a wedge with no visible cause, which is worse than the crash it avoids.
- Bad, because the log line is the only trace, and on a player's device there is no log.

### Two stacks, and showing a panel closes the popups first

The service reads the call as an override, tears down the popup stack, and proceeds.

- Good, because it never fails and never wedges.
- Good, because it removes the cleanup discipline entirely.
- Bad, because it guesses what the caller meant, and the guess is wrong whenever the popup mattered — a confirm dialog silently vanishing is a lost decision, not a tidy-up.
- Bad, because it makes the service navigate, which is the state's job under ADR 0007.

### One mixed stack of windows

Panels and popups share a stack; back pops whatever is topmost.

- Good, because back is one uniform rule over one structure.
- Good, because there is a single, trivially readable answer to "what is on top?".
- Bad, because it admits a panel above a popup, which is exactly the state the design exists to prevent.
- Bad, because preventing it needs a guard rule the structure cannot express, so the invariant depends on the service being correct.
- Bad, because it cannot express a panel swapping in underneath an open popup.

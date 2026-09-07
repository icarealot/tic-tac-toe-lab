# Presses are resolved to cells arithmetically, without colliders

## Status

Accepted

## Context and Problem Statement

A completed press must resolve to a cell or to no cell. The board is a uniform grid whose geometry is already known. Should hit-testing use that geometry directly or delegate the answer to Unity collision and event systems?

## Decision Drivers

- A uniform board has an exact inverse mapping from a local point to a cell.
- ADR 0001 requires the row and vertical-axis relationship to have one authority.
- Board rules and geometry should be testable without a scene.
- Duplicate visual and collision geometry can drift apart.

## Considered Options

- Arithmetic hit-testing owned by the board model
- Event-system raycasting with per-cell colliders
- A direct physics query

## Decision Outcome

Chosen option: “arithmetic hit-testing owned by the board model,” because it keeps board geometry in one place and makes boundary behavior independently testable.

A press moves through a sequence of coordinate spaces. Input reports a screen point, the camera converts it to a world point, the board view converts it to a local point, and the board model resolves that point to a cell address or no cell.

The board model owns cell size and spacing. A point in the spacing between cells, or beyond the board’s outer edge, resolves to no cell. Colliders and physics raycasters are not part of board interaction.

### Positive Consequences

- Layout and hit-testing share one geometric authority.
- Boundary and spacing cases are fast to test without a scene.
- The scene needs no per-cell collision setup.
- Gameplay rules remain independent of Unity objects.

### Negative Consequences

- User-interface elements do not automatically block board presses.
- Hover and pointer-highlight behavior require explicit work.
- Every press must be converted into board-local space before resolution.
- Visible cell dimensions and model geometry can disagree if assets change independently.
- An irregular or animated board would invalidate the uniform-grid assumption.

## Pros and Cons of the Options

### Arithmetic hit-testing owned by the board model

- Good, because geometry has one authority.
- Good, because the result is testable without Unity physics.
- Good, because spacing can deliberately reject a press.
- Bad, because input blocking and hover behavior are not provided automatically.
- Bad, because it assumes a uniform board.

### Event-system raycasting with per-cell colliders

- Good, because Unity supplies pointer and hover behavior.
- Good, because user-interface blocking follows the same event system.
- Bad, because collider dimensions duplicate board geometry.
- Bad, because meaningful tests require a configured scene.

### A direct physics query

- Good, because collider shapes can differ from visible sprites.
- Bad, because it retains duplicate geometry without gaining the full event-system behavior.
- Bad, because it couples board interaction to the physics scene.

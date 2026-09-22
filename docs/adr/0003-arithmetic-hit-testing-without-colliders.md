# Board presses use arithmetic hit-testing without colliders

## Status

Accepted

## Context and Problem Statement

A completed board press must resolve to one cell or no cell. The board is a fixed, uniform layout whose cell size and spacing are already known. Should interaction invert that layout arithmetically, or rely on Unity collision or event-system objects for each cell?

## Decision Drivers

- Layout and hit-testing must agree at cell edges and within spacing.
- Point-to-cell behavior should be deterministic and testable in EditMode.
- Per-cell collision geometry would duplicate known board geometry.
- Cell addressing must follow ADR 0002 in every input path.
- The project needs only a uniform 3×3 board.

## Considered Options

- Arithmetic hit-testing owned by board layout
- Event-system raycasting against per-cell targets
- Physics queries against per-cell colliders

## Decision Outcome

Chosen option: "arithmetic hit-testing owned by board layout," because one deterministic calculation can generate cell positions and invert those positions during interaction.

Input supplies a screen point. The presenter uses the camera and board view to convert it to a board-local point, then asks `BoardLayout` to resolve that point to a `CellCoordinate`. Points in spacing or beyond the board resolve to no cell. Board interaction uses neither per-cell colliders nor event-system raycasting.

### Positive Consequences

- Layout and hit-testing share one geometric authority.
- Boundary, spacing, and outside-board cases have fast deterministic tests.
- The scene needs no per-cell collision setup.
- Game rules receive cell coordinates rather than Unity collision objects.

### Negative Consequences

- User-interface blocking of board input must be controlled explicitly.
- Every press requires screen-to-world-to-board-local conversion.
- Irregular cells or animated layout geometry would require a different approach.
- Visual assets can disagree with layout constants if production wiring is not validated.

## Pros and Cons of the Options

### Arithmetic hit-testing owned by board layout

- Good, because generation and inverse mapping use the same dimensions.
- Good, because no scene or physics world is required to test the calculation.
- Good, because spacing can deliberately reject a press.
- Bad, because the method assumes a regular board.
- Bad, because input blocking and hover behavior are not automatic.

### Event-system raycasting against per-cell targets

- Good, because Unity supplies pointer targeting and user-interface blocking.
- Good, because hover behavior can use the same targets.
- Bad, because target rectangles duplicate board layout knowledge.
- Bad, because meaningful tests require configured Unity objects.

### Physics queries against per-cell colliders

- Good, because collider shapes may differ from visual bounds.
- Bad, because collision geometry duplicates the uniform layout.
- Bad, because board interaction becomes dependent on the physics scene.
- Bad, because it does not provide the event system's full interaction behavior.

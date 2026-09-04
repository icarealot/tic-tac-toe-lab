# Presses are resolved to cells arithmetically, without colliders

## Status

Accepted

## Context and Problem Statement

A press has to become a cell coordinate. The board is nine sprites laid out on a uniform grid, and the project is configured for the Input System package only, so `OnMouseDown` and the legacy `Input` class are both unavailable. Should the project let Unity answer "which cell was pressed?" through colliders and the `EventSystem`, or compute the answer from the board's own geometry?

## Decision Drivers

- The board is a perfectly uniform grid, so the mapping from a point to a cell coordinate has an exact closed form — it is the inverse of the expression that lays the cells out.
- [ADR 0001](./0001-row-column-cell-coordinates.md) commits the project to keeping the row/world-Y inversion in exactly one place; a second copy of the board's geometry is precisely the failure it guards against.
- The project is built test-first, and the `EditModeTests` assembly can only exercise logic that does not need a scene, a camera, or play mode.
- The rules of the game are being kept free of `UnityEngine` types wherever it costs nothing.

## Considered Options

- Arithmetic hit-testing owned by `BoardModel`
- `EventSystem` raycasting with `Collider2D` on each cell
- A physics query (`Physics2D.OverlapPoint`) driven by an input service

## Decision Outcome

Chosen option: "arithmetic hit-testing owned by `BoardModel`", because it is the only option where the board's geometry exists once and can be verified without pressing play.

The press pipeline is a chain of narrow conversions, each owned by the one type that has the knowledge for it. `InputService` reports a press on release, as a screen point. `CameraService`, which owns the camera, turns that into a world point. `BoardView`, which owns the board's transform, turns that into a local point. `BoardModel` resolves the local point to a cell coordinate, or to nothing at all.

`BoardModel` holds the cell size and the spacing as well as the marks, so it is the single authority on where cells are and which one contains a given local point. A press landing in the spacing between two cells resolves to no cell at all, and so does a press outside the board's outer edge.

There are no `Collider2D` components on cells, no `EventSystem` in the scene, and no `Physics2DRaycaster` on the camera. Their absence is deliberate.

### Positive Consequences

- The board's geometry is written once. Cell layout and cell hit-testing are the same arithmetic read in opposite directions, in the same type, so they cannot drift apart.
- Hit-testing is a pure function over plain numbers, so its edge cases — the boundary between a cell and the spacing, a point just outside the board — are covered by fast EditMode tests with no scene at all.
- The scene stays minimal: no `EventSystem`, no raycaster, no colliders to keep in sync with sprite sizes.
- `BoardPresenter` depends only on interfaces and never touches a Unity object, so the rules it enforces are unit-testable.

### Negative Consequences

- **Nothing blocks a press.** With no `EventSystem`, a UI panel drawn over the board will not absorb presses — they pass straight through to the cell beneath. The first overlay this project grows will need an explicit guard, and this is the consequence most likely to be discovered as a bug.
- Per-cell pointer behaviour that the `EventSystem` gives away — hover, enter/exit, press-highlight — has to be built by hand, and needs the pressed cell resolved on move as well as on release.
- The board's transform must be inverted by hand (`BoardView.ToLocal`) before the model sees a point. Forgetting that step fails silently: marks land in the wrong cells only once the board is moved or scaled.
- Cell size and spacing live in the model while the drawn size of a cell lives in the `CellView` prefab. If the prefab is re-authored at a different size, the visible cells and the pressable cells disagree, and nothing in code detects it.
- The approach depends on the board being a uniform grid. Any irregular layout — a differently shaped board, per-cell offsets, an animated cell — invalidates the closed form and pushes the project back towards colliders.

## Pros and Cons of the Options

### Arithmetic hit-testing owned by `BoardModel`

A local point is divided by the distance between cell centers to yield a cell coordinate, then checked against that cell's extent so that the spacing rejects the press.

- Good, because the geometry has exactly one home.
- Good, because it is testable in EditMode, with no scene, camera or play mode.
- Good, because it adds no components, no scene objects and no physics.
- Bad, because no UI can block a press.
- Bad, because hover and highlight cost extra work rather than coming for free.
- Bad, because it assumes a uniform grid forever.

### `EventSystem` raycasting with `Collider2D` on each cell

Each `CellView` carries a `BoxCollider2D` and implements `IPointerClickHandler`; a `Physics2DRaycaster` on the camera and an `EventSystem` with `InputSystemUIInputModule` in the scene resolve the hit.

- Good, because it is the idiomatic Unity answer and needs no maths.
- Good, because hover, enter, exit and press states come for free.
- Good, because UI correctly blocks presses aimed at the board.
- Bad, because the collider's size is a second statement of a cell's geometry, which can silently disagree with the layout.
- Bad, because the answer is only reachable in play mode, so the arithmetic ADR 0001 flags as error-prone stays untested.
- Bad, because it adds three pieces of scene and prefab wiring for a nine-cell static grid.

### A physics query driven by an input service

`InputService` converts the press to a world point and calls `Physics2D.OverlapPoint` to find the cell.

- Good, because input stays in one service, matching `ILogService` and `IFactoryService`.
- Good, because colliders can be shaped independently of sprites.
- Bad, because it keeps the colliders and their duplicated geometry while giving up the `EventSystem` features that justify them.
- Bad, because it still needs play mode to test.
- Bad, because the input service grows a dependency on the physics scene.

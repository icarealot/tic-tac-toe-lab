# Testing Standards

Review tests according to behavior risk, not test volume.

- **Risk, not volume** — Every retained test protects a named game rule, calculation, meaningful branch, state change, lifecycle, infrastructure contract, external-service contract, or critical player journey. Test count, coverage, and a bug's history are not sufficient justification.
- **Priority** — Favor deterministic business logic. Test failure-sensitive infrastructure and external contracts. Test glue only when it owns meaningful branching, coordination, state, or lifecycle behavior. Substitute third-party services in unit tests; use a real integration only when a critical behavior cannot be established through the service contract.
- **Smallest sufficient fixture** — Use plain EditMode with no `GameObject`, asset, frame, timing, or lifecycle dependency first; then an isolated PlayMode `GameObject`; then a production prefab only for serialized configuration; then a production scene only for bootstrap or cross-object production wiring.
- **Owning seam** — Assert public observable behavior primarily at the layer that owns the rule. Avoid repeating the same outcome at every architectural layer.
- **Independent oracle** — Derive expected values from rules, worked examples, or known-good literals rather than reproducing the implementation calculation.
- **Focused assertions** — Exclude private sequencing, incidental collaborator call counts, diagnostic text, styling, layout, and hierarchy unless they are the contract. Assert ordering only when ordering is itself behavior, and avoid repeated round trips once one case establishes the lifecycle invariant. Parameterize equivalent cases.
- **Qualitative boundary** — Use human playtests for feel, visuals, audio, controls, camera behavior, usability, and level design rather than treating automation as proof of quality.
- **Scene discipline** — Each production-scene test identifies a distinct bootstrap or wiring risk. Extend an existing critical journey when readable rather than adding another scene load.

A missing test is a finding only when the changed behavior clears the risk rule. Name that unprotected behavior and its cheapest sufficient fixture; never ask for tests generically.

## Context

The codebase already has a working inventory subsystem centered on `Inventory`, `InventoryStock`, and `InventoryTransaction`, and it already has campaign-scoped `ReliefPackageDefinition` templates. However, relief package definitions are currently only planning templates. They do not create package stock in warehouse, do not consume real component stock, and do not provide an assembly audit trail.

The desired workflow is warehouse-centric:
- a relief package definition describes what a package contains
- each package definition points to an output supply item that represents the packaged result in stock
- a station inventory can calculate how many packages are assemblable from current component stock
- a moderator or authorized warehouse operator can assemble N packages
- assembly consumes loose component stock and imports the package output item into inventory
- assembly history records who assembled the package, when, where, how many packages were produced, and which component supply items were consumed

The design should reuse the existing inventory transaction backbone and keep assembled packages visible as normal stock items so downstream transfer and delivery flows need minimal change.

## Goals / Non-Goals

**Goals:**
- Treat assembled relief packages as normal supply items in warehouse stock.
- Keep `ReliefPackageDefinition` as the source of package composition.
- Add a package-assembly workflow that consumes component stock and increases package-item stock.
- Preserve an explicit assembly history separate from generic inventory transactions.
- Support a “maximum assemblable quantity” calculation based on current inventory stock.
- Prevent package-category supply items from being used as package-definition components.

**Non-Goals:**
- Full batch/lot package tracking, labeling, or barcode support.
- Point-level package reservations for households.
- Automatic stock deduction on household delivery completion in this change.
- Unit-conversion logic across different measurement systems; this change assumes supply items are stored using consistent smallest units.
- Package-to-package nesting.

## Decisions

### 1. Use normal supply items as the stock representation of assembled packages

**Decision:** An assembled relief package will be stored in inventory as a normal `SupplyItem` record rather than a separate package-stock subsystem.

**Rationale:**
- The current inventory and transfer subsystems already operate entirely on `SupplyItem` and `InventoryStock`.
- Reusing that model minimizes downstream changes for stock display, stock movement, and shortage workflows.
- This aligns with the desired operational UI where warehouse staff can see ordinary supply items and assembled package items in the same inventory view.

**Alternatives considered:**
- Create a separate package-stock subsystem: rejected because it would duplicate stock movement concepts already implemented in inventory.
- Treat package definitions as stock directly: rejected because definitions are templates, not warehouse quantities.

### 2. Keep package definition and package stock separate but logically connected

**Decision:** `ReliefPackageDefinition` remains the package template, while a dedicated output `SupplyItem` represents the package as inventory stock. The definition stores the output package supply item reference.

**Rationale:**
- This preserves the distinction between “what a package contains” and “what stock item is stored after assembly”.
- Using an explicit output supply item avoids fragile lookup-by-name behavior.
- It allows package stock to move through existing inventory and transfer flows without changing their core model.

**Alternatives considered:**
- Match package definition to output supply item by name only: rejected because name-based coupling is brittle.
- Store package output item only in assembly request: rejected because the definition should be self-describing for repeatable warehouse operations.

### 3. Model assembly as an auditable warehouse workflow above inventory transactions

**Decision:** Add package-assembly log entities to capture each assembly execution and the component quantities consumed.

**Rationale:**
- Inventory transactions alone can show stock movement but not the business context of a package assembly action.
- A dedicated assembly log can answer operational questions such as which station assembled packages on a given day and who performed the work.
- The assembly log can link business intent to the inventory transactions used for stock changes.

**Alternatives considered:**
- Use only transaction notes for audit: rejected because it weakens queryability and explicit audit semantics.

### 4. Consume component stock and import package stock through existing transaction service

**Decision:** Package assembly will reuse `InventoryTransactionService` to create stock movements instead of updating `InventoryStock` directly.

**Rationale:**
- The codebase already centralizes quantity mutation and validation in inventory transactions.
- Reusing the service keeps assembly stock changes consistent with procurement, transfer, and allocation behavior.
- This preserves inventory traceability and avoids a second mutation path.

**Alternatives considered:**
- Directly update `InventoryStock`: rejected because it bypasses the stock ledger and duplicates validation logic.

### 5. Forbid package-category items as package-definition components

**Decision:** Package definitions may only reference non-package supply items as components.

**Rationale:**
- This avoids package nesting and keeps package assembly logic simple.
- The current design goal is to build packages from basic inventory items stored in smallest units.

**Alternatives considered:**
- Allow nested packages: rejected as unnecessary complexity for MVP.

## Risks / Trade-offs

- **[Risk] Package output items and package definitions can drift** → Mitigation: store `OutputSupplyItemId` on the package definition and validate it on create/update.
- **[Risk] Assembly may fail midway if stock changes are not atomic** → Mitigation: perform component export and package import through the current transaction service within a controlled workflow.
- **[Risk] Category-based package filtering can be bypassed if validation exists only in UI** → Mitigation: enforce server-side validation that package-category supply items cannot be selected as package components.
- **[Risk] Maximum assemblable quantity can become stale quickly in concurrent warehouse operations** → Mitigation: treat it as advisory and revalidate stock at assembly execution time.
- **[Risk] Unit mismatch between supply items and package definition items** → Mitigation: this change assumes smallest-unit storage and should document that package quantities must match the stored unit semantics.

## Migration Plan

1. Extend `ReliefPackageDefinition` with an output package supply item reference.
2. Add package assembly log entities and relationships.
3. Add inventory transaction reason(s) needed for package assembly audit clarity.
4. Add service and API support for package assembly planning and execution.
5. Validate that assembled package supply items remain compatible with existing inventory listing and transfer flows.

Rollback strategy:
- Because this change is additive, rollback can disable package assembly endpoints and revert the migration if package assembly data has not become operationally critical.

## Open Questions

- Should package assembly use two explicit transactions (component export + package import) or one business operation that writes both under one assembly header?
- Does the first release need assembly cancellation/reversal, or only forward assembly execution?
- Should package output supply items be created manually in supply management or optionally scaffolded from package-definition setup?

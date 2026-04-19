## Context

The current backend models stock only at station inventory level through `Inventory` and `InventoryStock`, while business workflows such as `SupplyAllocation`, package assembly, and household delivery operate at campaign level. Approved supply allocations currently export stock from a source station inventory without crediting any persisted campaign-owned stock balance, and campaign inventory balance endpoints only aggregate active station inventory snapshots. This leaves campaign distribution teams without a source of truth for campaign-usable stock, assembled package availability, or package consumption after completed deliveries.

The inventory model also stores one aggregate stock row per `(InventoryId, SupplyItemId)`. Import transaction metadata already supports `ImportBatchCode`, `UnitCost`, and `ExpiryDate`, but these values are not reflected in on-hand stock records, so the system cannot track remaining quantity per lot or support lot-based valuation and depletion. In parallel, relief campaigns use a special lifecycle (`Draft`, `ReadyToExecute`, `InProgress`, `Suspended`, `Completed`, `Cancelled`) that is enforced in status patch logic but not consistently reflected in the surrounding editability and API behavior.

This change spans multiple services, introduces new persistence structures, and requires migration-safe rollout because existing workflows already mutate inventory quantities in production-facing paths.

## Goals / Non-Goals

**Goals:**
- Persist campaign-owned operational stock so approved allocations become visible and consumable by campaign distribution workflows.
- Make package assembly and completed household delivery consume and produce campaign-level stock with auditable transaction history.
- Introduce per-lot inventory tracking for imports and exports while preserving aggregated stock summaries for fast reads and backward compatibility.
- Align relief campaign status patch behavior, readiness validation, and editable states so relief lifecycle operations are predictable and consistent.
- Preserve transaction-centered auditability by modeling new stock movements through explicit ledger records rather than direct quantity edits.

**Non-Goals:**
- Introduce separate physical inventories for each campaign team or distribution point in this phase.
- Replace all existing station inventory reporting with campaign inventory reporting for non-relief domains.
- Implement full warehouse-management policies beyond a simple deterministic lot consumption strategy.
- Redesign fundraising or rescue campaign lifecycles.

## Decisions

### 1. Add a dedicated campaign operational inventory ledger
The system will introduce campaign-owned stock persistence, modeled as `CampaignInventory` plus `CampaignInventoryStock` and a campaign transaction ledger. Approved supply allocation will become a two-sided movement:
- export from source station inventory
- import into campaign inventory

Package assembly and delivery completion will consume or produce campaign stock instead of mutating station inventory directly.

**Why this choice:** It gives the campaign a true source of truth for what it can distribute, assemble, and consume. It also keeps station warehouse stock and campaign operational stock conceptually separate.

**Alternative considered:** Recompute campaign stock on the fly from allocations and delivery records only. Rejected because it becomes fragile once partial deliveries, reversals, or package assembly flows are added.

### 2. Keep team/distribution ownership as transaction metadata, not separate inventories
Campaign team and distribution point attribution will be captured on campaign stock transactions and delivery records instead of creating per-team inventories in this phase.

**Why this choice:** It satisfies reporting and audit needs without exploding the data model into multiple stock pools. Team-specific physical stock can be added later if business truly needs stock handoff between campaign center and field teams.

**Alternative considered:** Create `CampaignTeamInventory` immediately. Rejected for phase 1 because it adds complexity before campaign-level stock is stabilized.

### 3. Introduce `InventoryLot` while retaining `InventoryStock` as an aggregate summary
The system will add an `InventoryLot` entity to persist per-receipt stock lots with fields such as inventory, supply item, batch code, unit cost, expiry, received quantity, remaining quantity, source reference, and import transaction linkage. `InventoryStock` remains as the aggregate summary row used by existing read paths and compatibility logic.

On import:
- create a new lot
- increment aggregate stock summary

On export:
- select lots by deterministic depletion policy
- reduce lot remaining quantity
- reduce aggregate stock summary

**Why this choice:** It minimizes disruption to existing services that already rely on `InventoryStock.CurrentQuantity` while enabling traceability and valuation.

**Alternative considered:** Replace `InventoryStock` entirely with dynamic aggregation from lots. Rejected for the first phase because it would require broad query rewrites and higher migration risk.

### 4. Use FEFO first, then FIFO for lot depletion
Exports will consume lots ordered by earliest expiry first; when expiry is missing or equal, fallback to oldest receipt first.

**Why this choice:** Relief supplies are often expiry-sensitive, so FEFO is more operationally useful than pure FIFO. Fallback FIFO keeps behavior deterministic.

**Alternative considered:** Weighted-average costing without lot depletion. Rejected because it does not solve traceability and expiry handling.

### 5. Separate station inventory transactions from campaign inventory transactions
Campaign stock movements will not be stored as plain station `InventoryTransaction` records. Instead, campaign stock will use its own transaction entity or logically separate ledger tied to `CampaignInventory`, while allocation approval links both sides of the movement for audit.

**Why this choice:** Station inventory and campaign operational stock are different ownership domains. A dedicated ledger avoids overloading `InventoryTransaction.InventoryId` with campaign semantics.

**Alternative considered:** Fake campaign inventory as another station inventory. Rejected because campaigns are not relief stations and this would blur ownership, station permissions, and reporting.

### 6. Make delivery completion consume assembled package stock
When a household delivery is completed, the system will validate that one package unit is available in campaign stock for the selected package definition output item, then create a campaign stock export transaction associated with the household delivery, campaign team, and distribution context before marking the delivery delivered.

**Why this choice:** Completed delivery is the moment at which package stock is actually consumed. This closes the gap between operational progress and stock balance.

**Alternative considered:** Reserve stock during assignment and only confirm status later. Rejected for now because reservation semantics are not required to solve the current feedback and would complicate failure/reassignment handling.

### 7. Normalize relief lifecycle rules in one place and expose allowed transitions
Relief campaign status transitions and readiness checks will remain type-specific, but the backend will centralize them as the single source of truth and update editability checks to use the same lifecycle semantics. The API response should also be extendable to expose allowed next statuses for debugging and frontend clarity.

**Why this choice:** The patch endpoint already exists, but its specialized rules conflict with generic assumptions elsewhere. Centralization prevents drift.

**Alternative considered:** Keep the split `ReadyToExecute` and `InProgress` lifecycle. Rejected because the product requirement is to allow relief campaigns to move directly from `Draft` to `Active`, and the extra states were causing frontend mismatch and patch failures.

## Risks / Trade-offs

- **[Risk] Dual stock ledgers increase implementation complexity** → Mitigation: keep campaign stock scope limited to relief campaign operations and reuse transaction-style patterns from existing inventory services.
- **[Risk] Aggregate `InventoryStock` and `InventoryLot` can drift** → Mitigation: only mutate both through centralized transaction services and validate totals in tests/migration checks.
- **[Risk] Existing APIs may expect station inventory based package availability** → Mitigation: version or explicitly update affected relief endpoints and document campaign-stock semantics in spec and implementation notes.
- **[Risk] Data migration for existing stock has no historical lot breakdown** → Mitigation: seed one synthetic opening lot per current stock row for existing inventories, with null batch and unknown source metadata.
- **[Risk] Delivery completion may fail on old data with no campaign stock** → Mitigation: introduce migration/backfill paths for active allocations and gate new behavior behind campaign stock availability.
- **[Trade-off] No per-team stock pool in phase 1** → This reduces complexity now but means team-level stock handoff remains reporting metadata rather than a physical stock boundary.
- **[Trade-off] Keeping aggregate stock summaries duplicates data** → This increases write complexity but preserves compatibility and query simplicity for current services.

## Migration Plan

1. Add new schema objects for campaign inventory, campaign inventory stock, campaign stock transactions, and inventory lots.
2. Backfill inventory lots from current `InventoryStock` rows using one synthetic opening lot per stock row with remaining quantity equal to current quantity.
3. Backfill campaign operational stock only for explicitly selected states if needed; otherwise enable new campaign inventory creation for new allocations going forward and document historical limitations.
4. Update allocation approval flow to write both source station export and campaign stock import atomically.
5. Update package assembly and delivery completion flows to read/write campaign stock.
6. Update campaign inventory balance endpoint to report campaign-owned stock balances.
7. Update relief campaign lifecycle validation and patch responses.
8. Validate with integration tests, then deploy.

Rollback strategy:
- keep additive schema changes first;
- if application rollback is needed before campaign stock is used in production, disable new write paths and fall back to current station-only behavior;
- avoid destructive removal of old fields until the new flow is fully stable.

## Open Questions

- Should a completed household delivery always consume exactly one output package unit, or can one household receive multiple units of the same package definition in a later phase?
- Does campaign inventory need one shared pool per campaign, or one pool per active campaign station attached to the campaign?
- When allocation is cancelled after partial campaign consumption, should the system reject the cancellation or only reverse the remaining unconsumed campaign stock?
- Should campaign stock transactions support manual adjustment endpoints in phase 1, or only system-generated movements?

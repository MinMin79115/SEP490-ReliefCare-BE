## Why

The current backend records stock at station inventory level only, so supply allocations remove stock from a station without creating a persisted operational balance for the campaign. This prevents teams and distribution flows from knowing how much campaign-usable stock or how many packages remain, while the inventory model also lacks lot-level cost and expiry tracking needed for traceability and station budget reconciliation.

## What Changes

- Add a campaign-level inventory ledger so approved supply allocations move stock from station inventory into persisted campaign stock instead of only deducting the source inventory.
- Add campaign stock consumption flows for package assembly and household delivery completion so delivered packages reduce campaign-available balances with auditable transactions.
- Add lot-based inventory receipt and consumption tracking so each import can be stored as a separate lot with quantity, cost, batch, expiry, and remaining balance while preserving aggregated stock summaries.
- Update inventory import and transaction behavior to support lot creation on import and lot-aware depletion on export.
- Fix relief campaign status patch behavior by aligning relief status transitions, readiness validation, and editability rules with the relief lifecycle used by the API.

## Capabilities

### New Capabilities
- `campaign-operational-inventory`: Persist campaign-owned stock balances created from approved allocations and consumed by package assembly and delivery flows.
- `inventory-lot-tracking`: Persist per-lot inventory receipts and lot-aware stock depletion with cost, batch, expiry, and remaining quantity tracking.
- `relief-campaign-status-lifecycle`: Enforce and expose a consistent relief campaign lifecycle for status patching and readiness checks.

### Modified Capabilities

- None.

## Impact

- Affected services: `SupplyAllocationService`, `CampaignService`, `ReliefDistributionService`, `InventoryService`, `InventoryTransactionService`.
- Affected APIs: campaign inventory balance, supply allocation approval, package assembly, household delivery completion, inventory import/transaction endpoints, campaign status patch.
- Affected persistence: new campaign inventory and inventory lot tables/entities, related repositories, migrations, and transaction flows.
- Affected reporting/audit behavior: package availability, remaining campaign stock, import history, lot traceability, and relief campaign status validation.

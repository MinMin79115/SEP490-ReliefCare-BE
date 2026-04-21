## Why

The current backend can define relief package templates, but it cannot execute warehouse-side package assembly from actual inventory stock. Relief stations need a workflow to assemble predefined aid packages from loose supply items, store the assembled packages in inventory as normal stock items, and preserve an audit log showing when a station created packages, who created them, and which component supply items were consumed.

This change is needed so warehouse operations can move from package planning to package stock execution while reusing the existing inventory and transfer systems instead of introducing a parallel warehouse subsystem.

## What Changes

- Add a warehouse package-assembly workflow for relief campaigns.
- Extend relief package definitions so each definition identifies the supply item that represents the assembled package in stock.
- Add package-assembly execution records and assembly-item audit records.
- Allow the system to calculate the maximum number of packages that can be assembled from current inventory stock.
- Allow the system to assemble packages by consuming component stock and increasing the stock of the package output supply item.
- Ensure package output items can be treated like normal supply items for later inventory movement and relief distribution.
- Prevent package-category supply items from being used as package-definition components.

## Capabilities

### New Capabilities
- `relief-package-assembly`: Assemble relief packages from inventory stock, track assembly history, and expose package assembly availability for warehouse operations.

### Modified Capabilities
- `relief-distribution-operations`: Relief package definitions will additionally identify the output supply item that represents the assembled package in stock.

## Impact

- Affected domain areas: `SupplyItem`, `InventoryStock`, `InventoryTransaction`, `ReliefPackageDefinition`, and new package-assembly audit entities.
- Affected API areas: relief package definition creation/update and new package-assembly endpoints.
- Affected stock behavior: package assembly will create inventory-backed stock changes using the current transaction backbone.
- Reused systems: inventory stock, inventory transaction, supply transfer, and relief package definition management.

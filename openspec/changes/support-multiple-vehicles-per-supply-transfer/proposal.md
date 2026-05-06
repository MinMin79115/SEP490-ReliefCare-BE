## Why

The current relief aid logistics flow allows a `SupplyTransfer` to reference only one vehicle and one driver, which does not match real relief operations where a single transfer batch may require multiple vehicles carrying supplies together or departing at different times. This change enables detailed vehicle assignment and tracking per transfer while keeping the transfer status as the overall logistics lifecycle.

## What Changes

- Add support for assigning multiple vehicles to one supply transfer through a dedicated transfer-vehicle assignment model.
- Track each assigned vehicle independently with driver, assignment status, timestamps, and notes.
- Update supply transfer shipping/receiving logic to work with multiple assigned vehicles instead of a single `VehicleId`/`DriverUserId` pair.
- Return assigned vehicle details as a collection in supply transfer responses.
- Add API operations for assigning, removing, and updating the lifecycle of vehicles within a transfer.
- Synchronize vehicle availability so assigned/in-transit vehicles are not double-booked across active logistics flows.
- **BREAKING**: Clients that rely on a single `SupplyTransfer.VehicleId` or `DriverUserId` must migrate to the transfer vehicle collection once the legacy fields are removed or deprecated.

## Capabilities

### New Capabilities
- `supply-transfer-vehicle-assignments`: Defines how multiple vehicles and drivers are assigned to, tracked within, and released from a supply transfer.

### Modified Capabilities

No existing OpenSpec capabilities are present in this repository yet.

## Impact

- Domain model: add `SupplyTransferVehicle` and `SupplyTransferVehicleStatus`; update `SupplyTransfer` and `Vehicle` navigation properties.
- Database: add a `SupplyTransferVehicles` table, indexes, relationships, and optional data migration from legacy `SupplyTransfers.VehicleId`/`DriverUserId`.
- Application services: update `SupplyTransferService` shipping, receiving, cancellation, mapping, and validation logic; add transfer vehicle lifecycle methods.
- API: add endpoints for managing vehicles on a transfer and update supply transfer response contracts.
- Repositories/queries: include assigned vehicle, vehicle type, and driver data when loading transfer details.
- Vehicle availability: validate station ownership, `VehicleStatus.Free`, active transfer assignments, active rescue operations, and campaign assignments where applicable.
- Frontend/API consumers: update transfer detail and shipping screens from single vehicle selection to multi-vehicle assignment and per-vehicle tracking.

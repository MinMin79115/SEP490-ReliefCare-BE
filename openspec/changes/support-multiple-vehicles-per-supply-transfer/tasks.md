## 1. Domain Model

- [x] 1.1 Add `SupplyTransferVehicleStatus` enum with `Assigned`, `InTransit`, `Arrived`, `Completed`, `Cancelled`, and `Incident` states.
- [x] 1.2 Add `SupplyTransferVehicle` entity with transfer, vehicle, optional driver, status, timestamps, and note fields.
- [x] 1.3 Add `ICollection<SupplyTransferVehicle>` navigation to `SupplyTransfer`.
- [x] 1.4 Add `ICollection<SupplyTransferVehicle>` navigation to `Vehicle`.

## 2. Database and EF Core

- [x] 2.1 Add `DbSet<SupplyTransferVehicle>` to `ApplicationDbContext`.
- [x] 2.2 Configure `SupplyTransferVehicle` relationships to `SupplyTransfer`, `Vehicle`, and driver user with appropriate delete behavior.
- [x] 2.3 Add unique index on `{ SupplyTransferId, VehicleId }` to prevent duplicate vehicle assignment in the same transfer.
- [x] 2.4 Add indexes needed for active assignment lookup by `VehicleId` and `Status`.
- [x] 2.5 Create EF migration for the new table and relationships.
- [x] 2.6 Add optional migration/backfill logic for existing `SupplyTransfers.VehicleId` and `DriverUserId` data if legacy data must be preserved.

## 3. DTOs and API Contracts

- [x] 3.1 Add request DTO for assigning multiple vehicles to a supply transfer.
- [x] 3.2 Add request DTO for updating transfer vehicle lifecycle or reporting incidents.
- [x] 3.3 Add response DTO for assigned transfer vehicles including vehicle, vehicle type, driver, status, timestamps, and note.
- [x] 3.4 Update `SupplyTransferResponse` to expose a `Vehicles` collection.
- [x] 3.5 Deprecate or stop relying on single-transfer `VehicleId` and `DriverUserId` response fields where applicable.

## 4. Repository and Query Loading

- [x] 4.1 Update `SupplyTransferRepository` detail queries to include transfer vehicles, vehicles, vehicle types, and drivers.
- [ ] 4.2 Add query support for finding active supply transfer assignments by vehicle id.
- [x] 4.3 Update mapping logic in `SupplyTransferService` to map the assigned vehicle collection.
- [x] 4.4 Add or update repository methods needed for adding, removing, and updating transfer vehicle assignments.

## 5. Service Logic

- [x] 5.1 Add `AssignVehiclesAsync` with validation for transfer status, source station authorization, duplicate vehicle ids, station ownership, vehicle availability, and active assignment conflicts.
- [x] 5.2 Add `RemoveVehicleAsync` for removing non-started assignments and releasing the vehicle.
- [x] 5.3 Add `DepartVehicleAsync` to mark an assignment `InTransit`, set departure timestamp, and move the parent transfer to `Shipping` when needed.
- [x] 5.4 Add `ArriveVehicleAsync` to mark an in-transit assignment `Arrived` and set arrival timestamp.
- [x] 5.5 Add `CompleteVehicleAsync` to mark an arrived assignment `Completed` and release the vehicle.
- [x] 5.6 Add `ReportVehicleIncidentAsync` to mark an active assignment `Incident` while keeping the vehicle unavailable.
- [x] 5.7 Update `ShipAsync` to require at least one active assigned vehicle and use the assignment collection instead of a single vehicle field.
- [x] 5.8 Update `ReceiveAsync` to complete active transfer vehicle assignments and release vehicles in the same transaction as receiving inventory.
- [x] 5.9 Update `CancelAsync` to cancel active transfer vehicle assignments and release vehicles when cancellation is allowed.

## 6. API Endpoints

- [x] 6.1 Add endpoint to assign multiple vehicles to a supply transfer.
- [x] 6.2 Add endpoint to remove a vehicle from a supply transfer before departure.
- [x] 6.3 Add endpoint to mark a transfer vehicle as departed.
- [x] 6.4 Add endpoint to mark a transfer vehicle as arrived.
- [x] 6.5 Add endpoint to complete a transfer vehicle assignment.
- [x] 6.6 Add endpoint to report an incident for a transfer vehicle assignment.
- [x] 6.7 Add or update endpoint for available vehicles for transfer logistics by source station.

## 7. Validation, Authorization, and Concurrency

- [x] 7.1 Ensure only authorized source station actors can assign, remove, ship, or depart transfer vehicles.
- [x] 7.2 Ensure only authorized destination station actors can receive transfer inventory and complete destination-side actions where applicable.
- [x] 7.3 Check `Vehicle.Status == Free` and source station ownership inside the assignment transaction.
- [ ] 7.4 Check active rescue operations and active campaign vehicle assignments before assigning a vehicle if those flows share the same vehicle pool.
- [x] 7.5 Wrap assignment, shipping, receiving, cancellation, and vehicle release operations in database transactions.
- [ ] 7.6 Add optimistic concurrency or conditional update handling to prevent concurrent double-booking of the same vehicle.

## 8. Tests and Verification

- [ ] 8.1 Add service tests for assigning multiple vehicles successfully.
- [ ] 8.2 Add tests rejecting duplicate, busy, deleted, wrong-station, and already-active vehicles.
- [ ] 8.3 Add tests for depart, arrive, complete, incident, ship, receive, and cancel lifecycle transitions.
- [ ] 8.4 Add tests verifying vehicles are set `Busy` on assignment and `Free` on completion/cancellation/receiving.
- [ ] 8.5 Add API tests for new transfer vehicle endpoints.
- [x] 8.6 Run project build and relevant automated tests.

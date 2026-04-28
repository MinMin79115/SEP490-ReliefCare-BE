## Context

The current relief aid logistics flow stores vehicle assignment directly on `SupplyTransfer` through a single `VehicleId`/`DriverUserId` pair. This makes the transfer behave like a one-vehicle trip, while real relief transfers may require a convoy of vehicles, different drivers, staggered departures, and per-vehicle operational states.

The system already has three relevant status concepts:

- `SupplyTransferStatus`: lifecycle of the transfer request and inventory movement.
- `VehicleStatus`: global availability of a vehicle (`Free`/`Busy`).
- `VehicleAssignmentStatus`: campaign-specific vehicle assignment state.

This change introduces a separate assignment model for supply transfer vehicles so the transfer remains the parent logistics document and each assigned vehicle can be tracked independently.

## Goals / Non-Goals

**Goals:**

- Support multiple vehicles and drivers on a single supply transfer.
- Track assignment status and timestamps independently for each transfer vehicle.
- Prevent active vehicles from being double-booked in logistics, rescue, or campaign flows.
- Keep inventory export/import at the `SupplyTransfer` level unless item-per-vehicle tracking is added later.
- Provide API response data suitable for transfer detail, shipping, and vehicle tracking screens.
- Preserve a practical migration path from the legacy single-vehicle fields.

**Non-Goals:**

- Track exact quantities loaded on each vehicle in this change.
- Add route optimization, GPS tracking, fuel logs, mileage, or maintenance workflows.
- Redesign rescue vehicle assignment.
- Redesign campaign vehicle assignment, except for availability checks that prevent conflicts.
- Introduce a new global trip/dispatch aggregate beyond the supply transfer vehicle assignment.

## Decisions

### Decision 1: Add `SupplyTransferVehicle` instead of storing many IDs on `SupplyTransfer`

Use a normalized child entity:

```text
SupplyTransfer 1 -> n SupplyTransferVehicle n -> 1 Vehicle
```

Rationale:

- Supports multiple vehicles, drivers, timestamps, notes, and per-vehicle status.
- Avoids comma-separated IDs or JSON arrays in relational columns.
- Allows efficient querying for active assignments by vehicle.

Alternative considered: keep `SupplyTransfer.VehicleId` and create multiple transfers for one real-world batch. This was rejected because it fragments approval, inventory, document, and receiving state across duplicate transfers.

### Decision 2: Keep transfer status and vehicle-assignment status separate

`SupplyTransfer.Status` remains the overall lifecycle:

```text
Pending -> Approved -> Shipping -> Received
                  \-> Cancelled
```

Each `SupplyTransferVehicle.Status` tracks that vehicle inside the transfer:

```text
Assigned -> InTransit -> Arrived -> Completed
         \-> Cancelled
         \-> Incident
```

Rationale:

- A transfer can be `Shipping` while one vehicle is in transit, another has arrived, and another has an incident.
- Transfer-level inventory receiving remains a single business action, while vehicle movement can be tracked independently.

### Decision 3: Reserve vehicles when assigned

When a vehicle is assigned to a supply transfer, set `Vehicle.Status = Busy` immediately rather than waiting until departure.

Rationale:

- Assignment reserves the vehicle for the logistics operation.
- Prevents a vehicle from being selected by another supply transfer, rescue operation, or campaign between assignment and departure.

Alternative considered: set `Busy` only at departure. This allows planning flexibility but creates double-booking risk unless a separate reservation status is added.

### Decision 4: Keep inventory quantities at transfer level for this change

The existing `SupplyTransferItem` and `InventoryTransaction` behavior remains transfer-level. This change does not introduce `SupplyTransferVehicleItem`.

Rationale:

- The immediate requirement is “one transfer has many vehicles”, not “which vehicle carries each item”.
- Avoids a large inventory redesign.
- A future change can add item allocation per vehicle if needed.

### Decision 5: Add new APIs while optionally preserving legacy fields during migration

Add explicit endpoints for transfer vehicle assignment and lifecycle updates. Existing single-vehicle fields can be retained temporarily for compatibility, but new logic and responses should use `Vehicles[]`.

Rationale:

- Allows backend and frontend migration in phases.
- Reduces risk of breaking existing clients immediately.

## Risks / Trade-offs

- **Risk: Two sources of truth if legacy `VehicleId`/`DriverUserId` remain.** → Mitigate by treating legacy fields as read-only/deprecated and using `SupplyTransferVehicles` as the source of truth for new logic.
- **Risk: Vehicle double-booking due to concurrent assignment requests.** → Mitigate with database transaction, `Vehicle.Status == Free` validation inside the transaction, active assignment checks, and optionally optimistic concurrency on `Vehicle`.
- **Risk: Inconsistent vehicle release on cancellation or receiving errors.** → Mitigate by updating vehicle statuses and assignment statuses in the same transaction as transfer lifecycle changes.
- **Risk: More complex frontend workflows.** → Mitigate with response DTOs that expose a complete `Vehicles[]` collection and clear per-vehicle actions.
- **Risk: Inventory receiving semantics become unclear when vehicles arrive separately.** → Mitigate by keeping `ReceiveAsync` as the transfer-level inventory confirmation action and using per-vehicle arrival/completion as logistics tracking only.

## Migration Plan

1. Add `SupplyTransferVehicleStatus`, `SupplyTransferVehicle`, navigation properties, EF configuration, and migration.
2. If existing transfer records have `VehicleId`, backfill one `SupplyTransferVehicle` per such transfer.
3. Update repository includes and response mapping to return `Vehicles[]`.
4. Add assignment and lifecycle APIs for transfer vehicles.
5. Update `ShipAsync`, `ReceiveAsync`, and `CancelAsync` to use the assignment collection.
6. Keep legacy fields temporarily if needed by clients, but stop using them as the authoritative assignment source.
7. After clients migrate, remove or fully deprecate legacy single-vehicle fields in a later change.

Rollback strategy: if deployment fails before legacy fields are removed, disable new endpoints and continue using the legacy single-vehicle flow. If the new table has been populated, data can remain unused until the issue is fixed.

## Open Questions

- Should drivers be required for every assigned vehicle, or optional until departure?
- Should station heads be the only actors allowed to update vehicle movement, or can assigned drivers update their own vehicle status?
- Should `Incident` keep the vehicle `Busy`, or should a future vehicle status such as `Maintenance` be introduced?
- Does the frontend need per-vehicle item allocation in a later phase?

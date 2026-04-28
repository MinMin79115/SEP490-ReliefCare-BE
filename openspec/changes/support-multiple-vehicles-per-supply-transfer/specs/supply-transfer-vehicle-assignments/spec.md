## ADDED Requirements

### Requirement: Supply transfer supports multiple assigned vehicles
The system SHALL allow a single approved supply transfer to have multiple assigned vehicles through transfer vehicle assignment records.

#### Scenario: Assign multiple vehicles to an approved transfer
- **WHEN** an authorized source station actor assigns two available vehicles to an approved supply transfer
- **THEN** the system creates two transfer vehicle assignments linked to the same supply transfer
- **AND** the supply transfer response includes both vehicles in its `Vehicles` collection

#### Scenario: Reject assignment before approval
- **WHEN** an actor attempts to assign vehicles to a supply transfer that is `Pending`, `Shipping`, `Received`, or `Cancelled`
- **THEN** the system rejects the assignment

#### Scenario: Reject duplicate vehicle in same transfer
- **WHEN** an actor attempts to assign the same vehicle more than once to the same supply transfer
- **THEN** the system rejects the duplicate assignment

### Requirement: Assigned vehicles include driver and tracking metadata
The system SHALL store vehicle assignment metadata independently for each vehicle in a supply transfer, including optional driver, status, timestamps, and note.

#### Scenario: Assignment stores metadata
- **WHEN** a vehicle is assigned with a driver and note
- **THEN** the transfer vehicle assignment stores the vehicle, driver, assignment status, assignment timestamp, and note

#### Scenario: Response exposes assigned vehicle metadata
- **WHEN** a client retrieves a supply transfer detail
- **THEN** the response includes each assigned vehicle's license plate, vehicle type, driver information, status, timestamps, and note

### Requirement: Vehicle availability is reserved during active transfer assignment
The system SHALL mark a vehicle as unavailable for other active operations while it has an active supply transfer assignment.

#### Scenario: Assigning a free vehicle reserves it
- **WHEN** an available vehicle is assigned to an approved supply transfer
- **THEN** the system sets the vehicle status to `Busy`

#### Scenario: Reject busy vehicle assignment
- **WHEN** an actor attempts to assign a vehicle whose status is not `Free`
- **THEN** the system rejects the assignment

#### Scenario: Reject vehicle from another station
- **WHEN** an actor attempts to assign a vehicle that does not belong to the transfer source station
- **THEN** the system rejects the assignment

#### Scenario: Reject vehicle active in another logistics assignment
- **WHEN** an actor attempts to assign a vehicle that already has an active supply transfer vehicle assignment
- **THEN** the system rejects the assignment

### Requirement: Transfer vehicle assignments have their own lifecycle
The system SHALL track each assigned vehicle's lifecycle independently from the parent supply transfer lifecycle.

#### Scenario: Vehicle departs
- **WHEN** an authorized actor marks an assigned vehicle as departed
- **THEN** the system changes the transfer vehicle assignment status to `InTransit`
- **AND** records the departure timestamp
- **AND** changes the parent supply transfer status to `Shipping` if it is currently `Approved`

#### Scenario: Vehicle arrives
- **WHEN** an authorized actor marks an in-transit assigned vehicle as arrived
- **THEN** the system changes the transfer vehicle assignment status to `Arrived`
- **AND** records the arrival timestamp

#### Scenario: Vehicle completes assignment
- **WHEN** an authorized actor completes an arrived vehicle assignment
- **THEN** the system changes the transfer vehicle assignment status to `Completed`
- **AND** records the completion timestamp
- **AND** sets the vehicle status to `Free`

#### Scenario: Vehicle incident is reported
- **WHEN** an authorized actor reports an incident for an active assigned vehicle
- **THEN** the system changes the transfer vehicle assignment status to `Incident`
- **AND** keeps the vehicle unavailable until the assignment is completed or cancelled

### Requirement: Transfer shipping uses assigned vehicles
The system SHALL require at least one active assigned vehicle before a supply transfer can be shipped.

#### Scenario: Reject shipping without vehicles
- **WHEN** an authorized actor attempts to ship an approved supply transfer with no active vehicle assignments
- **THEN** the system rejects the shipping action

#### Scenario: Ship transfer with assigned vehicles
- **WHEN** an authorized actor ships an approved supply transfer with active assigned vehicles
- **THEN** the system changes the supply transfer status to `Shipping`
- **AND** records the shipped timestamp
- **AND** sets assigned vehicles that have not departed to `InTransit`

### Requirement: Receiving or cancellation releases assigned vehicles
The system SHALL release vehicles when their supply transfer assignment is completed or cancelled by a valid transfer lifecycle action.

#### Scenario: Receive transfer releases active assigned vehicles
- **WHEN** an authorized destination station actor receives a shipping supply transfer
- **THEN** the system creates the transfer-in inventory transaction
- **AND** changes the supply transfer status to `Received`
- **AND** completes active transfer vehicle assignments
- **AND** sets their vehicles to `Free`

#### Scenario: Cancel approved transfer releases assigned vehicles
- **WHEN** an authorized actor cancels a pending or approved supply transfer with assigned vehicles
- **THEN** the system changes the supply transfer status to `Cancelled`
- **AND** changes active transfer vehicle assignments to `Cancelled`
- **AND** sets their vehicles to `Free`

#### Scenario: Reject cancellation after shipping
- **WHEN** an actor attempts to cancel a supply transfer that is `Shipping` or `Received`
- **THEN** the system rejects the cancellation

### Requirement: Transfer vehicle updates are transactional
The system SHALL update transfer vehicle assignment state, parent transfer state, and vehicle availability atomically for assignment and lifecycle operations.

#### Scenario: Assignment fails without partial reservation
- **WHEN** assigning multiple vehicles fails validation for any vehicle
- **THEN** the system does not create any transfer vehicle assignment from that request
- **AND** does not change any vehicle status from that request

#### Scenario: Concurrent assignment cannot double-book vehicle
- **WHEN** two requests attempt to assign the same free vehicle concurrently
- **THEN** only one request succeeds
- **AND** the other request receives a vehicle-not-available error

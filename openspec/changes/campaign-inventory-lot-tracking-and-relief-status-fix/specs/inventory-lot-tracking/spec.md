## ADDED Requirements

### Requirement: Inventory imports MUST create distinct stock lots
The system SHALL persist each inventory import as a distinct inventory lot containing the imported supply item, quantity, remaining quantity, unit cost, batch metadata, expiry metadata, and source reference where provided.

#### Scenario: Import transaction creates a new lot
- **WHEN** an inventory import transaction is created for a supply item
- **THEN** the system SHALL create a new inventory lot for that receipt
- **AND** the lot SHALL store the imported quantity as its original quantity and remaining quantity
- **AND** the aggregate inventory stock summary SHALL increase by the imported quantity

#### Scenario: Multiple imports create multiple lots for the same item
- **WHEN** the same supply item is imported into the same inventory in multiple receipts
- **THEN** the system SHALL create a separate inventory lot for each receipt
- **AND** the aggregate inventory stock summary SHALL reflect the combined remaining quantity across all lots

### Requirement: Inventory exports MUST deplete lots deterministically
The system SHALL deplete inventory lots using earliest-expiry-first ordering and oldest-receipt-first ordering as a fallback when expiry is unavailable or equal.

#### Scenario: Export depletes the earliest expiring lots first
- **WHEN** an export transaction is created for an item with multiple available lots that have different expiry dates
- **THEN** the system SHALL reduce remaining quantity from the earliest expiring eligible lots before later expiring lots

#### Scenario: Export falls back to oldest received lot when expiry is equal or missing
- **WHEN** an export transaction is created for an item whose eligible lots have equal or null expiry dates
- **THEN** the system SHALL reduce remaining quantity from the oldest received lot first

### Requirement: Aggregate stock summary MUST stay synchronized with lots
The system SHALL maintain aggregate inventory stock summary quantities as the sum of remaining quantities across all non-empty lots for the same inventory and supply item.

#### Scenario: Import updates lot and summary quantities together
- **WHEN** an import transaction succeeds
- **THEN** the created inventory lot remaining quantity SHALL equal the imported quantity
- **AND** the aggregate inventory stock summary SHALL reflect the new total remaining quantity across lots

#### Scenario: Export updates lot and summary quantities together
- **WHEN** an export transaction succeeds
- **THEN** the selected inventory lots SHALL have their remaining quantities reduced
- **AND** the aggregate inventory stock summary SHALL be reduced by the exported quantity

### Requirement: Inventory history MUST expose lot traceability data
The system SHALL preserve enough lot linkage to identify which import lots remain on hand and which lots were depleted by each export transaction.

#### Scenario: On-hand lot history is available for an inventory item
- **WHEN** inventory history is queried for a supply item
- **THEN** the system SHALL be able to return the lots for that item with batch, expiry, unit cost, original quantity, and remaining quantity

#### Scenario: Export history can be traced to consumed lots
- **WHEN** an export transaction is queried
- **THEN** the system SHALL be able to identify which inventory lots were consumed and by what quantity

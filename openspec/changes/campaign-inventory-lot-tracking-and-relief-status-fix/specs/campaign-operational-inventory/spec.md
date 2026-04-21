## ADDED Requirements

### Requirement: Approved supply allocation MUST create campaign operational stock
The system SHALL persist campaign-owned operational stock for relief campaigns. When a supply allocation is approved, the system SHALL atomically export the approved quantities from the source station inventory and import the same quantities into campaign operational stock linked to the target campaign.

#### Scenario: Approving an allocation moves stock into campaign inventory
- **WHEN** an allocation for a relief campaign transitions from `Pending` to `Approved`
- **THEN** the source station inventory quantity SHALL be reduced for each allocated item
- **AND** the campaign operational inventory quantity SHALL be increased by the same amounts
- **AND** both sides of the movement SHALL be recorded in auditable transaction records

#### Scenario: Allocation approval fails if source stock is insufficient
- **WHEN** an allocation is approved and any allocated item quantity exceeds available source inventory stock
- **THEN** the system SHALL reject the approval
- **AND** no campaign operational stock SHALL be created or updated

### Requirement: Campaign inventory balance MUST report campaign-owned stock
The system SHALL provide campaign inventory balance data from persisted campaign operational stock instead of inferring balance only from attached station inventory quantities.

#### Scenario: Campaign balance shows campaign stock after approved allocation
- **WHEN** a campaign has approved allocations that have created campaign operational stock
- **THEN** the campaign inventory balance response SHALL include the campaign-owned quantities for those supply items

#### Scenario: Campaign balance excludes unapproved allocations
- **WHEN** a campaign has allocations in `Pending` or `Cancelled` state that have not created campaign stock
- **THEN** the campaign inventory balance response SHALL NOT count those quantities as available campaign stock

### Requirement: Package assembly MUST consume and produce campaign stock
The system SHALL execute relief package assembly against campaign operational stock. Component items SHALL be consumed from campaign stock and assembled package output items SHALL be added back into campaign stock.

#### Scenario: Assembling packages updates campaign stock quantities
- **WHEN** a user assembles relief packages for a campaign package definition
- **THEN** the required component quantities SHALL be deducted from campaign operational stock
- **AND** the output package item quantity SHALL be added to campaign operational stock
- **AND** the assembly event SHALL be recorded with consumed component details

#### Scenario: Assembly is rejected when campaign stock cannot satisfy components
- **WHEN** the requested package assembly quantity requires more component stock than the campaign currently owns
- **THEN** the system SHALL reject the assembly request
- **AND** no campaign stock quantities SHALL change

### Requirement: Delivery completion MUST consume package stock from campaign inventory
The system SHALL consume campaign-owned package stock when a household delivery is completed. The consumed stock movement SHALL be linked to the delivery record and campaign distribution context.

#### Scenario: Completing a delivery consumes one campaign package unit
- **WHEN** a household delivery is completed for a selected package definition
- **THEN** the system SHALL reduce campaign operational stock for the package definition output item
- **AND** the stock consumption SHALL be recorded with references to the campaign, delivery, and team or distribution context if present
- **AND** the delivery SHALL only be marked delivered after stock consumption succeeds

#### Scenario: Delivery completion fails when package stock is unavailable
- **WHEN** a household delivery is completed but no campaign stock is available for the selected package output item
- **THEN** the system SHALL reject the completion request
- **AND** the delivery status SHALL remain unchanged

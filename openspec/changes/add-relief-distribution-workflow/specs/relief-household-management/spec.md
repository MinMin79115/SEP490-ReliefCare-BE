## ADDED Requirements

### Requirement: Relief campaigns SHALL manage affected households as campaign-scoped beneficiaries
The system SHALL allow a relief campaign to maintain a campaign-scoped list of affected households provided by an external authority or internal data entry. Each household record MUST belong to exactly one campaign and MUST store enough information to support assignment and delivery tracking.

#### Scenario: Create household list for a relief campaign
- **WHEN** a manager creates or imports affected-household records for a relief campaign
- **THEN** the system stores each household as a campaign-scoped beneficiary record linked to that campaign

#### Scenario: Reject household creation for non-relief campaign flow
- **WHEN** a caller attempts to create relief-household records for a campaign that is not of type `Relief`
- **THEN** the system rejects the operation

### Requirement: Households SHALL support distribution-point or direct-delivery assignment
Each campaign household SHALL support assignment to either a temporary distribution point or a direct-delivery flow for isolated households. The system MUST preserve whether the household is isolated and which fulfillment mode applies.

#### Scenario: Assign non-isolated household to distribution point
- **WHEN** a manager assigns a non-isolated household to a distribution point
- **THEN** the household is marked for distribution-point fulfillment under that campaign

#### Scenario: Mark isolated household for direct delivery
- **WHEN** a manager marks a household as isolated and assigns it to direct delivery
- **THEN** the household is stored as a direct-delivery beneficiary without requiring a distribution-point assignment

### Requirement: The system SHALL provide checklist-ready household views by fulfillment target
The system SHALL provide a way to list households for a campaign by their assigned distribution point or direct-delivery grouping so volunteers and managers can work from a checklist view.

#### Scenario: View households assigned to a distribution point
- **WHEN** a user requests the checklist for a distribution point
- **THEN** the system returns the households assigned to that point and their current fulfillment status

#### Scenario: View households assigned to direct delivery
- **WHEN** a user requests the checklist for a direct-delivery grouping
- **THEN** the system returns the households assigned to that direct-delivery flow and their current fulfillment status

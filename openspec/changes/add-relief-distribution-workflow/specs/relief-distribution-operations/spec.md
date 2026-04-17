## ADDED Requirements

### Requirement: Relief campaigns SHALL define temporary distribution points
The system SHALL allow managers to create and manage campaign-scoped temporary distribution points for relief campaigns. A distribution point MUST be distinct from a permanent relief station and MUST be linkable to campaign teams and assigned households.

#### Scenario: Create distribution point for a relief campaign
- **WHEN** a manager creates a distribution point for a relief campaign
- **THEN** the system stores a campaign-scoped distribution point that can receive household assignments and team assignments

### Requirement: Relief campaigns SHALL define standard relief packages from existing supply items
The system SHALL allow managers to define one or more standard relief packages for a relief campaign using existing supply items and quantities. A package definition MUST be reusable across multiple households within the same campaign.

#### Scenario: Create package definition
- **WHEN** a manager defines a package containing multiple supply items and quantities for a relief campaign
- **THEN** the system stores a reusable package definition for that campaign

### Requirement: Household delivery completion SHALL require photo proof
The system SHALL track delivery completion per household and MUST require at least one photo proof when a household is marked as successfully delivered.

#### Scenario: Complete household delivery with proof
- **WHEN** an assigned user marks a household as delivered and uploads at least one photo proof
- **THEN** the system records the delivery as completed with the proof and completion timestamp

#### Scenario: Reject delivered status without proof
- **WHEN** an assigned user attempts to mark a household as delivered without photo proof
- **THEN** the system rejects the completion request

### Requirement: Household delivery checklist SHALL support both point pickup and direct delivery
The system SHALL allow household fulfillment records to be completed from either a distribution-point flow or a direct-delivery flow while preserving which mode was used.

#### Scenario: Complete delivery at distribution point
- **WHEN** a volunteer completes delivery for a household assigned to a distribution point
- **THEN** the system records the household as fulfilled through the distribution-point flow

#### Scenario: Complete delivery for isolated household
- **WHEN** a volunteer completes delivery for an isolated household assigned to direct delivery
- **THEN** the system records the household as fulfilled through the direct-delivery flow

### Requirement: Shortage handling SHALL require moderator approval before stock movement execution
The system SHALL allow users to create shortage requests for a relief distribution operation and MUST require moderator approval or rejection before stock movement is executed. Approved shortage handling MUST reuse the existing inventory movement backbone.

#### Scenario: Submit shortage request
- **WHEN** a user reports that a distribution operation does not have enough supplies
- **THEN** the system records a pending shortage request with requested items and quantities

#### Scenario: Approve shortage request
- **WHEN** a moderator approves a pending shortage request
- **THEN** the system records the approval and triggers the appropriate stock-movement flow using the existing allocation or transfer mechanisms

#### Scenario: Reject shortage request
- **WHEN** a moderator rejects a pending shortage request
- **THEN** the system records the rejection and does not execute stock movement

### Requirement: Relief distribution SHALL preserve inventory transaction traceability
The system SHALL preserve inventory traceability for relief distribution by reusing existing stock-accounting flows rather than creating a separate inventory subsystem.

#### Scenario: Distribution shortage results in traceable stock movement
- **WHEN** an approved shortage request causes supplies to be moved or issued
- **THEN** the resulting stock movement is traceable through the existing inventory transaction backbone

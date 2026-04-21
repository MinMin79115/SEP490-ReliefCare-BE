## ADDED Requirements

### Requirement: Managers MUST be able to extract budget from a fundraising campaign into a relief campaign
The system SHALL allow a manager to transfer available budget from a fundraising campaign into a relief campaign.

#### Scenario: Valid extraction succeeds
- **WHEN** a manager extracts an amount from a fundraising campaign into a relief campaign
- **AND** the fundraising campaign has enough remaining budget
- **THEN** the fundraising campaign `BudgetSpent` SHALL increase by the extracted amount
- **AND** the relief campaign `BudgetTotal` SHALL increase by the same amount
- **AND** the extraction SHALL be recorded in transfer history

#### Scenario: Reject extraction when source remaining budget is insufficient
- **WHEN** the requested extraction amount is greater than `fundraising.BudgetTotal - fundraising.BudgetSpent`
- **THEN** the system SHALL reject the operation
- **AND** the system SHALL log the failed attempt

#### Scenario: Reject invalid campaign type pairing
- **WHEN** the source campaign is not `Fundraising` or the target campaign is not `Relief`
- **THEN** the system SHALL reject the extraction request

### Requirement: Extraction updates MUST be atomic
The system SHALL update both campaigns and transfer history in a single database transaction.

#### Scenario: Extraction commits all-or-nothing
- **WHEN** a valid extraction request is processed
- **THEN** the fundraising campaign update, relief campaign update, and transfer history record SHALL either all commit or all rollback together

### Requirement: Extraction history MUST be traceable
The system SHALL persist transfer history with source campaign, target campaign, amount, timestamp, actor, and optional note.

#### Scenario: Extraction history can be queried later
- **WHEN** an extraction succeeds
- **THEN** the system SHALL store enough data to identify who transferred how much from which fundraising campaign to which relief campaign and when

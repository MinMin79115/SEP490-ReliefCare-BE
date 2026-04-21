## ADDED Requirements

### Requirement: Relief package definitions MAY include cash support
The system SHALL allow a relief package definition to define an optional per-package cash support amount.

#### Scenario: Package with cash support is created
- **WHEN** a user creates or updates a relief package definition with a positive cash support amount
- **THEN** the package definition SHALL persist that amount

#### Scenario: Package without cash support remains valid
- **WHEN** a user creates or updates a relief package definition without a cash support amount
- **THEN** the package SHALL be treated as goods-only

### Requirement: Delivery records MUST snapshot cash support amount
The system SHALL store the actual cash support amount used for each household delivery.

#### Scenario: Delivery captures package cash support
- **WHEN** a household delivery is assigned or completed using a package that has cash support
- **THEN** the delivery record SHALL store the cash support amount used for that delivery

### Requirement: Relief campaign budget MUST be deducted on delivery completion
The system SHALL deduct the delivery cash support amount from the relief campaign budget when the delivery is completed.

#### Scenario: Delivery with cash support deducts budget
- **WHEN** a moderator completes a delivery whose `CashSupportAmount` is greater than zero
- **THEN** the relief campaign `BudgetSpent` SHALL increase by that amount
- **AND** the delivery SHALL be recorded with package, quantity, money used, time, and actor

#### Scenario: Package-only delivery does not deduct money
- **WHEN** a moderator completes a delivery whose `CashSupportAmount` is zero or null-equivalent
- **THEN** the relief campaign budget SHALL remain unchanged by the cash-support logic

### Requirement: Relief budget overspending MUST be blocked
The system SHALL reject delivery completion when the required cash support amount exceeds the relief campaign remaining budget.

#### Scenario: Reject distribution that exceeds relief campaign balance
- **WHEN** `delivery.CashSupportAmount` is greater than `relief.BudgetTotal - relief.BudgetSpent`
- **THEN** the system SHALL reject the operation
- **AND** the system SHALL log the failure
- **AND** the system SHALL return the message:
  "Insufficient balance. Please extract more funds from fundraising campaign or create a new fundraising campaign."

### Requirement: Inventory deduction and cash deduction MUST be transactional together
The system SHALL complete package stock deduction and relief budget deduction in the same database transaction.

#### Scenario: Delivery completion commits stock and money together
- **WHEN** a delivery completion succeeds for a package with or without cash support
- **THEN** stock movement, delivery completion, proof persistence, and budget deduction SHALL either all commit or all rollback together

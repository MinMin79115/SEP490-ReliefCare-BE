## ADDED Requirements

### Requirement: Relief campaign status patch MUST enforce the relief lifecycle
The system SHALL enforce relief-specific campaign status transitions independently from fundraising and rescue campaign transitions.

#### Scenario: Valid relief transition succeeds
- **WHEN** a relief campaign requests a status transition that is valid in the relief lifecycle
- **THEN** the system SHALL persist the new status

#### Scenario: Invalid generic transition is rejected for relief campaigns
- **WHEN** a relief campaign requests a transition that is not valid for the relief lifecycle
- **THEN** the system SHALL reject the request with a validation error describing the invalid transition

### Requirement: Relief readiness validation MUST gate executable statuses consistently
The system SHALL apply readiness validation whenever a relief campaign transitions into an executable state such as `Active`.

#### Scenario: Relief campaign cannot become active without required operational setup
- **WHEN** a relief campaign is moved to `Active` without the required active team, active station, and usable operational resources
- **THEN** the system SHALL reject the status change

### Requirement: Relief campaign editability MUST align with the relief lifecycle
The system SHALL use the relief lifecycle consistently when determining whether a relief campaign can be edited or status-patched.

#### Scenario: Editable relief states match relief workflow semantics
- **WHEN** a relief campaign is in a state designated as editable by the relief lifecycle rules
- **THEN** update operations SHALL be allowed for that campaign

#### Scenario: Non-editable relief states are rejected consistently
- **WHEN** a relief campaign is in a non-editable terminal or executing state according to the relief lifecycle rules
- **THEN** update operations SHALL be rejected consistently with the same lifecycle rules used by status patching

### Requirement: Relief status APIs MUST expose valid next transitions
The system SHALL provide or embed enough lifecycle metadata for clients to determine the valid next statuses for a relief campaign.

#### Scenario: Client can inspect allowed relief transitions
- **WHEN** a client retrieves a relief campaign or requests lifecycle metadata
- **THEN** the response SHALL identify the statuses that are currently valid next transitions for that campaign

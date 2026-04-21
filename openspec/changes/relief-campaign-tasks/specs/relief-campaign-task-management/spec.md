## ADDED Requirements

### Requirement: Relief campaign tasks MUST be team-owned operational work packages
The system SHALL allow relief campaign tasks to be created only for relief campaigns and only when attached to a valid campaign team belonging to that campaign.

#### Scenario: Create a task for a valid campaign team
- **WHEN** a user creates a task for a relief campaign with a `CampaignTeamId` that belongs to that campaign
- **THEN** the system SHALL create the task in `Planned` status
- **AND** the task SHALL store the provided title, description, dates, and priority

#### Scenario: Reject task creation when the team does not belong to the campaign
- **WHEN** a user creates a task with a `CampaignTeamId` that does not belong to the target campaign
- **THEN** the system SHALL reject the request

### Requirement: Relief campaign tasks MUST use a controlled status transition map
The system SHALL enforce the following transitions for relief campaign tasks:
- `Planned -> InProgress | Cancelled`
- `InProgress -> Blocked | Completed | Cancelled`
- `Blocked -> InProgress | Cancelled`

#### Scenario: Valid task transition succeeds
- **WHEN** a task in `Planned` status is changed to `InProgress`
- **THEN** the system SHALL persist the new status

#### Scenario: Invalid task transition is rejected
- **WHEN** a task in `Completed` status is changed to `InProgress`
- **THEN** the system SHALL reject the request

### Requirement: Relief task execution MUST depend on campaign active status
The system SHALL only allow execution-side task transitions (`InProgress`, `Blocked`, `Completed`) while the parent relief campaign is `Active`.

#### Scenario: Starting a task in an active campaign succeeds
- **WHEN** the parent relief campaign is `Active` and a task transitions from `Planned` to `InProgress`
- **THEN** the system SHALL allow the transition

#### Scenario: Starting a task in a non-active campaign fails
- **WHEN** the parent relief campaign is not `Active` and a task transitions to `InProgress`
- **THEN** the system SHALL reject the transition

### Requirement: Task completion MUST NOT mutate stock or household fulfillment directly
The system SHALL treat campaign task status as operational coordination only. Completing or cancelling a task SHALL NOT directly update inventory, supply allocation, supply transfer, package assembly, household delivery, or campaign household fulfillment records.

#### Scenario: Completing a task leaves stock and delivery truth unchanged
- **WHEN** a campaign task is marked `Completed`
- **THEN** the system SHALL update only task-related records
- **AND** stock and delivery status changes SHALL continue to require their own workflow endpoints

### Requirement: Relief campaign tasks MUST be queryable by campaign and team
The system SHALL provide task listing for a campaign with filters for status and campaign team.

#### Scenario: List tasks for a campaign
- **WHEN** a client requests tasks for a relief campaign
- **THEN** the system SHALL return tasks belonging to that campaign

#### Scenario: Filter tasks by campaign team
- **WHEN** a client requests tasks for a relief campaign filtered by `CampaignTeamId`
- **THEN** the system SHALL return only tasks owned by that team

## Context

The repository already includes a partial domain model for campaign tasking:
- `CampaignTask` is a team-owned task with title, description, dates, status, priority, and a required `CampaignTeamId`.
- `MemberTask` and `MemberTaskItem` suggest support for assigning portions of a campaign task to volunteers.
- `CampaignTaskItem` suggests optional line-item quantity tracking tied to allocation items.

However, the application layer currently exposes campaign teams but not campaign tasks. Relief operations are implemented through separate workflows for households, household deliveries, distribution points, package definitions, package assembly, shortage requests, supply allocation, and supply transfer. This means the system already has strong domain truth for stock, deliveries, and beneficiary fulfillment, but lacks a coordinated operational task layer for teams to execute work.

This change should make the existing task model usable for relief campaigns while preserving the separation of concerns already present in the backend.

## Goals / Non-Goals

**Goals:**
- Make `CampaignTask` usable for relief campaign operations through service and API workflows.
- Keep tasks team-owned and aligned with existing `CampaignTeam` assignment patterns.
- Support a simple, enforceable task state machine for relief execution.
- Ensure tasks coordinate operational work without becoming the source of truth for stock or delivery completion.
- Provide a minimal but extensible foundation for later member assignment and richer task context.

**Non-Goals:**
- Replace `CampaignHousehold`, `HouseholdDelivery`, or distribution points as the source of beneficiary fulfillment truth.
- Replace inventory, allocation, transfer, or package assembly as stock/accounting truth.
- Introduce a generic workflow engine for all campaign types in this phase.
- Implement task-driven stock reservation or automatic household completion in MVP.

## Decisions

### 1. Tasks are operational work packages, not fulfillment/accounting truth
Relief campaign tasks will represent work packages such as route execution, distribution-point shifts, package assembly assignments, or shortage follow-ups. They will not directly own stock truth, household fulfillment truth, or delivery proof.

**Why this choice:** The backend already has dedicated entities for those concerns. Using tasks as orchestration avoids duplication and inconsistent state.

### 2. Every relief task is owned by a campaign team
`CampaignTask.CampaignTeamId` will remain mandatory and be the primary ownership model. Tasks cannot exist without a valid team assignment inside the same campaign.

**Why this choice:** The domain already models tasks as team-owned. This matches relief operations better than member-only or station-only ownership.

### 3. Member assignment is optional and layered
`MemberTask` will be supported as a follow-up under a team task, but MVP task execution will not require member subtasks.

**Why this choice:** Many relief operations are team-level first. Member subtasks add value later without blocking core task management.

### 4. Task execution is only allowed when the relief campaign is active
Tasks may be created and edited in planning states, but status transitions into active execution (`InProgress`, `Completed`, `Blocked`) are only allowed when the campaign is `Active`.

**Why this choice:** Relief execution should respect the campaign lifecycle already enforced in `CampaignService`.

### 5. Use the existing task status enum with a strict transition map
The system will use:
- `Planned`
- `InProgress`
- `Blocked`
- `Completed`
- `Cancelled`

Allowed transitions:
- `Planned -> InProgress | Cancelled`
- `InProgress -> Blocked | Completed | Cancelled`
- `Blocked -> InProgress | Cancelled`

**Why this choice:** The enum already exists and fits relief operations without adding state complexity.

### 6. MVP task API stays focused on CRUD + status + list/detail
The first implementation will support:
- create task
- list tasks by campaign
- get task detail
- update task
- change task status
- cancel/delete task

Member assignment may be included in phase 2 of the same change or follow shortly after, depending on implementation effort.

**Why this choice:** This provides immediate frontend value without entangling tasks with every existing relief workflow.

### 7. Task context remains minimal in MVP
MVP will avoid adding extra context fields such as `DistributionPointId`, `ReliefStationId`, or `TaskType` unless implementation reveals they are essential. Frontend can still categorize tasks using title/description/priority/team for the first phase.

**Why this choice:** The existing entity is already usable for a first release. Adding more fields early increases schema and API complexity.

## Risks / Trade-offs

- **[Risk] Tasks may be mistaken for delivery truth** → Mitigation: keep service rules explicit that task completion does not auto-complete household deliveries or mutate stock.
- **[Risk] Existing `CampaignTaskItem`/`MemberTaskItem` may tempt over-coupling to inventory** → Mitigation: do not use them as stock reservation/accounting mechanisms in MVP.
- **[Trade-off] Minimal context fields reduce reporting richness initially** → This keeps implementation smaller but may require later extension for task categorization.
- **[Trade-off] Optional member subtasks means some teams operate at team level only** → Acceptable for MVP because team coordination is the primary relief need.

## Migration Plan

1. Reuse existing `CampaignTasks`, `CampaignTaskItems`, `MemberTasks`, and `MemberTaskItems` tables/entities.
2. Add repository abstractions and application DTOs/services without changing the database schema initially.
3. Add controller endpoints and service validation for task ownership and status transitions.
4. Validate task execution rules against relief campaign status.
5. If member assignment is included in this change, add endpoints on top of existing `MemberTask` tables without schema expansion.

## Open Questions

- Should MVP include member subtask endpoints immediately, or should member assignment be a follow-up after core task CRUD/status is stable?
- Does frontend need explicit task categorization (`TaskType`) now, or can title/description-based categorization work for the first release?
- Should cancelled tasks be hard-deleted or soft-retained as immutable operational history?

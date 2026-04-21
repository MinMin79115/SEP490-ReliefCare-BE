## Why

The backend already contains domain entities for `CampaignTask`, `CampaignTaskItem`, `MemberTask`, and `MemberTaskItem`, but there is no application or API workflow that makes them usable for relief campaigns. Relief operations currently rely on teams, households, deliveries, distribution points, allocations, transfers, and package assembly, but there is no task layer for coordinating team-owned work such as running distribution shifts, delivery routes, assembly jobs, or shortage follow-ups.

Without a task workflow, frontend and operations teams cannot plan, assign, track, block, and complete relief work packages in a way that aligns with existing campaign-team ownership and relief distribution flows.

## What Changes

- Introduce a usable relief campaign task workflow on top of the existing `CampaignTask` and `MemberTask` domain model.
- Add application services, DTOs, repository abstractions, and API endpoints for relief task CRUD, status transitions, and task listing.
- Enforce campaign/team/task rules so relief tasks are team-owned, executable only when the campaign is active, and do not directly mutate stock or beneficiary fulfillment state.
- Support basic member assignment as a follow-up layer under team-owned tasks without making member subtasks mandatory for MVP.
- Expose task data in a frontend-friendly way for relief campaign coordination screens.

## Capabilities

### New Capabilities
- `relief-campaign-task-management`: Create, list, update, cancel, and transition relief campaign tasks owned by campaign teams.
- `relief-campaign-task-assignment`: Assign optional member-level subtasks under team-owned campaign tasks.

### Modified Capabilities
- None.

## Impact

- Affected domain: `CampaignTask`, `CampaignTaskItem`, `MemberTask`, `MemberTaskItem`, `CampaignTeam`, `Campaign`.
- Affected application layer: new campaign task DTOs, service contracts, and validation rules.
- Affected API surface: new task endpoints under relief/campaign workflow.
- Affected frontend flows: relief campaign task board/list, task detail, task status controls, team/member assignment UI.

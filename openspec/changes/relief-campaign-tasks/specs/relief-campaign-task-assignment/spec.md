## ADDED Requirements

### Requirement: Member subtasks MUST belong to a campaign task owned by the same campaign team
The system SHALL allow optional member-level assignment under a campaign task, but member assignments SHALL remain scoped to the owning campaign team.

#### Scenario: Assign a member subtask under a team-owned task
- **WHEN** a user assigns a member subtask to a volunteer belonging to the owning team of the campaign task
- **THEN** the system SHALL create the member task in `Assigned` status

#### Scenario: Reject member assignment outside the owning team
- **WHEN** a user assigns a member subtask to a volunteer who does not belong to the task's campaign team
- **THEN** the system SHALL reject the request

### Requirement: Member subtask states MUST not override campaign task truth automatically
The system SHALL keep member subtask status separate from parent campaign task status. Parent task completion SHALL remain an explicit action.

#### Scenario: Completing one member subtask does not auto-complete the parent task
- **WHEN** a member task is marked `Completed`
- **THEN** the parent campaign task SHALL remain unchanged unless explicitly updated

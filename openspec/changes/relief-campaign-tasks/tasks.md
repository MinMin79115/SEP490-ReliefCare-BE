## 1. Task application foundation

- [x] 1.1 Add campaign task and member task repository abstractions on top of existing entities/tables
- [x] 1.2 Add relief campaign task DTOs for create, update, detail, list, and status change flows
- [x] 1.3 Add `ICampaignTaskService` and `CampaignTaskService` for task orchestration and validation

## 2. Relief campaign task business rules

- [x] 2.1 Enforce that tasks can only be created for relief campaigns and valid campaign teams within the same campaign
- [x] 2.2 Implement the relief task status transition map (`Planned`, `InProgress`, `Blocked`, `Completed`, `Cancelled`)
- [x] 2.3 Gate execution-side transitions so they are only allowed when the parent relief campaign is `Active`
- [x] 2.4 Ensure task completion and cancellation do not directly mutate stock, allocation, transfer, delivery, or household fulfillment records

## 3. Task API surface

- [x] 3.1 Add endpoints to create, list, get detail, update, change status, and cancel/delete campaign tasks
- [x] 3.2 Add task list filters for campaign team and task status
- [x] 3.3 Return frontend-friendly task responses with team context and scheduling fields

## 4. Optional member assignment layer

- [x] 4.1 Add member task DTOs and service methods for assigning member subtasks under a campaign task
- [x] 4.2 Validate that assigned members belong to the owning campaign team
- [x] 4.3 Ensure member subtask status does not auto-complete the parent campaign task

## 5. Validation and verification

- [ ] 5.1 Add tests for relief-only task creation, team ownership validation, and task status transitions
- [ ] 5.2 Add tests for campaign active gating on execution-side task transitions
- [ ] 5.3 Add tests confirming task completion does not mutate stock or household delivery state
- [ ] 5.4 Add tests for member assignment constraints if member subtasks are included in implementation

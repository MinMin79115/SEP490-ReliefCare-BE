## 1. Domain and data model

- [x] 1.1 Add relief-distribution entities for campaign households, distribution points, package definitions, delivery records, proof records, and shortage requests
- [x] 1.2 Add supporting enums and relationships for delivery mode, fulfillment status, and shortage-request status
- [x] 1.3 Create and review database migrations for the new relief-distribution schema

## 2. Application services and orchestration

- [x] 2.1 Add repository contracts and infrastructure repositories for the new relief-distribution entities
- [x] 2.2 Implement campaign household management services for create/import, assignment, and checklist queries
- [x] 2.3 Implement distribution-point and package-definition services for relief campaigns
- [x] 2.4 Implement household delivery completion with required photo proof
- [x] 2.5 Implement shortage-request approval flow that orchestrates existing allocation/transfer and inventory transaction services

## 3. API surface

- [x] 3.1 Add API endpoints for campaign households and checklist views
- [x] 3.2 Add API endpoints for distribution points and package definitions
- [x] 3.3 Add API endpoints for household delivery completion and proof upload
- [x] 3.4 Add API endpoints for shortage request submission and moderator approval/rejection

## 4. Validation and verification

- [x] 4.1 Add validations for relief-only flows, household assignment rules, and proof-required delivery completion
- [ ] 4.2 Add tests for package creation, checklist completion, isolated-household delivery, and shortage approval flows
- [x] 4.3 Run diagnostics and validate the OpenSpec change before implementation begins

## 1. Campaign operational inventory foundation

- [x] 1.1 Add domain entities, repositories, and EF mappings for campaign inventory, campaign inventory stock, and campaign stock transaction history
- [x] 1.2 Create database migrations for campaign operational inventory persistence and transaction linkage
- [x] 1.3 Add service-layer abstractions for creating, querying, and mutating campaign operational stock atomically

## 2. Supply allocation and campaign balance integration

- [x] 2.1 Update supply allocation approval to export stock from source station inventory and import the same quantities into campaign operational inventory in one transaction
- [x] 2.2 Update supply allocation cancellation rules to reverse campaign stock only when reversal is still valid against consumed balances
- [x] 2.3 Update campaign inventory balance queries and DTOs to report persisted campaign-owned stock instead of station-inventory-only aggregates

## 3. Campaign package consumption flows

- [x] 3.1 Update package assembly availability calculation to use campaign operational stock as the source of consumable component inventory
- [x] 3.2 Update package assembly execution to consume component stock and produce package output stock in campaign inventory with audit records
- [x] 3.3 Update household delivery completion to consume campaign package stock before marking deliveries completed
- [x] 3.4 Add campaign-team and distribution-context references to campaign stock consumption records for reporting

## 4. Lot-based station inventory tracking

- [ ] 4.1 Add inventory lot entities, repository queries, and EF mappings for per-receipt batch, cost, expiry, and remaining quantity tracking
- [ ] 4.2 Create migrations and backfill logic to seed opening lots from existing aggregate inventory stock rows
- [ ] 4.3 Update inventory import logic to create a new lot for each import while keeping aggregate stock summaries synchronized
- [ ] 4.4 Update inventory export logic to deplete lots by FEFO then FIFO while keeping aggregate stock summaries synchronized
- [ ] 4.5 Extend inventory history responses or queries to expose on-hand lots and lot-consumption traceability

## 5. Relief campaign lifecycle fixes

- [x] 5.1 Centralize relief lifecycle transition and readiness rules so status patch and edit validations use the same source of truth
- [x] 5.2 Fix campaign update/editability checks for relief campaigns to align with `ReadyToExecute`, `InProgress`, and terminal-state semantics
- [x] 5.3 Extend campaign status APIs or responses to expose allowed next statuses for relief campaigns

## 6. Validation and rollout verification

- [ ] 6.1 Add tests for allocation-to-campaign-stock movement, package assembly against campaign stock, and delivery completion stock consumption
- [ ] 6.2 Add tests for lot creation on import, FEFO/FIFO depletion on export, and aggregate stock synchronization
- [ ] 6.3 Add tests for valid and invalid relief campaign status transitions and readiness failures
- [ ] 6.4 Validate the OpenSpec change and verify migrations, API behavior, and backward-compatible read paths before implementation rollout

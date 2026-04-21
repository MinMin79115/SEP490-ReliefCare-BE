## Context

The repository already uses `Campaign.BudgetTotal` and `Campaign.BudgetSpent` as the operational money counters for campaigns. Fundraising donations currently increase `Campaign.BudgetTotal` when completed, and procurement already spends against `BudgetTotal - BudgetSpent`. Relief distribution also already has a transactional completion flow for package stock deduction, but there is no explicit logic for extracting money from a fundraising campaign into a relief campaign or for deducting money-bearing package support during relief delivery.

The system also contains a central `Fund` ledger used for donation pool reporting, but campaign operations today are controlled by campaign budget fields rather than by fund balance debits. To keep the implementation compatible and minimal, this change should continue using campaign budget counters as the campaign-level operational source of truth and avoid introducing a new wallet subsystem.

## Goals / Non-Goals

**Goals:**
- Support extracting available budget from a fundraising campaign into a relief campaign.
- Support optional per-package cash support and deduct it when delivery is completed.
- Prevent overspending in both extraction and distribution flows.
- Preserve traceability for transfer and distribution money usage.
- Keep the implementation compatible with existing budget/procurement/reporting behavior.

**Non-Goals:**
- Replace the central `Fund` subsystem as the donation pool ledger.
- Introduce a full wallet/accounting engine or double-entry bookkeeping subsystem.
- Change package inventory or delivery proof workflows beyond the cash-support addition.
- Add a generic multi-campaign finance allocation module beyond the targeted extraction flow.

## Decisions

### 1. Reuse `Campaign.BudgetTotal` and `Campaign.BudgetSpent`
Campaign money state will continue to use the existing budget counters.

#### Semantics
- **Fundraising campaign**
  - `BudgetTotal` = total donated money accumulated
  - `BudgetSpent` = total money extracted/transferred out into relief campaigns
- **Relief campaign**
  - `BudgetTotal` = total money allocated into the relief campaign
  - `BudgetSpent` = money consumed by procurement and cash-support distribution

**Why this choice:** The repo already treats these fields as campaign-level money state. Reusing them avoids introducing a second operational ledger.

### 2. Add optional `CashSupportAmount` on `ReliefPackageDefinition`
Relief packages will support an optional per-package cash value.

**Why this choice:** Moderators distribute either package-only or package-plus-money. Package definition is the right place to store the default money amount.

### 3. Snapshot cash support on `HouseholdDelivery`
When a household delivery is assigned or completed, the delivery record will store the actual `CashSupportAmount` used for that delivery.

**Why this choice:** Package definitions are mutable. Delivery records need immutable history for audit and reconciliation.

### 4. Deduct relief budget only on delivery completion
Relief campaign budget will be deducted during `CompleteHouseholdDeliveryInternalAsync(...)`, not at assignment time.

**Why this choice:** Assignment is planning; completion is the real-world action. This avoids premature budget reservation and reversal complexity.

### 5. Add one lightweight budget transfer history entity
Use a small dedicated entity such as `CampaignBudgetTransfer` to log fundraising-to-relief extractions.

**Why this choice:** `AuditLog` is too generic for finance reporting, while a dedicated transfer history is minimal but queryable.

### 6. Keep `Fund` as donation pool reporting, not operational spending control
The central `Fund` subsystem will continue to track donation contributions and fund summary reporting, but relief budget extraction and distribution spending will be controlled at campaign budget level.

**Why this choice:** This avoids a risky midstream refactor of all spending flows into the fund subsystem.

## Risks / Trade-offs

- **[Risk] Semantics of `BudgetSpent` differ by campaign type** → Mitigation: document the type-specific meaning clearly and expose remaining budget consistently as `BudgetTotal - BudgetSpent`.
- **[Risk] Package cash support can drift if package definitions change after assignment** → Mitigation: snapshot the cash amount on `HouseholdDelivery`.
- **[Risk] Failed overspend attempts may not be captured by generic entity audit** → Mitigation: add explicit logging for extraction/distribution failures.
- **[Trade-off] `Fund` and campaign budgets remain separate views of money** → Acceptable for minimal compatible implementation; central fund remains reporting-oriented while campaign budgets drive operations.

## Migration Plan

1. Add `CashSupportAmount` to `ReliefPackageDefinition` and `HouseholdDelivery`.
2. Add `CampaignBudgetTransfer` entity/table and repository access.
3. Extend relief package and delivery DTOs to carry cash support amount.
4. Add extraction method to `CampaignService` and a controller endpoint for managers.
5. Update relief delivery completion to deduct budget in the same DB transaction as inventory deduction.
6. Extend responses/summaries where needed to expose the new values.
7. Add tests for extraction limits, relief overspending prevention, and delivery cash support auditability.

## Open Questions

- Should relief package cash support be editable per delivery assignment, or should the initial implementation use the package default only?
- Should batch delivery completion support custom per-item cash overrides, or simply use the delivery snapshot value?
- Is a public campaign summary expected to separate fundraising extracted-out money from actual relief spending, or is `BudgetSpent` sufficient for now?

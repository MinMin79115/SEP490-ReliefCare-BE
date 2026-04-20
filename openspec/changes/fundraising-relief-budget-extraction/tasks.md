## 1. Budget extraction foundation

- [ ] 1.1 Add a lightweight `CampaignBudgetTransfer` entity, mapping, repository access, and migration for extraction history
- [ ] 1.2 Add DTOs for manager-triggered fundraising-to-relief budget extraction
- [ ] 1.3 Add `CampaignService` logic and controller endpoint to extract budget atomically between campaigns

## 2. Financial validation rules

- [ ] 2.1 Enforce fundraising remaining-budget checks before extraction
- [ ] 2.2 Enforce valid campaign type pairing (Fundraising -> Relief only)
- [ ] 2.3 Add explicit logging for failed extraction attempts and failed overspend distribution attempts

## 3. Relief package cash support

- [ ] 3.1 Add `CashSupportAmount` to `ReliefPackageDefinition` and extend create/update/read DTOs accordingly
- [ ] 3.2 Validate package cash support amount is non-negative
- [ ] 3.3 Expose package cash support in relief package responses for frontend use

## 4. Relief distribution budget deduction

- [ ] 4.1 Add `CashSupportAmount` snapshot field to `HouseholdDelivery` and extend delivery request/response DTOs
- [ ] 4.2 Populate delivery cash support amount from the assigned package definition (or request value if supported) without breaking current assignment flow
- [ ] 4.3 Deduct relief campaign budget during delivery completion in the same transaction as package stock deduction
- [ ] 4.4 Reject delivery completion when cash support exceeds remaining relief campaign balance with the required business message
- [ ] 4.5 Persist distribution history with package, quantity, total money used, time, and actor

## 5. Verification and regression coverage

- [ ] 5.1 Add tests for successful and failed budget extraction
- [ ] 5.2 Add tests for package-with-money and package-only delivery completion behavior
- [ ] 5.3 Add tests for overspending rejection on relief campaign distribution
- [ ] 5.4 Validate summaries and existing procurement budget usage remain compatible with the new budget semantics

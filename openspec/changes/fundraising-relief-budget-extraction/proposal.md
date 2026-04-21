## Why

The current backend lets fundraising donations increase campaign money counters and supports relief package distribution, but it does not explicitly support the business flow where a manager extracts money from a fundraising campaign into a relief campaign budget and moderators then spend that relief budget when distributing packages that may include cash support.

Without this flow, the system cannot enforce the required financial constraints:
- prevent extracting more money than a fundraising campaign has available,
- prevent moderators from distributing more cash support than a relief campaign has remaining,
- keep a traceable history of extracted budget and money-bearing package distributions.

## What Changes

- Add a manager-driven budget extraction flow from a fundraising campaign into a relief campaign.
- Add optional cash support amount to relief package definitions.
- Snapshot package cash support onto household deliveries and deduct relief campaign budget when deliveries are completed.
- Enforce overspending checks for both budget extraction and relief distribution.
- Persist business-level transfer/distribution history and ensure all balance changes and failures are logged.

## Capabilities

### New Capabilities
- `campaign-budget-extraction`: transfer available budget from a fundraising campaign into a relief campaign.
- `relief-package-cash-support`: define optional cash support value on a relief package definition and deduct it at delivery completion.

### Modified Capabilities
- `donation-budget-accumulation`: fundraising donations continue to accumulate into campaign budget totals, but fundraising campaign spent budget will now also represent extracted-out budget.
- `relief-delivery-completion`: delivery completion now also validates and deducts monetary support from the relief campaign budget when applicable.

## Impact

- Affected domain: `Campaign`, `ReliefPackageDefinition`, `HouseholdDelivery`, and a new lightweight transfer history entity for campaign budget extraction.
- Affected services: `CampaignService`, `ReliefDistributionService`, and existing donation/fund summary flows that expose campaign budget fields.
- Affected API surface: new extraction endpoint, updated relief package requests/responses, updated delivery completion request/response payloads.
- Affected reporting/audit: budget transfer history, package cash support tracking per delivery, and failed overspend attempts.

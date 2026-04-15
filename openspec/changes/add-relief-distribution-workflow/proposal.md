## Why

The current backend has campaign, inventory, procurement, allocation, and team-assignment primitives, but it does not support the actual relief-distribution workflow after a disaster. The project needs a campaign-level flow to manage affected households, temporary distribution points, package-based aid delivery, checklist completion with photo proof, and shortage approval while reusing the existing inventory transaction backbone.

## What Changes

- Add relief-distribution planning for `Campaign` entries of type `Relief`.
- Add campaign-scoped affected household management to represent government-provided beneficiary lists.
- Add campaign-scoped temporary distribution points for organizing pickup-based aid delivery.
- Add relief package definitions so managers can define standard aid bundles from existing supply items.
- Add household delivery checklist tracking with required photo proof for successful delivery completion.
- Add shortage-request approval flow for distribution operations that reuses existing inventory allocation/transfer and transaction patterns.
- Define MVP scope boundaries so the first release focuses on package distribution and traceability, not route optimization or advanced beneficiary scoring.

## Capabilities

### New Capabilities
- `relief-household-management`: Manage campaign-scoped affected households and assign them to distribution or direct-delivery flows.
- `relief-distribution-operations`: Manage distribution points, relief packages, household delivery checklist completion, and shortage handling for relief campaigns.

### Modified Capabilities

- None.

## Impact

- Affected domain areas: `Campaign`, relief operations, supply allocation, supply transfer, attachments/proof handling, and inventory transaction orchestration.
- Affected API areas: new endpoints for households, distribution points, package definitions, delivery completion, and shortage approval.
- Affected data model: new campaign-scoped beneficiary, distribution, package, delivery-proof, and shortage-request entities.
- Reused systems: procurement, inventory, allocation, transfer, campaign-team assignment, and campaign task assignment.

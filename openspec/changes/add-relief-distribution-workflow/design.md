## Context

The current backend already supports campaign creation, relief-station attachment, team assignment, inventory, procurement, supply allocation, and supply transfer. That existing model is sufficient for upstream sourcing and stock accounting, but it does not model the final-mile relief workflow described for post-disaster support in the first 1 to 7 days.

The desired workflow is campaign-centric:
- manager creates a `Relief` campaign
- a government-provided household list is loaded into the campaign
- households are either assigned to temporary distribution points or marked for direct delivery if isolated
- the campaign defines standard relief packages made from existing supply items
- volunteers complete a household checklist and must upload photo proof when marking delivery as completed
- distribution shortages require moderator approval and must produce inventory-backed movement records

The design must reuse existing stock-accounting primitives instead of creating a second inventory subsystem.

## Goals / Non-Goals

**Goals:**
- Add first-class beneficiary tracking for relief campaigns.
- Add lightweight temporary distribution-point modeling without overloading permanent relief stations.
- Add package definitions that map existing supply items into standard aid bundles.
- Add per-household delivery completion records with photo proof.
- Add shortage-request approval that orchestrates existing supply allocation/transfer and inventory transaction flows.
- Keep assignment concerns separate from beneficiary fulfillment concerns.

**Non-Goals:**
- Route optimization, ETA planning, or advanced logistics batching.
- Household-level eligibility scoring, deduplication engine, or national master-data management.
- Offline-first sync, GPS-based proof validation, digital signatures, or OCR identity verification.
- Full point-level warehouse subsystem or vehicle inventory tracking.
- Generalizing the existing request-based attachment model across the whole platform in this change.

## Decisions

### 1. Model affected households as first-class campaign entities

**Decision:** Add a campaign-scoped household entity instead of representing each household as a generic task.

**Rationale:**
- The user's core workflow is a beneficiary checklist, not a task list.
- Each household needs identity, address, isolated status, assignment target, and fulfillment state.
- Reusing `MemberTask` or `CampaignTask` as the primary household record would mix operational assignment with beneficiary truth and create poor reporting.

**Alternatives considered:**
- Reuse `MemberTask` for each household: rejected because it overloads task tables and weakens auditability.
- Reuse `Request` hierarchy: rejected because household relief recipients are not incoming requests and do not match request verification semantics.

### 2. Add temporary distribution points separate from relief stations

**Decision:** Add a campaign-scoped distribution-point entity rather than reusing `ReliefStation` as the pickup site.

**Rationale:**
- `ReliefStation` is a persistent operational node with inventory, moderators, teams, and transfer relationships.
- A distribution point in this use case is a lightweight temporary pickup location for a specific campaign.
- Separating the two keeps the data model clear and avoids fake station/inventory records for every temporary site.

**Alternatives considered:**
- Use `ReliefStation` for distribution points: rejected due to semantic mismatch and unnecessary operational complexity.

### 3. Add package definitions instead of materializing package instances for every household

**Decision:** Introduce a package definition with package items, scoped to a campaign.

**Rationale:**
- The user explicitly wants standardized packages such as "5kg gạo + 3 chai nước".
- Package definitions allow planning total demand from existing supply items without creating heavy per-household packing objects.
- Actual fulfillment differences can be captured later via delivery-item rows if needed.

**Alternatives considered:**
- Only use flat supply item lines everywhere: rejected because package planning and household assignment become harder to reason about.
- Create one package instance per household upfront: deferred because it adds complexity without immediate benefit for MVP.

### 4. Use household delivery records as the checklist truth

**Decision:** Add a dedicated household-delivery aggregate for checklist completion and proof.

**Rationale:**
- The user's operational UI is a checklist of households under a distribution point or direct-delivery route.
- Successful completion must require photo proof.
- Delivery records must support normal pickup and isolated-household direct delivery in one model.

**Alternatives considered:**
- Mark status directly on household rows only: rejected because it cannot capture evidence, actor, timestamps, or repeated workflow attempts cleanly.
- Attach proof to tasks only: rejected because proof belongs to beneficiary fulfillment, not just staff assignment.

### 5. Keep existing inventory/accounting systems as source of truth

**Decision:** Reuse `ProcurementOrder`, `SupplyAllocation`, `SupplyTransfer`, and `InventoryTransaction` for supply movement and stock accounting.

**Rationale:**
- The current codebase already has working procurement, issue, transfer, and inventory-ledger flows.
- The relief module should orchestrate these primitives, not replace them.
- Shortage approval must produce inventory-backed movement records consistent with the rest of the system.

**Alternatives considered:**
- Create a separate relief stock subsystem: rejected because it duplicates accounting logic and increases reconciliation risk.

### 6. Keep shortage approval as a business request layer above stock movement

**Decision:** Introduce a shortage-request object and let approved requests trigger the existing allocation/transfer machinery.

**Rationale:**
- A shortage request is an approval workflow artifact.
- A transfer or allocation is an execution artifact.
- Separating them gives cleaner auditability and matches the user's expectation that a moderator reviews requests before stock changes happen.

**Alternatives considered:**
- Use `SupplyTransfer` directly as the request: acceptable only for some cross-station flows, but too narrow for all shortage cases.

### 7. Keep tasks for assignment, not for beneficiary identity

**Decision:** Reuse `CampaignTask` and `MemberTask` for distribution sessions/routes and volunteer assignment, while keeping household data in new relief entities.

**Rationale:**
- Existing task models are still useful for assigning operational work.
- This separation lets the system answer both "who is assigned?" and "which households have actually received aid?" without overloading one model.

## Risks / Trade-offs

- **[Risk] New relief entities increase migration surface area** → Mitigation: keep MVP additions focused on households, points, packages, delivery records, and shortage requests only.
- **[Risk] Delivery completion may drift from stock reality if deduction rules are unclear** → Mitigation: keep inventory transaction logic anchored to existing allocation/transfer flows and define clear orchestration boundaries in implementation.
- **[Risk] Reusing `SupplyAllocation` without explicit destination semantics can create ambiguity** → Mitigation: implementation should add destination metadata or a clear linking strategy for distribution operations.
- **[Risk] Proof handling may duplicate attachment concerns across domains** → Mitigation: use a relief-specific proof model in MVP and defer cross-domain attachment generalization.
- **[Risk] Scope creep into routing, logistics, and advanced beneficiary management** → Mitigation: explicitly exclude those concerns from this change and keep the spec centered on package-based distribution traceability.

## Migration Plan

1. Add new relief-distribution entities and enums through database migration.
2. Add repositories and service-layer orchestration for households, points, packages, deliveries, and shortage requests.
3. Add API endpoints for campaign relief operations.
4. Integrate approved shortage requests with existing stock-movement services.
5. Validate behavior with end-to-end scenarios covering household assignment, checklist completion, proof upload, and shortage approval.

Rollback strategy:
- Because this change is additive for MVP, rollback can disable new endpoints and revert the migration if deployment must be undone before operational data is relied upon.

## Open Questions

- Should a household be assigned exactly one package definition in MVP, or can managers choose a package at delivery time?
- Should stock be decremented at allocation time, delivery-completion time, or both depending on the flow?
- Should direct-delivery households be grouped under distribution tasks/routes explicitly in MVP, or remain team-assigned only?
- Does the first release need household import versioning, or is a single imported list per campaign enough?

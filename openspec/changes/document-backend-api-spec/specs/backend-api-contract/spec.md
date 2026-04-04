## ADDED Requirements

### Requirement: Backend SHALL publish a complete endpoint contract catalog
The system SHALL maintain a complete and current contract catalog for all HTTP endpoints exposed by backend controllers, including method, route template, authorization requirement, request binding type, and response semantics.

#### Scenario: Generate full endpoint matrix from controllers
- **WHEN** a reviewer inspects the API contract capability
- **THEN** every controller action in the backend is listed with HTTP method and full route template
- **AND** each listed action includes authorization mode (anonymous/authenticated/role-restricted)
- **AND** each listed action includes request source classification (path/query/body/form)

#### Scenario: Detect route ambiguity risks
- **WHEN** a route parameter lacks explicit constraint while sibling literal routes exist or may be introduced
- **THEN** the contract catalog flags the route as ambiguity risk
- **AND** the catalog records a recommendation to add route constraint where applicable

### Requirement: Mutating endpoints MUST define explicit authorization policy
All mutating endpoints (POST, PUT, PATCH, DELETE) MUST declare an explicit authorization policy, unless the endpoint is intentionally public and documented with justification.

#### Scenario: Mutating endpoint with role-based protection
- **WHEN** a mutating endpoint is intended for internal operators
- **THEN** the endpoint is marked as authenticated with required role/policy in the contract

#### Scenario: Intentionally public mutating endpoint
- **WHEN** a mutating endpoint must be public by business design
- **THEN** the contract marks it as public
- **AND** the contract includes explicit rationale for anonymous access

#### Scenario: Missing policy on mutating endpoint
- **WHEN** a mutating endpoint lacks explicit authorization declaration
- **THEN** the endpoint is reported as a contract violation
- **AND** remediation action is recorded in implementation tasks

### Requirement: Resource creation MUST use consistent HTTP creation semantics
Resource creation endpoints MUST return `201 Created` on success and provide resource identification to clients.

#### Scenario: Successful create operation
- **WHEN** a client creates a new resource through a create endpoint
- **THEN** the API returns `201 Created`
- **AND** the response includes resource identifier or canonical location reference

#### Scenario: Legacy create endpoint returns 200
- **WHEN** an existing create endpoint still returns `200 OK`
- **THEN** the contract marks it as non-compliant
- **AND** migration work is scheduled to align behavior with `201 Created`

### Requirement: Delete operations MUST use consistent deletion semantics
Delete endpoints MUST return `204 No Content` when deletion succeeds and no response payload is needed.

#### Scenario: Successful delete without payload
- **WHEN** a delete operation succeeds
- **THEN** the API returns `204 No Content`
- **AND** no response body is required

#### Scenario: Delete endpoint currently returns message body
- **WHEN** a delete endpoint returns `200 OK` with ad-hoc message
- **THEN** the contract identifies the inconsistency
- **AND** implementation tasks define transition to `204 No Content` or justified exception policy

### Requirement: Backend MUST enforce a unified error response contract
All non-success responses MUST conform to one unified error contract across modules, including HTTP status, machine-readable error identity, human-readable message, and trace/correlation identifier.

#### Scenario: Validation failure response
- **WHEN** a request fails validation
- **THEN** the API returns a standardized error payload with status and validation details
- **AND** the payload includes trace/correlation identifier for diagnostics

#### Scenario: Domain/business exception response
- **WHEN** a business rule prevents operation completion
- **THEN** the API maps the exception to a standardized error payload
- **AND** clients receive consistent error fields regardless of module

#### Scenario: Unhandled exception response
- **WHEN** an unhandled exception occurs
- **THEN** the global exception handling path emits the unified error contract
- **AND** sensitive internal stack details are not exposed in production payloads

### Requirement: API success payload shape MUST be consistent per endpoint category
The API SHALL define and follow consistent success response shape rules for single-resource, collection, and paginated endpoints.

#### Scenario: Single-resource retrieval
- **WHEN** a single-resource GET endpoint succeeds
- **THEN** the response shape follows the agreed single-resource contract pattern

#### Scenario: Paginated list retrieval
- **WHEN** a paginated endpoint succeeds
- **THEN** the response includes data collection and paging metadata under a consistent schema

#### Scenario: Controller returns ad-hoc anonymous payload
- **WHEN** an endpoint returns ad-hoc anonymous object not aligned with contract pattern
- **THEN** the endpoint is recorded as non-compliant in contract review results

### Requirement: API route naming conventions MUST be standardized
Endpoint route naming MUST follow one consistent naming convention across all modules.

#### Scenario: Mixed naming styles detected
- **WHEN** route templates include mixed casing/style patterns across modules
- **THEN** the contract flags the inconsistency
- **AND** migration tasks define canonical style and transition plan

#### Scenario: New endpoint introduction
- **WHEN** a new endpoint is added after contract adoption
- **THEN** the endpoint route MUST comply with the standardized naming convention

### Requirement: Request binding and content type expectations MUST be explicit
Each endpoint SHALL document expected request binding source and content type requirements to avoid ambiguity for clients.

#### Scenario: Form-bound endpoint
- **WHEN** an endpoint expects form-data input
- **THEN** the contract explicitly marks form binding and required fields/content type

#### Scenario: JSON body endpoint
- **WHEN** an endpoint expects JSON payload
- **THEN** the contract explicitly marks body binding and schema type

#### Scenario: Query-driven filtering endpoint
- **WHEN** an endpoint supports filtering or paging via query parameters
- **THEN** the contract lists required and optional query parameters and their semantics

### Requirement: Contract compliance review MUST produce actionable gap list
The backend API contract process SHALL produce a compliance report that maps current implementation to contract requirements with prioritized remediation actions.

#### Scenario: Initial baseline review
- **WHEN** the contract capability is introduced
- **THEN** a baseline gap list is produced for all modules
- **AND** each gap is classified by severity (security, contract-breaking, consistency)

#### Scenario: Ongoing change review
- **WHEN** new endpoints or controller changes are proposed
- **THEN** contract compliance is re-evaluated
- **AND** non-compliant changes cannot be considered complete without remediation or approved exception

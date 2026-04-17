## ADDED Requirements

### Requirement: Relief package definitions SHALL identify the package output supply item
The system SHALL allow each relief package definition to identify the `SupplyItem` that represents the assembled package in inventory stock.

#### Scenario: Create package definition with output supply item
- **WHEN** a manager creates a relief package definition
- **THEN** the definition stores the output supply item that will be increased in stock after package assembly

#### Scenario: Reject package definition without output supply item
- **WHEN** a caller attempts to create or update a package definition without an output supply item
- **THEN** the system rejects the request

### Requirement: Package definitions SHALL only use non-package supply items as components
The system SHALL reject any package definition component that references a supply item categorized as a package.

#### Scenario: Reject package component categorized as package
- **WHEN** a caller includes a package-category supply item as a component in a package definition
- **THEN** the system rejects the request

### Requirement: The system SHALL calculate assemblable package quantity from current inventory stock
The system SHALL provide a way to calculate the maximum number of packages that can be assembled from a selected inventory using the quantities defined in a relief package definition.

#### Scenario: Calculate maximum assemblable quantity
- **WHEN** a user requests assembly availability for a package definition against an inventory
- **THEN** the system returns the maximum number of packages that can be assembled based on current stock of all required components

### Requirement: Warehouse package assembly SHALL consume component stock and increase package stock
The system SHALL support a package-assembly workflow that consumes component supply-item stock from an inventory and increases the stock of the package output supply item in the same warehouse context.

#### Scenario: Assemble packages from inventory
- **WHEN** an authorized user assembles a quantity of packages from an inventory using a package definition
- **THEN** the system decreases the component supply-item stock quantities according to the package definition and increases the stock quantity of the output package supply item by the assembled quantity

#### Scenario: Reject assembly when component stock is insufficient
- **WHEN** an authorized user attempts to assemble more packages than the current inventory can support
- **THEN** the system rejects the assembly request and does not mutate stock

### Requirement: Package assembly SHALL preserve inventory transaction traceability
The system SHALL reuse the inventory transaction backbone so that stock changes caused by package assembly remain traceable in inventory history.

#### Scenario: Assembly creates traceable inventory movements
- **WHEN** a package assembly operation succeeds
- **THEN** the resulting component consumption and package output are traceable through inventory transaction records

### Requirement: Package assembly SHALL record auditable assembly history
The system SHALL store explicit package-assembly history showing where, when, by whom, and from which components a package assembly operation was performed.

#### Scenario: View assembly history
- **WHEN** a user requests package-assembly history for a station or campaign
- **THEN** the system returns assembly records with produced quantity, operator, timestamp, and consumed component details

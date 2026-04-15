## 1. Data model and stock semantics

- [x] 1.1 Extend relief package definitions to store the output package supply item reference
- [x] 1.2 Add package-assembly log entities and component-detail entities for audit history
- [x] 1.3 Add or clarify transaction reason support for package-assembly stock movements
- [x] 1.4 Create database migrations for package assembly changes

## 2. Application services and orchestration

- [x] 2.1 Add package-definition validation so package-category supply items cannot be selected as components
- [x] 2.2 Add service logic to calculate maximum assemblable quantity by inventory and package definition
- [x] 2.3 Add package-assembly execution logic that consumes component stock and increases output package stock through inventory transactions
- [x] 2.4 Add package-assembly history queries scoped by campaign/station/package definition

## 3. API surface

- [x] 3.1 Extend relief package definition APIs to support output supply item selection
- [x] 3.2 Add APIs for assembly availability and package-assembly execution
- [x] 3.3 Add APIs for package-assembly history retrieval

## 4. Validation and verification

- [x] 4.1 Add validations for insufficient stock, invalid package components, and invalid output package item selection
- [ ] 4.2 Add tests for availability calculation, successful assembly, rejected assembly, and assembly history retrieval
- [x] 4.3 Validate the OpenSpec change and verify implementation behavior before rollout

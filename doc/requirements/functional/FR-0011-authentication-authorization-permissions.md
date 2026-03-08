---
status: "draft"
date:
---

# User Authorization by Permissions

## Statement
The system shall authorize access to protected operations based on the authenticated user's permissions.

## Rationale
Ensure that authenticated users can only perform operations they are permitted to execute.

## Acceptance Criteria
1. The system shall evaluate the user's permissions before executing a protected operation.
2. The system shall allow the operation when the user has the required permission.
3. The system shall reject the operation when the user does not have the required permission.
4. The system shall return an authorization error for insufficient permissions.
5. Authorization shall be applied consistently across protected operations.

## Verification Method
Test and Inspection

## More Information
- Permissions may be represented as claims or mapped from stored user permissions.

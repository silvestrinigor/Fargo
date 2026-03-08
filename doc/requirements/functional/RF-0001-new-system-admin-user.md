---
status: "draft"
date:
---

# New System Admin User

## Statement
The system shall create a default administrator user during system initialization if no users exist in the system.

## Rationale
Ensure that a newly deployed system always has at least one user with full permissions to manage the system.

## Acceptance Criteria
1. When the system initializes and no users exist in the system, the system shall create a default administrator user.
2. The created administrator user shall have all available permissions.
3. If at least one user already exists in the system, the system shall not create the default administrator user.
4. The default administrator user shall be created only once.

## Verification Method
Test | Inspection

## More Information
- The administrator user is defined as a user that possesses all system permissions.
- This behavior occurs only during system initialization.
- The administrator credentials should be defined through configuration parameters (for example: environment variables or application configuration).

System Start
      │
      ▼
Check if any user exists
      │
 ┌────┴─────┐
 │          │
No         Yes
 │          │
Create      Do nothing
Admin

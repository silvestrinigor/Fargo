---
status: "draft"
date:
---

# New System Admin User Default Nameid

## Statement
The system shall set the default administrator user NameId using the value provided in the environment configuration.

## Rationale
Ensure that the identifier of the default administrator user can be configured during deployment without modifying the application code.

## Acceptance Criteria
1. When the system creates the default administrator user, the NameId shall be read from environment configuration.
2. The NameId value shall be used as the identifier of the created administrator user.
3. If the environment configuration for the default administrator NameId is missing or empty, the system shall fail initialization or report a configuration error.
4. The configured NameId shall only be used when creating the default administrator user.

## Verification Method
Test and Inspection

## More Information
- The default administrator NameId should be provided through environment variables or equivalent configuration mechanisms.
- This requirement applies only when the system creates the default administrator user during system initialization.

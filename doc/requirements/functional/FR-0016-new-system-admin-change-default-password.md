---
status: "draft"
date:
---

# Default Administrator Password Change Requirement

## Statement
The system shall require the default administrator user to change the configured default password after the first successful authentication.

## Rationale
Reduce the security risk associated with long-term use of deployment-time default credentials.

## Acceptance Criteria
1. The system shall identify whether the administrator user is using the default configured password.
2. After the first successful authentication with the default password, the system shall require a password change.
3. The system shall restrict access to non-essential protected operations until the password is changed.
4. After the password is changed, the system shall no longer treat the account as using the default password.

## Verification Method
Test and Inspection

## More Information
- This requirement applies only to the default administrator user created during system initialization.

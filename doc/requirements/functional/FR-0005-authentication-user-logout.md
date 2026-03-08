---
status: "draft"
date:
---

# User Logout

## Statement
The system shall allow an authenticated user to terminate their active session.

## Rationale
Ensure that users can explicitly end their authenticated session and prevent further use of the issued authentication tokens.

## Acceptance Criteria
1. The system shall allow an authenticated user to request logout.
2. When a logout request is received, the system shall invalidate the user's refresh token.
3. After logout, the invalidated refresh token shall no longer be accepted by the system.
4. If an invalid or already revoked refresh token is provided, the system shall reject the logout request.
5. The system shall confirm that the logout operation was successfully completed.

## Verification Method
Test and Inspection

## More Information
- Logout is performed using the refresh token associated with the authenticated session.
- Invalidating the refresh token prevents the generation of new access tokens.
- Access tokens may remain valid until their expiration time unless additional token revocation mechanisms are implemented.

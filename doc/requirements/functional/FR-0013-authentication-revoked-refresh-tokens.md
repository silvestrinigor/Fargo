---
status: "draft"
date:
---

# Revoked Refresh Token Rejection

## Statement
The system shall reject any revoked refresh token.

## Rationale
Ensure that a refresh token cannot be used after logout, revocation, or rotation.

## Acceptance Criteria
1. The system shall verify whether the refresh token has been revoked before processing a refresh request.
2. The system shall reject revoked refresh tokens.
3. The system shall not issue a new access token from a revoked refresh token.

## Verification Method
Test and Inspection

## More Information
- Revocation may occur during logout or refresh token rotation.

---
status: "draft"
date:
---

# Refresh Token Rotation

## Statement
The system shall invalidate a refresh token after it is used and issue a new refresh token when generating a new access token.

## Rationale
Ensure that refresh tokens cannot be reused after a successful refresh operation, reducing the risk of token replay attacks and improving session security.

## Acceptance Criteria
1. When a valid refresh token is used to request a new access token, the system shall invalidate the used refresh token.
2. The system shall generate and return a new refresh token together with the new access token.
3. The previously used refresh token shall no longer be accepted by the system.
4. If an invalid, expired, or revoked refresh token is provided, the system shall reject the refresh request.
5. Each refresh token shall be usable only once.

## Verification Method
Test and Inspection

## More Information
- Refresh token rotation helps prevent replay attacks where a stolen refresh token could otherwise be reused.
- The system should securely store refresh tokens and their revocation status.
- This requirement applies to the authentication token refresh process.

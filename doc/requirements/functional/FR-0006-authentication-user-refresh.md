---
status: "draft"
date:
---

# Authentication Token Refresh

## Statement
The system shall allow an authenticated user to obtain a new access token using a valid refresh token.

## Rationale
Ensure that users can maintain authenticated sessions without repeatedly providing credentials while preserving security through short-lived access tokens.

## Acceptance Criteria
1. The system shall allow a client to request a new access token by providing a valid refresh token.
2. If the refresh token is valid and not expired, the system shall generate a new access token.
3. The system shall reject the request if the refresh token is invalid, revoked, or expired.
4. The system shall associate the newly generated access token with the user linked to the refresh token.
5. The system may generate a new refresh token when issuing a new access token.

## Verification Method
Test and Inspection

## More Information
- The refresh token is used to obtain a new access token without requiring the user to re-enter credentials.
- Refresh tokens should have a longer expiration time than access tokens.
- Refresh tokens should be securely stored and protected against unauthorized access.

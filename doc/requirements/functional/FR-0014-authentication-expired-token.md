---
status: "draft"
date:
---

# Expired Token Rejection

## Statement
The system shall reject expired authentication tokens.

## Rationale
Ensure that authentication tokens are valid only within their defined lifetime.

## Acceptance Criteria
1. The system shall reject expired access tokens for protected resource requests.
2. The system shall reject expired refresh tokens for token refresh requests.
3. The system shall not issue new tokens based on an expired refresh token.

## Verification Method
Test and Inspection

## More Information
- Access token expiration and refresh token expiration may have different durations.

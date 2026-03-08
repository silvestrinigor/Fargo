---
status: "draft"
date:
---

# Login Rate Limiting

## Statement
The system shall limit repeated login attempts within a defined period.

## Rationale
Reduce the risk of brute-force attacks against user credentials.

## Acceptance Criteria
1. The system shall track repeated failed login attempts for the same user identifier, client, or equivalent control scope.
2. The system shall temporarily restrict further login attempts when the allowed threshold is exceeded.
3. The system shall return an appropriate authentication or throttling response when login attempts are limited.
4. The system shall allow authentication attempts again after the restriction period ends.
5. The rate limiting mechanism shall apply consistently to authentication requests.

## Verification Method
Test | Analysis | Inspection

## More Information
- The allowed threshold and restriction period should be defined by security policy.
- The control scope may be based on user identifier, IP address, device, or a combination of these factors.

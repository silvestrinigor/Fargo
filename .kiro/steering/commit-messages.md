---
inclusion: manual
---

# Commit messages

Use Conventional Commits. The full guide is
`docs/contributing/commit-messages.md`; follow it when writing a commit
message. Only commit when the user asks.

- **Format:** `<type>(<scope>): <subject>`, then an optional body and footer.
- **Types:** `feat`, `fix`, `docs`, `refactor`, `perf`, `test`, `style`,
  `build`, `ci`, `chore`.
- **Scopes (optional):** `core`, `application`, `infrastructure`, `http`,
  `realtime`, `cli`, `mcp`, `migration`, `docs`, or a service name in
  lowercase without `Fargo.`. Omit when a change spans several areas.
- **Subject:** imperative, lowercase start, no trailing period, 72 characters
  or fewer. Never vague (`code cleanup`).
- **Body:** blank line after the subject, wrapped at about 72 characters,
  explains why rather than how.
- **Breaking changes:** `!` after the type or scope and a `BREAKING CHANGE:`
  footer.
- **Scope of a commit:** one logical change. Keep formatting and renames apart
  from behavior changes, and include the docs update with the behavior.

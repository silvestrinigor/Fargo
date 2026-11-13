---
inclusion: manual
---

# Branches and pull requests

The full guide is `docs/contributing/branches-and-pull-requests.md`; follow it
when creating a branch, opening a pull request, or merging.

- **Never** push to `master` or `develop`, force-push, or rewrite their
  history. Only push or open a pull request when the user asks.
- **Branches:** start from the latest `develop`, named
  `<type>/<short-description>` (`feat/partition-commands`), lowercase and
  hyphenated. Types are the commit types.
- **Pull request title:** a Conventional Commit subject
  (`docs/contributing/commit-messages.md`). It becomes the squash commit.
- **Pull request:** one logical change, filled-in template, docs updated in the
  same pull request, build and tests passing first.
- **Merge:** squash into `develop`, merge commit from `develop` into `master`,
  never a rebase merge. Delete the branch afterwards.
- **Releases:** tag the merge commit on `master` with `vX.Y.Z`.
- **Hotfix:** branch from `master`, then merge `master` back into `develop`.

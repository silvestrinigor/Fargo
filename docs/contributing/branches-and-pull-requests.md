# Branches and Pull Requests

Work on Fargo happens on short-lived branches and reaches the shared branches
through pull requests. This page covers the branches, how to name them, what a
pull request needs, and how it is merged. Commit messages follow
[commit messages](commit-messages.md).

## Long-lived branches

| Branch | Role |
|---|---|
| `master` | What is released. It is always in a releasable state. Pushing to it deploys the documentation site, and release tags go on it. |
| `develop` | Where work is integrated. Feature branches start from it and return to it. |

- Never push directly to `master` or `develop`. Change them through a pull
  request.
- Never rewrite their history: no force pushes, no rebasing, no resetting.
- `master` changes only by merging `develop` (a release) or a hotfix.

## Short-lived branches

Start from the latest `develop` and name the branch
`<type>/<short-description>`:

- The type is one of the [commit types](commit-messages.md#types), such as
  `feat`, `fix`, `docs`, or `refactor`.
- The description is lowercase and hyphenated: `feat/partition-commands`,
  `fix/outbox-retry`, `docs/branch-rules`.
- Branches that tools create, such as Dependabot's, keep their generated names.
- Keep a branch small and short. Delete it after the merge.
- Rebasing or amending a branch that only you use is fine. Do not force-push a
  branch that others have pulled.

## Pull requests

- **Scope:** one logical change, as for a commit. Split unrelated work into
  separate pull requests.
- **Title:** a Conventional Commit subject, such as
  `feat(http): add partition commands`. The title becomes the commit message
  when the pull request is squashed, so the commits inside the branch do not
  have to follow the rules.
- **Description:** use the pull request template. Say what changed and why, how
  it was tested, and any breaking change.
- **Documentation:** update the docs in the same pull request as the behavior,
  as the [writing documentation](writing-documentation.md) guide requires.
- **Drafts:** open a draft pull request early for large work, and mark it ready
  when it is.

Before you ask for a review:

1. The solution builds with `dotnet build`.
2. The tests pass with `dotnet test`.
3. The branch is up to date with its target branch.
4. The docs build without new warnings (`docfx docs/docfx.json`) when you
   changed them.

## Merging

| Target | Method | Why |
|---|---|---|
| `develop` | Squash merge | One Conventional Commit per pull request. |
| `master` | Merge commit | Each release stays visible as one merge. |

- Merge only when the checks pass and every review comment is addressed.
- Check the squash commit message before you confirm. It must still match the
  pull request title.
- Delete the branch after the merge.
- Do not use a rebase merge.

## Releases and hotfixes

- **Release:** open a pull request from `develop` into `master` and merge it
  with a merge commit. To publish the CLI, tag the merge commit with
  `vX.Y.Z` (or a prerelease such as `v0.1.0-alpha.1`) and push the tag. The
  release workflow builds the executables.
- **Hotfix:** branch from `master` as `fix/<description>`, open a pull request
  into `master`, and then merge `master` back into `develop` so the fix is not
  lost.

## Recommended repository settings

These are settings on GitHub, not part of the code:

- Protect `master` and `develop`: require a pull request, and block force pushes
  and deletion.
- Allow squash and merge-commit merges, and turn off rebase merges.
- Delete head branches automatically after a merge.

---
name: release
description: 'Prepare a new release of the GPreviatti.Template.Hexagonal.Solution NuGet template. Use whenever the user or an agent asks to release, publish, bump, or version a new template version. Updates the release notes in the root csproj, the package_version in the publish workflow, and reviews the root Readme.md.'
---

# Release (Hexagonal Solution Template)

Use this skill every time a new version of the template package must be released. It keeps three places in sync:

1. Release notes in [GPreviatti.Template.Hexagonal.Solution.csproj](../../../GPreviatti.Template.Hexagonal.Solution.csproj) (`PackageReleaseNotes`).
2. Version in [publish.yml](../../workflows/publish.yml) (`package_version` of the `pack-and-publish` job).
3. Accuracy of [Readme.md](../../../Readme.md).

The root csproj has no `PackageVersion` property; the published version comes only from `package_version` in `publish.yml`.

## Step 1: Collect the changes since the last release

1. Find the last release commit, which is the one that last changed `package_version`:
   ```powershell
   git --no-pager log --oneline -n 1 -G "package_version" -- .github/workflows/publish.yml
   ```
2. List everything merged since then:
   ```powershell
   git --no-pager log --oneline <last-release-commit>..HEAD
   ```
3. Read the current `package_version` and the current `PackageReleaseNotes` so the new notes follow the same style.

## Step 2: Decide the new version

Versions follow `MAJOR.MINOR.PATCH` (current line: `10.x.y`). Choose the bump from the changes:

| Change type | Bump |
|---|---|
| Breaking change for template users (removed/renamed template, changed parameters or generated structure) | MAJOR (confirm with the user first) |
| New template feature, new template, new option, or significant new capability | MINOR |
| Package upgrades, analyzer or bug fixes, documentation, internal fixes | MINOR (repo precedent: `10.19.0 -> 10.20.0` was a package-upgrade release) |

If the bump is not clear from the commits, ask the user with `ask_user` (options: patch, minor, major) before editing files. Never guess silently.

## Step 3: Update the release notes

Edit `PackageReleaseNotes` in [GPreviatti.Template.Hexagonal.Solution.csproj](../../../GPreviatti.Template.Hexagonal.Solution.csproj):

- Replace the previous entries with the entries for the new release (the notes describe the **current** release only, as in the existing file).
- One bullet per meaningful change, using the same 4-space indentation as the file.
- Summarize from the Step 1 commit list; leave out noise (merge commits, comment typos).
- Name affected templates (`Bff`, `Contracts`, `Full`, `Simple`, `SimpleWebUi`, `WebUi`) when relevant.
- Keep package names and versions exact.

## Step 4: Update the publish workflow version

Edit [publish.yml](../../workflows/publish.yml) and set the new version in the `pack-and-publish` job:

```yaml
    with:
      dotnet_version: "${{ vars.PROJECT_DOTNET_VERSION }}"
      package_version: "<NEW_VERSION>"
```

The value must match the version being released. Do not change any other workflow setting.

## Step 5: Review the Readme

Review [Readme.md](../../../Readme.md) against the release changes and update only what is out of date:

- **Available templates**: names and descriptions match the `templates/` folders and the release notes (add, remove, or rename templates if the release did).
- **Template documentation** links point to existing `templates/*/Readme.md` files.
- **Technologies included**: add or remove technologies changed by the release (for example a new analyzer, cache, or messaging library). Do not list transitive packages.
- **Quick start, migrations, help, update/uninstall commands**: the `dotnet new` commands and options still match the templates.
- **Version information**: if the Readme shows a version anywhere, it must show the new one. Do not add version numbers where none exist unless the release requires it.
- Keep the Readme concise. Release notes belong in the csproj, not the Readme.

## Step 6: Validate

1. Confirm the version and notes are in place:
   ```powershell
   Select-String -Path .github\workflows\publish.yml -Pattern 'package_version'
   Select-String -Path GPreviatti.Template.Hexagonal.Solution.csproj -Pattern 'PackageReleaseNotes'
   ```
2. Confirm the package still packs (validates the csproj and release notes):
   ```powershell
   dotnet pack GPreviatti.Template.Hexagonal.Solution.csproj -c Release -o $env:TEMP\release-check
   ```
   Delete `$env:TEMP\release-check` afterwards.
3. Review `git --no-pager diff` and confirm only the release files (csproj, publish.yml, Readme.md) changed.

## Step 7: Finish

- Do **not** commit, tag, or push unless the user asks. When asked to commit, use the message `chore(release): bump version to <NEW_VERSION> and update package release notes` with the Co-authored-by trailer.
- Report to the user: old and new version, release notes added, and Readme changes made.
- The publish itself is run manually through the `Publish template in Nuget.org` workflow (`workflow_dispatch`); this skill does not trigger it.

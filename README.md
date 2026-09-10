# CI/CD Concept Document — MyDesktopApp (.NET Windows Desktop)

## 1. Overall CI/CD Flow

The pipeline is split into two YAML files by purpose and speed:

- **`azure-pipelines-ci.yml`** — fast feedback. Triggers on every push and PR to `main`. Restores, builds, runs NUnit unit and integration tests, and publishes artifacts. No UI tests, no installer — these are intentionally excluded so every commit gets validated in minutes, not tens of minutes.
- **`azure-pipelines-release.yml`** — the slow, thorough path. Triggered automatically when CI succeeds on `main` (via a `resources.pipelines` reference), not on every push directly. It downloads CI's already-tested build output (rather than rebuilding), runs UI automation tests on a dedicated interactive agent, builds the installer, includes a placeholder for production code signing, and finally deploys through a gated Release stage.

This two-pipeline split means a broken commit is caught within minutes by CI, while the expensive UI-test/installer/release chain only ever runs against code that has already passed unit and integration tests — and only once per validated commit, not once per pipeline.

## 2. Key Design Decisions

- **Release consumes CI's artifacts instead of rebuilding.** The Release pipeline downloads CI's `buildOutput` and `uiTestOutput` artifacts directly rather than re-running restore/build/test. This guarantees the exact binary that shipped is the one that was tested — no risk of a second build introducing drift from a different SDK patch or NuGet resolution — and it avoids compiling and unit-testing the same commit twice.
- **UI tests run on a separate, interactive agent pool** (`SelfHostedWindowsPool-Interactive`), distinct from the standard build pool. UI automation tools need a real Windows desktop session; agents running as a service execute in Session 0, which has no desktop and cannot host or interact with application windows.
- **Reusable YAML templates** (`build-and-test.yml`, `ui-tests.yml`, `installer.yml`) keep the restore/build/test logic in one place, shared by CI directly and by Release indirectly through the artifacts it consumes.
- **CI uses `$(Build.BuildId)`** to create a unique application version. The version is stored with the CI build artifact for traceability, and the Release pipeline consumes the exact CI-tested application binaries without rebuilding them.

## 3. Branching, PR, and Release Strategy

- `main` is the only long-lived, protected branch. Feature work happens on short-lived branches and merges via PR.
- PR triggers on CI double as the validation stage — a PR into `main` runs the full restore/build/unit-and-integration-test pass before merge is allowed, so there's no need for a separate third pipeline just for validation.
- Release is fully decoupled from push events. It only fires after CI succeeds on `main`, keeping "did it build and pass tests" and "should this ship" as two distinct, sequential questions rather than one combined pipeline.
- In a real team setup this would be paired with branch policies (required PR reviewers, required CI success, no direct pushes to `main`).

## 4. Build, Testing, Packaging, Artifact Handling

Build → NUnit unit and integration tests → test result publishing (always, even on failure) → app + UI test binaries published as artifacts. Release then: downloads those artifacts → runs UI automation (FlaUI/VSTest against the real app on the interactive agent) → builds the installer via WiX from the already-tested published app output, including the executable, application/library DLLs, and required .NET runtime metadata files → publishes the MSI as the `release` artifact, with code signing represented by a placeholder production step → deploys through an approval-gated environment.

## 5. Secrets, Permissions, and Service Connections

No secrets are hardcoded in YAML. Code-signing certificate and password are referenced as pipeline variables (`$(CodeSigningCertSecureFile)`, `$(CodeSigningCertPassword)`) that, in a real org, would be backed by a Secure File in the Azure DevOps Library or an Azure Key Vault–linked variable group, injected only at run time. Any service connections (artifact feeds, external signing services, deployment targets) would similarly be scoped per-pipeline with least-privilege permissions rather than shared broadly across the project.

## 6. Software Protection, Licensing, Installer, and Update Infrastructure

- **Installer**: built via WiX from the complete CI-tested published app output, including the executable, application/library DLLs, and required .NET runtime metadata files. Packaging happens only after UI tests pass, so a build that fails automation never gets packaged.
- **Code signing**: placeholder step included (`signtool`) with a comment on secret handling; in production this protects users from SmartScreen warnings and gives tamper-evidence.
- **Licensing**: not implemented here, but would sit as a step between build and installer packaging — e.g. embedding a license validation library or key-check into the app build, with license issuance handled by a separate backend service, not by the pipeline itself.
- **Update infrastructure**: a placeholder "publish update manifest" step is included in the Release stage, representing where a real setup would publish the versioned MSI plus a manifest/appcast file to a feed consumed by an auto-update client (e.g., Squirrel or a custom updater polling a JSON manifest).

## 7. Logging, Telemetry, Monitoring

Beyond pipeline-level test result publishing, a production setup would benefit from:

- Build/release metrics fed into Azure DevOps Analytics or a dashboard (build duration trends, flaky-test detection).
- Application-side telemetry (e.g., Application Insights) reporting install success/failure and crash data post-release, closing the loop between "shipped" and "actually works for users."
- Pipeline run notifications (Teams/Slack) on Release stage failures or pending approvals, so gated releases don't sit unnoticed.

## 8. Assumptions Made

- Both agent pools (`SelfHostedWindowsPool` and `SelfHostedWindowsPool-Interactive`) are backed by the same physical machine for this assignment, with the interactive pool's agent run as a console process under an active Windows session rather than as a service, to satisfy the requirement for a real desktop session. In production these would be separate dedicated machines.
- Agent pool names, project names, and service connection names are placeholders, per the assignment's instructions.
- The test project references the NUnit3TestAdapter and NUnit packages, and references both the Core and Utilities projects, so `dotnet test` discovers and executes the unit and integration tests without a separate NUnit-specific task.
- Code signing, static analysis, and dependency scanning are implemented as placeholder steps with real commands commented in, rather than fully wired to live tooling/credentials.

## 9. Possible Improvements for Production

- Replace the hardcoded `1.0.$(Build.BuildId)` versioning with a tool like GitVersion, deriving Major.Minor from branch/tag conventions automatically.
- Make the static analysis and dependency-scan steps enforcing (fail the build) rather than informational.
- Wire real code signing through Key Vault–backed secrets and a dedicated signing service connection.
- Add a genuine licensing/entitlement step and a real auto-update manifest publish, backed by a CDN or release feed.
- Introduce a staging/canary environment ahead of Production, with its own approval gate, so releases can be validated against real hardware before general rollout.
- Add pipeline notifications and post-deployment telemetry monitoring, so a release's real-world health is visible, not just its pipeline status.

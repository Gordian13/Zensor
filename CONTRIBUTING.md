# Contributing

Thank you for contributing to this project.

To keep collaboration predictable and the `main` branch stable, all changes must follow the workflow described below.

## Project Standards

- Unity version: `6000.3.14f1 LTS`
- Default branch: `main`
- `main` is protected
- Direct commits to `main` are not allowed
- All changes must be implemented in branches
- Every branch must be created from an existing issue

## Workflow

### 1. Start with an Issue

Before starting any work, create or assign an issue.

Each issue should define:
- the goal
- the scope
- acceptance criteria if needed
- the responsible person

Work should not start without an issue.

### 2. Create a Branch from the Issue

Branches must always be created from the related issue.

Use the following naming convention:

```text
<issue-number>-<short-description>
```

Examples:

```text
42-player-movement-fix
57-main-menu-ui
63-save-system
```

Branch naming rules:
- use lowercase only
- use hyphens instead of spaces
- keep the name short and descriptive

### 3. Open a Pull Request

When the work is ready, open a Pull Request targeting `main`.

The Pull Request should:
- reference the related issue
- describe the implemented changes
- mention anything that should be tested or reviewed carefully

### 4. Review Before Merge

Every Pull Request must be reviewed before it is merged.

Rules:
- no direct merge into `main`
- no self-merge without approval
- review feedback must be addressed
- only approved Pull Requests may be merged

## Definition of Done

Before merging, make sure that:

- the issue requirements are fulfilled
- the project works with Unity `6000.3.14f1 LTS`
- the changes were tested locally
- the code follows the project conventions
- documentation is updated if necessary
- the Pull Request is linked to the issue
- the Pull Request has been reviewed and approved
- no known critical issues were introduced
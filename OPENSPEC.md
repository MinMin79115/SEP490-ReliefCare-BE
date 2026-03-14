# OpenSpec Guide for this repository (OpenCode)

This document explains how to use OpenSpec in this repo with **OpenCode**.

## 1. Setup

Install OpenSpec CLI globally:

```bash
npm install -g @fission-ai/openspec@latest
```

Initialize OpenSpec for OpenCode (already done in this repo):

```bash
openspec init --tools opencode .
```

You should have:

- `.opencode/command/opsx-propose.md`
- `.opencode/command/opsx-explore.md`
- `.opencode/command/opsx-apply.md`
- `.opencode/command/opsx-archive.md`

If slash commands do not show up, restart your editor/IDE.

---

## 2. Core OpenSpec concepts

OpenSpec organizes work into **changes**.

Each change usually contains these artifacts:

- `proposal.md` → Why and what
- `design.md` → How (technical decisions)
- `specs/**/spec.md` → Requirements and scenarios
- `tasks.md` → Implementation checklist

Folder structure:

```text
openspec/
  changes/
    <change-name>/
      proposal.md
      design.md
      specs/
      tasks.md
```

---

## 3. Standard workflow (CLI)

### Step 1 — Create a change

```bash
openspec new change "my-change-name"
```

Use kebab-case for change names.

### Step 2 — Check current status

```bash
openspec status --change "my-change-name" --json
```

This tells you which artifacts are ready/blocked/done.

### Step 3 — Get writing instructions per artifact

```bash
openspec instructions proposal --change "my-change-name" --json
openspec instructions design --change "my-change-name" --json
openspec instructions specs --change "my-change-name" --json
openspec instructions tasks --change "my-change-name" --json
```

Follow template and rules exactly.

### Step 4 — Validate

```bash
openspec validate "my-change-name" --type change --strict
```

### Step 5 — Implement code according to tasks

Once artifacts are complete and validated, implement code following `tasks.md`.

### Step 6 — Archive when complete

```bash
openspec archive "my-change-name"
```

---

## 4. OpenCode slash command workflow

You can also use OpenCode commands directly in chat:

- `/opsx:propose "your idea"` → scaffold change and artifacts
- `/opsx:explore <change-name>` → analyze/clarify before coding
- `/opsx:apply <change-name>` → execute tasks
- `/opsx:archive <change-name>` → archive completed change

Recommended flow:

1. `/opsx:propose`
2. Review/edit artifacts in `openspec/changes/<name>/`
3. Validate via CLI
4. `/opsx:apply` or manual implementation
5. Archive

---

## 5. Writing good spec files

In `spec.md`:

- Use normative wording: **SHALL/MUST**
- Every requirement must include at least one scenario
- Scenario heading must be exactly `#### Scenario: ...`
- Prefer behavior-level requirements over implementation details

Example:

```md
## ADDED Requirements

### Requirement: API SHALL return 201 on create
The system SHALL return `201 Created` for successful resource creation.

#### Scenario: Create succeeds
- **WHEN** client creates a resource
- **THEN** API returns `201 Created`
```

---

## 6. Current OpenSpec change in this repo

This repository currently includes:

- `openspec/changes/document-backend-api-spec/`

Purpose:

- Define a baseline API contract for backend modules.

---

## 7. Troubleshooting

### `unknown option '--change'` on validate

Use:

```bash
openspec validate "<change-name>" --type change --strict
```

not `--change`.

### Slash commands not available

- Ensure `.opencode/` exists
- Restart editor/IDE

### Validation fails on specs

- Check scenario heading uses `#### Scenario:`
- Ensure each requirement has at least one scenario
- Re-run strict validation

---

## 8. Useful commands cheat sheet

```bash
openspec --help
openspec list
openspec new change "my-change"
openspec status --change "my-change" --json
openspec instructions proposal --change "my-change" --json
openspec validate "my-change" --type change --strict
openspec archive "my-change"
```

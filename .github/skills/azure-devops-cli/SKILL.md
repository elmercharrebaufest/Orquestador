---
name: azure-devops-cli
description: Manage Azure DevOps resources via CLI including projects, repos, pipelines, builds, pull requests, work items, artifacts, and service endpoints. Use when working with Azure DevOps, az commands, devops automation, CI/CD, or when user mentions Azure DevOps CLI.
---

# Azure DevOps CLI

Manage Azure DevOps resources using the Azure CLI with the Azure DevOps extension.

**CLI Version:** 2.81.0 (current as of 2025)

## Environment Verification (REQUIRED before any task)

Before running any command, verify the environment is ready. If any check fails, **abort the task immediately** and tell the user exactly what to fix.

```bash
# 1. Verify az CLI is installed
az --version
# If this fails: inform the user they must install Azure CLI and abort.

# 2. Verify the azure-devops extension is installed
az extension show --name azure-devops --query name -o tsv
# If this fails: inform the user they must run 'az extension add --name azure-devops' and abort.

# 3. Verify the user is authenticated
az account show --query user.name -o tsv
# If this fails or returns an error: inform the user they must run 'az login' and abort.

# 4. Verify default organization and project are configured
az devops configure --list
# If organization or project are not set: inform the user they must run
# 'az devops configure --defaults organization=https://dev.azure.com/{org} project={project}' and abort.
```

> **IMPORTANT**: If any of these checks fail, stop the task entirely and tell the user the exact step they must complete before continuing. Do not attempt to install, authenticate, or configure on behalf of the user.

## CLI Structure

```
az devops          # Main DevOps commands
├── admin          # Administration (banner)
├── extension      # Extension management
├── project        # Team projects
├── security       # Security operations
│   ├── group      # Security groups
│   └── permission # Security permissions
├── service-endpoint # Service connections
├── team           # Teams
├── user           # Users
├── wiki           # Wikis
├── configure      # Set defaults
├── invoke         # Invoke REST API
├── login          # Authenticate
└── logout         # Clear credentials

az pipelines       # Azure Pipelines
├── agent          # Agents
├── build          # Builds
├── folder         # Pipeline folders
├── pool           # Agent pools
├── queue          # Agent queues
├── release        # Releases
├── runs           # Pipeline runs
├── variable       # Pipeline variables
└── variable-group # Variable groups

az repos           # Azure Repos
├── import         # Git imports
├── policy         # Branch policies
├── pr             # Pull requests
└── ref            # Git references

az artifacts       # Azure Artifacts
└── universal      # Universal Packages
```

## Reference Files

Read the relevant reference file based on the user's task. Each file contains complete command syntax and examples for its domain.

| File | When to read | Covers |
|---|---|---|
| `references/pipelines-and-builds.md` | Pipelines, builds, releases, artifacts | Pipelines CRUD, runs, builds, releases, artifacts download/upload |
| `references/variables-and-agents.md` | Pipeline variables, agent pools | Pipeline variables, Variable groups, Pipeline folders, Agent pools/queues |
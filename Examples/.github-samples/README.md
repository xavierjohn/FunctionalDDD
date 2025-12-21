# GitHub Copilot Agent Sample Specifications

This folder contains example project specifications for the FunctionalDDD Clean Architecture Agent.

## Files

- **project-spec-demo.yml** - Full e-commerce example with complete domain model, commands, queries, and API endpoints
- **project-spec-minimal.yml** - Minimal starting point for a simple project

## Usage

These YAML files can be used as:
1. **Templates** - Copy and modify for your own project
2. **Learning examples** - See how to structure a complete specification
3. **Testing** - Validate the agent's scaffolding capabilities

## How to Use

### Option 1: Copy as Template
```powershell
Copy-Item "Examples/.github-samples/project-spec-minimal.yml" ".github/my-project-spec.yml"
# Edit my-project-spec.yml with your project details
```

### Option 2: Reference in GitHub Issue
When creating a GitHub Issue with label `copilot-scaffold`, you can:
- Copy the YAML content into the issue body
- Reference the patterns shown in these examples
- Use natural language based on these examples

## Related Documentation

- [Copilot Instructions](../../.github/copilot-instructions.md) - Full agent capabilities
- [Agent README](../../.github/AGENT_README.md) - Overview and getting started
- [Feature Template](../../.github/feature-template.md) - For adding features iteratively

## Example Issue

See the agent documentation for examples of how to create issues that trigger scaffolding using these specifications.

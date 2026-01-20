# Copilot instructions

This repository is set up to use Aspire. Aspire is an orchestrator for the entire application and will take care of configuring dependencies, building, and running the application. The resources that make up the application are defined in `src\server\FakeSurveyGenerator.AppHost\AppHost.cs` including application code and external dependencies.

## General recommendations for working with Aspire
1. Before making any changes always run the apphost using `aspire run` and inspect the state of resources to make sure you are building from a known state.
1. Changes to the _AppHost.cs_ file will require a restart of the application to take effect.
2. Make changes incrementally and run the aspire application using the `aspire run` command to validate changes.
3. Use the Aspire MCP tools to check the status of resources and debug issues.

## Running the application
To run the application run the following command:

```
aspire run
```

If there is already an instance of the application running it will prompt to stop the existing instance. You only need to restart the application if code in `AppHost.cs` is changed, but if you experience problems it can be useful to reset everything to the starting state.

## Checking resources
To check the status of resources defined in the app model use the _list resources_ tool. This will show you the current state of each resource and if there are any issues. If a resource is not running as expected you can use the _execute resource command_ tool to restart it or perform other actions.

## Listing integrations
IMPORTANT! When a user asks you to add a resource to the app model you should first use the _list integrations_ tool to get a list of the current versions of all the available integrations. You should try to use the version of the integration which aligns with the version of the Aspire.AppHost.Sdk. Some integration versions may have a preview suffix. Once you have identified the correct integration you should always use the _get integration docs_ tool to fetch the latest documentation for the integration and follow the links to get additional guidance.

## Debugging issues
IMPORTANT! Aspire is designed to capture rich logs and telemetry for all resources defined in the app model. Use the following diagnostic tools when debugging issues with the application before making changes to make sure you are focusing on the right things.

1. _list structured logs_; use this tool to get details about structured logs.
2. _list console logs_; use this tool to get details about console logs.
3. _list traces_; use this tool to get details about traces.
4. _list trace structured logs_; use this tool to get logs related to a trace

## Other Aspire MCP tools

1. _select apphost_; use this tool if working with multiple app hosts within a workspace.
2. _list apphosts_; use this tool to get details about active app hosts.

## Playwright MCP server

The playwright MCP server has also been configured in this repository and you should use it to perform functional investigations of the resources defined in the app model as you work on the codebase. To get endpoints that can be used for navigation using the playwright MCP server use the list resources tool.

## Updating the app host
The user may request that you update the Aspire apphost. You can do this using the `aspire update` command. This will update the apphost to the latest version and some of the Aspire specific packages in referenced projects, however you may need to manually update other packages in the solution to ensure compatibility. You can consider using the `dotnet-outdated` with the users consent. To install the `dotnet-outdated` tool use the following command:

```
dotnet tool install --global dotnet-outdated-tool
```

## Persistent containers
IMPORTANT! Consider avoiding persistent containers early during development to avoid creating state management issues when restarting the app.

## Aspire workload
IMPORTANT! The aspire workload is obsolete. You should never attempt to install or use the Aspire workload.

## Official documentation
IMPORTANT! Always prefer official documentation when available. The following sites contain the official documentation for Aspire and related components

1. https://aspire.dev
2. https://learn.microsoft.com/dotnet/aspire
3. https://nuget.org (for specific integration package details)

## Agent Skills for Feature Implementation

This repository includes a comprehensive suite of GitHub Copilot Agent Skills to enable end-to-end feature implementation and validation. These skills automate the development workflow from code changes through interactive E2E testing.

### Overview of Available Skills

The skills are organized in `.github/skills/` and follow the [GitHub Agent Skills specification](https://docs.github.com/en/copilot/concepts/agents/about-agent-skills).

#### Orchestrator Skill
- **`implement-feature`** - High-level coordinator that guides the complete feature implementation workflow (analysis → implementation → build → test → runtime → E2E validation)

#### Standalone Skills
- **`build-backend`** - Compiles .NET backend projects and reports compilation errors
- **`build-frontend`** - Compiles TypeScript/React frontend with Vite and reports build errors
- **`test-backend-units`** - Executes unit and integration tests, reports failures and coverage
- **`validate-aspire-runtime`** - Starts Aspire orchestration and validates all resources reach healthy status
- **`validate-e2e`** - Executes feature-specific E2E validation using Playwright MCP to navigate and verify UI behavior

#### Foundation Skill
- **`fake-survey-gen-foundation`** - Reference documentation for project architecture, service topology, and conventions (loaded by all other skills)

### Quick Start: Using Skills to Implement a Feature

#### Guided Workflow (Recommended)
Use the orchestrator skill which sequences all steps with checkpoints:
```
Agent: "Use the implement-feature skill to add a new feature to the Fake Survey Generator"
```

The agent will guide you through:
1. **Analysis** - Understand requirements and identify affected components
2. **Implementation** - Generate code changes for backend/frontend
3. **Build** - Compile backend and frontend, halt on errors
4. **Test** - Run all tests, halt on failures
5. **Runtime** - Start Aspire, wait for all resources to be healthy
6. **E2E Validation** - Run feature-specific UI validation using Playwright
7. **Report** - Document results (success or specific failure point)

#### Standalone Skill Usage
Run individual skills as needed:
```bash
# Build backend only
Agent: "Use the build-backend skill to compile the backend"

# Run tests only
Agent: "Use the test-backend-units skill to validate tests pass"

# Start application
Agent: "Use the validate-aspire-runtime skill to start the running application"

# Validate feature in UI
Agent: "Use the validate-e2e skill to verify the new feature works correctly"
```

### Understanding Each Skill

Each skill is self-contained with detailed instructions in its `SKILL.md` file:

- **Purpose** - When and why to use the skill
- **What It Does** - Step-by-step breakdown of actions
- **How to Use** - Instructions for the agent to invoke the skill
- **Success/Failure Criteria** - Expected outcomes and how to handle failures
- **Next Steps** - What skill to invoke after success

### Key Features of the Skills

#### 1. Checkpoint-Based Validation
Each skill validates a specific layer and halts on failure:
```
Build → Test → Runtime → E2E
```
If backend build fails, stop before running tests. This focuses debugging on the right layer.

#### 2. Feature-Aware E2E Validation
The E2E skill is not a generic test runner—it adapts to the specific feature:
- **Feature**: "Change button to purple" → Validates button color
- **Feature**: "Add user registration" → Validates registration flow
- **Feature**: "Create surveys with questions" → Validates complete survey creation and listing

#### 3. Simple Error Reporting
Skills output to stdout/stderr with clear error messages:
- Compilation errors include file:line references
- Test failures include test names and assertions
- Runtime failures include specific resource health status

#### 4. Aspire Integration
Skills leverage Aspire MCP tools to:
- Orchestrate application startup
- Monitor resource health
- Discover endpoints for E2E validation

### Example: Implementing a Feature End-to-End

**Scenario**: "Add the ability to filter surveys by status (Draft, Published, Archived)"

**Using the Orchestrator Skill**:

```
User: "Implement a feature to filter surveys by status"

Agent Flow:
1. ANALYSIS: Understand feature
   - Frontend: Add filter dropdown to survey list
   - Backend: Add status filter parameter to API
   - Tests: Unit test filter logic, integration test endpoint
   - Happy-Path: List surveys → select status filter → verify results

2. IMPLEMENTATION: Generate code
   - Modify FakeSurveyGenerator.Application service method
   - Update FakeSurveyGenerator.Api endpoint
   - Create React filter component
   - Write tests

3. BUILD: Invoke build-backend skill
   → dotnet build (all .NET projects)
   → Report: ✅ Build successful

4. BUILD: Invoke build-frontend skill
   → npm run build (TypeScript + Vite)
   → Report: ✅ Build successful

5. TEST: Invoke test-backend-units skill
   → dotnet test (all test projects)
   → Report: ✅ 42 tests passed, coverage: 87%

6. RUNTIME: Invoke validate-aspire-runtime skill
   → aspire run (start all services)
   → Poll resources until healthy
   → Report: ✅ All resources healthy, UI: https://localhost:3000

7. E2E: Invoke validate-e2e skill
   → Navigate to survey list
   → Click filter dropdown
   → Select "Draft" status
   → Verify only draft surveys shown
   → Select "Published" status
   → Verify only published surveys shown
   → Report: ✅ Feature validation passed

Result: Feature implementation complete and validated end-to-end ✅
```

### Troubleshooting

#### Skill Usage Issues

**"The skill isn't working"**
- Verify you're in the repository root directory
- Check that dependencies are installed (`npm install` in frontend directory)
- Ensure Docker is running (needed for Testcontainers in integration tests)

**"Build passes but tests fail"**
- Review test failure details (test name, assertion, stack trace)
- Fix the failing test or feature logic
- Re-run test skill
- Do not proceed to Aspire/E2E until tests pass

**"Aspire startup fails"**
- Check which resource failed (SQL, Redis, API, etc.)
- Review Aspire dashboard logs at http://localhost:19888
- Common issues: Port already in use, container can't start
- See "Running the application" section in main AGENTS.md for troubleshooting

**"E2E validation fails"**
- Verify the feature implementation matches requirements
- Check browser console for errors (use Playwright tools)
- Ensure Aspire resources are still healthy
- Review specific E2E validation steps that failed

### Advanced: Customizing E2E Validation Flows

The E2E skill is flexible and can validate any happy-path workflow:

1. **Provide Feature Context**: When invoking E2E skill, include:
   - What the feature does
   - What happy-path the user would follow
   - What state changes to verify

2. **E2E Skill Adapts**: The agent tailors Playwright interactions to:
   - Navigate to correct pages
   - Interact with new UI elements
   - Verify expected state changes
   - Validate API calls (via network inspection)

3. **Examples**:
   - Simple: "Verify the new button is purple" → Click button, inspect color
   - Complex: "Verify users can create, edit, and delete surveys" → Create → Edit → Delete workflow

### Notes

- **Initial Setup**: First time running Aspire may take longer due to container initialization
- **Dapr**: API includes Dapr sidecar for distributed capabilities; auto-managed by Aspire
- **Database**: Uses persistent volume; survives application restarts
- **Cleanup**: After E2E validation, stop Aspire with `Ctrl+C`; restart for next validation run
- **Browser Certificates**: UI uses developer HTTPS certificates; automatically trusted

### References

- Skills location: `.github/skills/`
- GitHub Agent Skills spec: https://docs.github.com/en/copilot/concepts/agents/about-agent-skills
- Project foundation documentation: See `fake-survey-gen-foundation` skill
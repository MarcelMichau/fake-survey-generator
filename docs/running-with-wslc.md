# Run locally with WSLc

On Windows, Aspire can use WSL Containers (WSLc) to run SQL Server, Redis, and Redis Insight. The API, worker, Vite UI, and Dapr sidecar run on Windows. Run the commands in this guide from PowerShell at the repository root.

## Check prerequisites

Use Aspire CLI 17 or later, WSLc 3.0.4.0 or later, the .NET SDK version in [`global.json`](../global.json), and Node.js and npm. Install the Windows Dapr CLI and make sure `dapr.exe` is on your `PATH`. See [Aspire machine requirements](https://github.com/microsoft/aspire/blob/main/docs/machine-requirements.md), [WSL container setup](https://learn.microsoft.com/en-us/windows/wsl/tutorials/wsl-containers), and [Dapr CLI installation](https://docs.dapr.io/getting-started/install-dapr-cli/).

```powershell
aspire --version
wslc --version
dapr --version
```

If `wslc` is missing or too old, update WSL with `wsl --update` and check the version again.

## Initialize Dapr

Use slim initialization to install the native sidecar runtime. Aspire starts the sidecar when it launches the AppHost. The example uses the Dapr runtime version pinned in [CI](../.azdo/pipelines/azure-dev.yml). If you already have a runtime installed, set `$daprVersion` to the **runtime** version reported by `dapr --version` when selecting container images. See [Dapr slim initialization](https://docs.dapr.io/operations/hosting/self-hosted/self-hosted-no-docker).

```powershell
$daprVersion = "1.19.0"
dapr init --slim --runtime-version $daprVersion
dapr --version
```

### Run with the local secrets component

The default [AppHost configuration](../src/server/FakeSurveyGenerator.AppHost/AppHost.cs) sets both addresses to empty strings so Dapr skips Placement and Scheduler. The sidecar still loads the project's [local secrets component](../dapr/components/local-file.yml). With this setup, continue to [Select the runtime and start Aspire](#select-the-runtime-and-start-aspire).

### Run Placement and Scheduler in WSLc

Use this setup if you want the Dapr services available locally. The Dapr CLI's `--container-runtime` switch supports Docker and Podman. Start the Dapr containers directly with `wslc` for this setup. The example publishes both services on Windows loopback, and Scheduler advertises its published Windows address so the native sidecar can reconnect. Scheduler data is disposable in this example. See the [Dapr init reference](https://docs.dapr.io/reference/cli/dapr-init/) and the [Scheduler reference](https://docs.dapr.io/concepts/dapr-services/scheduler/).

```powershell
$image = "docker.io/daprio/dapr:$daprVersion"

wslc run --rm -d --name dapr-placement-wslc `
  -p 127.0.0.1:16050:50005 --entrypoint ./placement $image

wslc run --rm -d --name dapr-scheduler-wslc `
  -p 127.0.0.1:16060:50006 --entrypoint ./scheduler $image `
  --etcd-data-dir=/var/tmp/dapr/scheduler `
  --override-broadcast-host-port=127.0.0.1:16060
```

Point the existing `DaprSidecarOptions` in [AppHost.cs](../src/server/FakeSurveyGenerator.AppHost/AppHost.cs) at the published ports.

```csharp
options.WithOptions(new DaprSidecarOptions
{
    ResourcesPaths = ["../../../dapr/components"],
    PlacementHostAddress = "127.0.0.1:16050",
    SchedulerHostAddress = "127.0.0.1:16060"
});
```

## Select the runtime and start Aspire

Aspire prefers a running Docker or Podman runtime when either is available. It should detect WSLc when WSLc is the only available runtime. To check automatic detection after uninstalling Docker Desktop, clear the runtime override in the current PowerShell session and run `aspire doctor`. See the [Aspire runtime selection discussion](https://github.com/microsoft/aspire/issues/17865).

```powershell
Remove-Item Env:ASPIRE_CONTAINER_RUNTIME -ErrorAction SilentlyContinue
aspire doctor
```

To explicitly select WSLc for the current session, set the override before starting Aspire.

```powershell
$env:ASPIRE_CONTAINER_RUNTIME = "wslc"
```

Start the AppHost. Check `wslc ps` after the UI is healthy to confirm that WSLc runs the SQL Server, Redis, and Redis Insight containers.

```powershell
$appHost = "src/server/FakeSurveyGenerator.AppHost/FakeSurveyGenerator.AppHost.csproj"
aspire start --apphost $appHost
aspire wait ui --apphost $appHost
wslc ps
```

To verify Dapr separately, request `/v1.0/healthz` on the `api-dapr-cli` resource's HTTP endpoint in the Aspire dashboard. A healthy sidecar returns HTTP 204. If you started Placement and Scheduler, the sidecar logs also show connections to `127.0.0.1:16050` and `127.0.0.1:16060`.

## Stop the local environment

```powershell
aspire stop --apphost $appHost
```

If you started the Dapr containers, stop them too. WSLc removes them because they were started with `--rm`.

```powershell
wslc stop dapr-scheduler-wslc dapr-placement-wslc
```

## What was verified

- After Docker Desktop was uninstalled, Aspire selected WSLc automatically with `ASPIRE_CONTAINER_RUNTIME` unset. SQL Server, Redis, and Redis Insight ran in WSLc. The API and UI became healthy, the worker started, and the Dapr sidecar returned HTTP 204 with the default AppHost settings.
- In a temporary AppHost, disabling Placement and Scheduler gave the sidecar an HTTP 204 health response. With that configuration and fresh SQL storage, all 10 acceptance tests passed. The persistent SQL volume caused SQL Server to exit during the acceptance run with `Another instance of the application is already running`.
- In a separate temporary AppHost, the native sidecar connected to Placement and Scheduler in WSLc on the loopback ports above, loaded the local secrets component, and returned HTTP 204. This check used Dapr runtime 1.17.6.

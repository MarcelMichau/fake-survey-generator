# Run on a remote machine over Tailscale

Run the app with `aspire run` on another machine, such as a VM, and open the Aspire dashboard, UI, API and Redis Insight from any device on your tailnet. A [Tailscale Service](https://tailscale.com/kb/1552/tailscale-services) gives the app its own hostname, such as `fsg.<tailnet>.ts.net`, with HTTPS certificates that browsers trust. Only devices on your tailnet can reach it. Nothing in the app listens on a public interface.

The steps below use the service name `fsg`. To use another name, pass `--service <name>` to the tool and use that name in the admin console.

## Check prerequisites

On the remote machine, you need the prerequisites from the [README](../README.md#how-do-i-run-this-thing), plus:

- [Tailscale](https://tailscale.com/download), signed in with a [tag-based identity](https://tailscale.com/kb/1068/tags). Tailscale Services can't be hosted on a device that's signed in as a user.
- [MagicDNS](https://tailscale.com/kb/1081/magicdns) and [HTTPS certificates](https://tailscale.com/kb/1153/enabling-https) enabled for the tailnet.
- Permission to run `tailscale serve`. On Linux, make your user the operator once with `sudo tailscale set --operator=$USER`.

## Define the Tailscale Service

Do this once per tailnet, in the [Services page](https://login.tailscale.com/admin/services) of the admin console:

1. Select **Advertise**, then **Define a Service**.
2. Name the Service `fsg`.
3. Set the endpoints to `tcp:17081,tcp:3000,tcp:7223,tcp:5540`.

Then let your devices connect to the Service. A rule that targets the host machine's tag doesn't cover Services hosted on it. Either give the Service a tag that your [tailnet policy](https://login.tailscale.com/admin/acls) already grants access to, or add a grant for the Service:

```json
"grants": [
  {
    "src": ["autogroup:member"],
    "dst": ["svc:fsg"],
    "ip": ["tcp:17081", "tcp:3000", "tcp:7223", "tcp:5540"]
  }
]
```

## Publish the Service

From the repository root on the remote machine, run the tool. It works the same on Linux, macOS and Windows.

```sh
dotnet tools/tailnet/tailnet.cs up
```

The tool tells Tailscale to forward each port on the Service hostname to the same port on the machine.

| Resource | Tailnet URL | Local port |
| --- | --- | --- |
| Aspire dashboard | `https://fsg.<tailnet>.ts.net:17081` | 17081 |
| UI | `https://fsg.<tailnet>.ts.net:3000` | 3000 |
| API | `https://fsg.<tailnet>.ts.net:7223` | 7223 |
| Redis Insight | `https://fsg.<tailnet>.ts.net:5540` | 5540 |

The first time, approve the machine as a host for `fsg` in the admin console. To approve hosts automatically, add an [auto-approver](https://tailscale.com/kb/1337/policy-syntax#autoapprovers) to your tailnet policy:

```json
"autoApprovers": {
  "services": {
    "svc:fsg": ["tag:<your-machine-tag>"]
  }
}
```

The Service configuration persists across restarts. You only need to run `up` again after you run `down`.

## Show tailnet links in the dashboard

The dashboard lists `localhost` links for each resource. To add tailnet links next to them, set this AppHost secret once on the remote machine:

```sh
aspire secret set Tailnet:Host fsg.<tailnet>.ts.net
```

With the secret set, the AppHost also gives Redis Insight the fixed port 5540 instead of a random one. Without it, the AppHost behaves exactly as it does by default.

## Sign in to Auth0 from the tailnet

The UI uses its own address as the Auth0 redirect address. Add `https://fsg.<tailnet>.ts.net:3000` to the Auth0 application's **Allowed Callback URLs**, **Allowed Logout URLs** and **Allowed Web Origins**.

## Start Aspire

```sh
aspire run
```

Open the dashboard login link that `aspire run` prints, with `localhost:17081` replaced by `fsg.<tailnet>.ts.net:17081`.

## Stop publishing the Service

```sh
dotnet tools/tailnet/tailnet.cs down
```

To see the current configuration, run `dotnet tools/tailnet/tailnet.cs status`.

## Troubleshooting

- **Requests fail with a certificate or connection error.** Make sure the app is running. Tailscale forwards to the local port, so the URLs only work while Aspire is running.
- **`fsg.<tailnet>.ts.net` doesn't resolve.** The Service isn't defined in the admin console yet. Define it as described above.
- **The URLs work on the remote machine but time out from other devices.** The tailnet policy doesn't grant access to the Service. Add a tag or a grant as described in [Define the Tailscale Service](#define-the-tailscale-service).
- **The hostname resolves, but connections time out, and the Service shows no pending host.** The machine probably started advertising before the Service was defined. Advertise it again with `tailscale serve drain svc:fsg`, then `tailscale serve advertise svc:fsg`.
- **The UI shows `Blocked request. This host is not allowed.`** The hostname doesn't end in `.ts.net`. Add it to `server.allowedHosts` in [`vite.config.ts`](../src/client/ui/vite.config.ts).
- **The tool can't find `tailscale`.** On macOS, install the CLI from the Tailscale app's settings, or set `TAILSCALE_CLI` to the path of the `tailscale` executable.

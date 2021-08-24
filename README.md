<div align="center">
  <img width="200" src="images/undraw_customer_survey_f9ur.svg">
</div>

<div align="center">
  <small align="center">Logo by <a href="https://undraw.co">Undraw</a></small>
</div>

<h1 align="center">
  Fake Survey Generator
</h1>

<p align="center">
This is an app. That generates surveys. Fake ones. For fun. That is all.
</p>

| Component(s)              | Build Status                                                                                                                                                                                                                                     |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Fake Survey Generator API | [![Build Status](https://dev.azure.com/marcelmichau/Personal/_apis/build/status/fake-survey-generator/fake-survey-generator-api?branchName=main)](https://dev.azure.com/marcelmichau/Personal/_build/latest?definitionId=16&branchName=main) |
| Fake Survey Generator UI  | [![Build Status](https://dev.azure.com/marcelmichau/Personal/_apis/build/status/fake-survey-generator/fake-survey-generator-ui?branchName=main)](https://dev.azure.com/marcelmichau/Personal/_build/latest?definitionId=15&branchName=main)  |
| Azure Infrastructure      | [![Build Status](https://dev.azure.com/marcelmichau/Personal/_apis/build/status/fake-survey-generator/azure-infrastructure?branchName=main)](https://dev.azure.com/marcelmichau/Personal/_build/latest?definitionId=17&branchName=main)      |

[![Open in Visual Studio Code](https://open.vscode.dev/badges/open-in-vscode.svg)](https://open.vscode.dev/MarcelMichau/fake-survey-generator)

## Screenshot

![Screenshot](images/screenshot.png "Screenshot of Fake Survey Generator UI")

## What is this?

This is a .NET | C# | React | TypeScript full-stack application of moderate complexity (not just a to-do app), used as a playground for experimentation. Simply put: This is where I mess around with code. It is heavily inspired by the [.NET Microservices: Architecture for Containerized .NET Applications](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/) book, as well as its companion reference application [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers). It also incorporates various elements from different repos & blog posts which served as inspiration.

It is built using Clean Architecture principles with CQRS (Command Query Responsibility Segregation) and DDD (Domain-Driven Design) thrown into the mix. It doesn't follow these principles to the letter, but provides a decent example of how to apply the basics of these principles.

It is heavily centered around the Microsoft [.NET](https://dotnet.microsoft.com/) + [Azure](https://azure.microsoft.com/) technology stacks as these are what I have the most experience in & just like building things with. 😀

Here are some of the features incorporated into this project:

### Application Features

- Unit & Integration tests for a CQRS/DDD project with [XUnit](https://xunit.net/)
- Running Unit & Integration tests with [Docker Compose](https://docs.docker.com/compose/)
- BDD-style acceptance/E2E tests using [SpecFlow](https://specflow.org/) & [Playwright for .NET](https://playwright.dev/dotnet/)
- Using an in-memory database for testing
- Implementing health checks for an ASP.NET Core Web API using [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
- Adding [Swagger](https://swagger.io/) to an ASP.NET Core Web API using [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- Adding [OpenID Connect](https://openid.net/connect/) for Authentication
- Adding OpenID Connect to Swagger UI
- Securing an ASP.NET Core Web API using JWT Bearer authentication
- Adding security headers to API responses using [NetEscapades.AspNetCore.SecurityHeaders](https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders)
- Using [AutoWrapper](https://github.com/proudmonkey/AutoWrapper) to return consistent API response models
- Using [Hosted Services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services) in ASP.NET Core Web API
- Using a distributed [Redis](https://redis.io/) cache
- Configuring SQL Server retry policies
- Using [Polly](https://github.com/App-vNext/Polly) to make resilient HTTP requests
- Implementing [Forwarded Headers](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer) for hosting ASP.NET Core Web API behind a load balancer
- Validation of commands using [FluentValidation](https://fluentvalidation.net/)
- Configuring [Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview) for telemetry

### Infrastructure Features

- Automatic semantic versioning using [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)
- Creating trusted SSL certificates for HTTPS in development
- Deploying Entity Framework Core Code-First Migrations to [Azure SQL Database](https://azure.microsoft.com/en-us/services/sql-database/) using [Azure Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/get-started/what-is-azure-pipelines?view=azure-devops)
- Using [Azure Active Directory](https://azure.microsoft.com/en-us/services/active-directory/) authentication to [Azure SQL Database](https://azure.microsoft.com/en-us/services/sql-database/) with Entity Framework Core
- Running a microservice application locally using [Docker Compose](https://docs.docker.com/compose/)
- Deploying a microservice application to [Kubernetes](https://kubernetes.io/) using [Helm](https://helm.sh/) charts
- Local [Kubernetes](https://kubernetes.io/) development using [Skaffold](https://skaffold.dev/)
- Using Azure Pipelines to build & deploy a microservice application to [Azure Kubernetes Service](https://azure.microsoft.com/en-us/services/kubernetes-service/)
- Using [AAD Pod Identity](https://github.com/Azure/aad-pod-identity) in AKS to associate an Azure Active Directory identity to pods
- Integrating [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/) with AKS to protect sensitive application configuration using [Azure Key Vault Provider for Secrets Store CSI Driver](https://github.com/Azure/secrets-store-csi-driver-provider-azure)
- Configuring Kubernetes [ExternalDNS](https://github.com/kubernetes-sigs/external-dns) with [Azure DNS](https://docs.microsoft.com/en-us/azure/dns/dns-overview) for custom domain names in AKS
- Configuring [Cert Manager](https://cert-manager.io/) with AKS & Azure DNS for automatic TLS certificates using [Let's Encrypt](https://letsencrypt.org/)
- Infrastructure as Code for Azure resources using [Azure Resource Manager](https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/overview) (ARM) templates

Some of the above features are relatively straightforward to implement, others have some intricacies that require some Googling in order to set up. I just like to have them placed in the context of a complete working application to refer back to when necessary.

## Why is this here?

I wanted something to try new things out on, without the risk of substantially endangering an actual production environment used by actual people.

It was for this reason that I built the Fake Survey Generator (FSG) app as a way to test out tools, libraries, patterns, frameworks & various other stuff.

It has a very simple domain: it generates surveys. Fake ones. They can be used as a tool for helping you decide what to have for dinner, which book you should read next, where you should go for your next team lunch, or anything that tickles your fancy.

This application is also used as a reference for configuring/wiring up some common things I sometimes forget how to do. _Living Documentation_ if you will. You know the culprits: How do I wire up that database again? What is the syntax for that logging configuration? How do I make thing A talk to thing B?

The domain is kept relatively simple such that it doesn't overwhelm the app with unnecessary complexity. It should be quite easy to wrap your head around without requiring a degree in Computer Science.

I also felt that a lot of reference/demo/boilerplate projects out there cover the core application domain & don't go into much detail around the building/deployment/hosting of final application. So this project aims to cover both. It contains application code, configuration, CI/CD pipelines, infrastructure-as-code needed to run the application, as well as a live, running version of the application (as long as budget allows 😁). So this repo hopefully may contain something for everyone & fill in the potential gaps across the whole spectrum of application development. It falls somewhere between a template/boilerplate project & a real-world production open-source application.

## How is this thing structured?

FSG consists of two parts:

### Server

The server side consists of the following main components:

- Fake Survey Generator API
  - Domain Project
  - Domain Unit Tests Project
  - Infrastructure Project
  - Application Project
  - Application Tests Project
  - API Integration Tests Project
  - EF Design Project (used purely for EF Core design-time tooling)
  - Data Project (helpers for test data used across test projects)
- Fake Survey Generator Worker

The server side makes use of the following tools, libraries & frameworks:

- Fake Survey Generator API
  - .NET 6.0
  - [ASP.NET Core](https://docs.microsoft.com/en-gb/aspnet/core) 6.0 Web API
  - [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) 6.0 with [Code-First Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)
  - [Dapper](https://github.com/StackExchange/Dapper)
  - [Redis](https://redis.io/)
  - [Swagger](https://swagger.io/)
  - [AutoMapper](https://automapper.org/)
  - [MediatR](https://github.com/jbogard/MediatR)
  - [FluentValidation](https://fluentvalidation.net/)
  - [Polly](https://github.com/App-vNext/Polly)
  - [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
  - [Docker](https://www.docker.com/)
  - [Serilog](https://serilog.net/)
  - [AutoWrapper](https://github.com/proudmonkey/AutoWrapper)
  - [NSubstitute](https://nsubstitute.github.io/)
- Fake Survey Generator Worker
  - .NET 6.0
  - ASP.NET Core 6.0 Worker Service

### Client

The client side consists of the following main components:

- UI

The client side makes use of the following tools, libraries & frameworks:

- [React](https://reactjs.org/)
- [TypeScript](https://www.typescriptlang.org/)
- [auth0.js](https://github.com/auth0/auth0.js/)
- [Tailwind CSS](https://tailwindcss.com/)
- [PostCSS](https://postcss.org/)
- [React Loading Skeleton](https://github.com/dvtng/react-loading-skeleton)
- [NGINX](https://www.nginx.com/)
- [Docker](https://www.docker.com/)

### Common

The application is built for Docker, Docker Compose & Kubernetes with Helm. For local development, Docker Compose is used when debugging the application with Visual Studio, and [Skaffold](https://skaffold.dev/) is used to package the application into a Helm chart to deploy to a local Kubernetes cluster for running locally.

The hosted version of the application is deployed here: https://aks.fakesurveygenerator.mysecondarydomain.com

The following endpoints are accessible:

- [/swagger](https://aks.fakesurveygenerator.mysecondarydomain.com/swagger/index.html) - The Swagger documentation page for the API
- [/health/live](https://aks.fakesurveygenerator.mysecondarydomain.com/health/live) - Health Checks endpoint used by Kubernetes liveness probe
- [/health/ready](https://aks.fakesurveygenerator.mysecondarydomain.com/health/ready) - Health Checks endpoint used by Kubernetes readiness probe

The hosted version utilizes the following infrastructure:

- [Azure Kubernetes Service](https://azure.microsoft.com/en-us/services/kubernetes-service/)
- [Azure SQL Database](https://azure.microsoft.com/en-us/services/sql-database/)
- [Azure Cache for Redis](https://azure.microsoft.com/en-us/services/cache/)
- [Azure Container Registry](https://azure.microsoft.com/en-us/services/container-registry/)
- [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/)
- [Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Azure DNS](https://docs.microsoft.com/en-us/azure/dns/dns-overview)
- [Azure DevOps](https://azure.microsoft.com/en-us/services/devops/) (for CI/CD)

## Authentication

The application makes use of [OpenID Connect](https://openid.net/connect/) for authentication which is implemented by [Auth0](https://auth0.com/). Currently supported connections are:

- Auth0
- Google
- Microsoft

Initially, this project used [IdentityServer](https://identityserver.io/) for authentication, but I didn't feel like maintaining the separate IdentityServer project as well as the Fake Survey Generator, so I switched it out for Auth0 because authn/authz is hard and I prefer to delegate that responsibility to the people who know how to do it properly. 😁

## How do I run this thing?

In order to run FSG on your local machine, you will need the following prerequisites:

To run with Docker Compose:

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [mkcert](https://github.com/FiloSottile/mkcert) - To generate SSL certificates for local development
- [Visual Studio](https://visualstudio.microsoft.com/) (optional)

1. After installing mkcert, run: `mkcert localhost` in the `certs` directory to create the localhost development certificates. If successful, the following will be printed:

   `The certificate is at "./localhost.pem" and the key at "./localhost-key.pem"`

2. In a Terminal/Command Prompt/PowerShell window in the project root, run:

   `docker-compose up`

3. In a browser, navigate to https://localhost:3000 to open up the Fake Survey Generator UI

or

1. Open `FakeSurveyGenerator.sln` in Visual Studio:

2. Make sure that the `docker-compose` project is selected as the startup project

3. Hit `F5` to debug the application, or `Ctrl` + `F5` to run without debugging

4. In a browser, navigate to https://localhost:3000 to open up the Fake Survey Generator UI

To run on local Kubernetes:

- [Docker Desktop](https://www.docker.com/products/docker-desktop) with Kubernetes enabled (Ensure that at least 2048 MB of Memory is allocated to Docker Engine)
- [NGINX Ingress Controller](https://kubernetes.github.io/ingress-nginx/deploy/#contents) installed on the Kubernetes cluster
- [Skaffold](https://skaffold.dev/)

To deploy to a local Kubernetes cluster:

1. Create an entry in your `hosts` file as follows:

   `127.0.0.1 k8s.local`

2. In a Terminal/Command Prompt/PowerShell window in the project root, run:

   `skaffold run`

3. In a browser, navigate to https://k8s.local to open up the Fake Survey Generator UI

## How do I contribute?

If you find a bug, want to add a feature, or want to improve the documentation, open up a PR!

## References

My deepest thanks to all the people who provided these resources as reference:

- [.NET Microservices: Architecture for Containerized .NET Applications](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)
- [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)
- [jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture)
- [Vladimir Khorikov - Applying Functional Principles in C#](https://pluralsight.com/courses/csharp-applying-functional-principles)
- [Vladimir Khorikov - Functional C#: Primitive obsession](https://enterprisecraftsmanship.com/posts/functional-c-primitive-obsession/)

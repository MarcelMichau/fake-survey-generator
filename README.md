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

[![Build Status](https://dev.azure.com/marcelmichau/Personal/_apis/build/status/fake-survey-generator/fake-survey-generator?branchName=main)](https://dev.azure.com/marcelmichau/Personal/_build/latest?definitionId=26&branchName=main)

[Open in github.dev](https://github.dev/MarcelMichau/fake-survey-generator)

## Screenshot

![Screenshot](images/screenshot.png "Screenshot of Fake Survey Generator UI")

## What is this?

This is a .NET | C# | React | TypeScript full-stack application of moderate complexity (not just a to-do app), used as a playground for experimentation. Simply put: This is where I mess around with code. It is heavily inspired by the [.NET Microservices: Architecture for Containerized .NET Applications](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/) book, as well as its companion reference application [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers). It also incorporates various elements from different repos & blog posts which served as inspiration.

It is built using Vertical Slice Architecture principles with CQRS (Command Query Responsibility Segregation) and DDD (Domain-Driven Design) thrown into the mix. It doesn't follow these principles to the letter, but provides a decent example of how to apply the basics of these principles.

It is heavily centered around the Microsoft [.NET](https://dotnet.microsoft.com/) + [Azure](https://azure.microsoft.com/) technology stacks as these are what I have the most experience in & just like building things with. 😀

Here are some of the features incorporated into this project:

### Application Features

- Observable, production ready, distributed application support using [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview)
- Unit & Integration tests for a CQRS/DDD project with [XUnit](https://xunit.net/)
- Target production database for integration tests using [Testcontainers for .NET](https://github.com/testcontainers/testcontainers-dotnet)
- BDD-style acceptance/E2E tests using [SpecFlow](https://specflow.org/) & [Playwright for .NET](https://playwright.dev/dotnet/)
- Implementing health checks for various components using [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
- Adding [Swagger](https://swagger.io/) to an ASP.NET Core Web API using [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- Adding [OpenID Connect](https://openid.net/connect/) for Authentication
- Adding OpenID Connect to Swagger UI
- Securing an ASP.NET Core Web API using JWT Bearer authentication
- Adding security headers to API responses using [NetEscapades.AspNetCore.SecurityHeaders](https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders)
- Using [Hosted Services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services) in ASP.NET Core Web API
- Using a distributed [Redis](https://redis.io/) cache
- Configuring SQL Server retry policies
- Using [Microsoft.Extensions.Http.Resilience](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.http.resilience?view=dotnet-plat-ext-8.0) for resilient HTTP requests
- Implementing [Forwarded Headers](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer) for hosting ASP.NET Core Web API behind a load balancer
- Validation of commands using [FluentValidation](https://fluentvalidation.net/)
- Configuring [Azure Monitor OpenTelemetry](https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable) for telemetry
- Using [Dapr](https://dapr.io/) with [Dapr SDK for .NET](https://github.com/dapr/dotnet-sdk) for platform agnostic integration with infrastructure components

### Infrastructure Features

- Automatic semantic versioning using [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning)
- Creating trusted SSL certificates for HTTPS in development
- Deploying Entity Framework Core Code-First Migrations to [Azure SQL Database](https://azure.microsoft.com/en-us/services/sql-database/) using [Azure Pipelines](https://docs.microsoft.com/en-us/azure/devops/pipelines/get-started/what-is-azure-pipelines?view=azure-devops)
- Using [Microsoft Entra ID](https://www.microsoft.com/en-us/security/business/identity-access/microsoft-entra-id) authentication to [Azure SQL Database](https://azure.microsoft.com/en-us/services/sql-database/) with Entity Framework Core
- Running a microservice application locally using [Docker Compose](https://docs.docker.com/compose/)
- Using Azure Pipelines to build & deploy a microservice application to [Azure Container Apps](https://azure.microsoft.com/en-us/services/container-apps/#overview)
- Infrastructure as Code for Azure resources using [Bicep](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview)

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
- Fake Survey Generator Worker
- Application Project
- Application Tests Project
- API Integration Tests Project
- E2E Acceptance Tests Project

The server side makes use of the following tools, libraries & frameworks:

- Fake Survey Generator API
  - .NET 8.0
  - [ASP.NET Core](https://docs.microsoft.com/en-gb/aspnet/core) 8.0 Web API
  - [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) 8.0 with [Code-First Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)
  - [Dapper](https://github.com/StackExchange/Dapper)
  - [Redis](https://redis.io/)
  - [Swagger](https://swagger.io/)
  - [AutoMapper](https://automapper.org/)
  - [MediatR](https://github.com/jbogard/MediatR)
  - [FluentValidation](https://fluentvalidation.net/)
  - [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
  - [Docker](https://www.docker.com/)
  - [Serilog](https://serilog.net/)
  - [NSubstitute](https://nsubstitute.github.io/)
- Fake Survey Generator Worker
  - .NET 8.0
  - ASP.NET Core 8.0 Worker Service

### Client

The client side consists of the following main components:

- UI

The client side makes use of the following tools, libraries & frameworks:

- [Vite](https://vitejs.dev/)
- [React](https://reactjs.org/)
- [TypeScript](https://www.typescriptlang.org/)
- [auth0.js](https://github.com/auth0/auth0.js/)
- [Tailwind CSS](https://tailwindcss.com/)
- [PostCSS](https://postcss.org/)
- [React Loading Skeleton](https://github.com/dvtng/react-loading-skeleton)
- [NGINX](https://www.nginx.com/)
- [Docker](https://www.docker.com/)

### Common

The application is built for Docker, Docker Compose. For local development, Docker Compose is used when debugging the application with Visual Studio/Rider.

The hosted version of the application is deployed here: https://fakesurveygenerator.mysecondarydomain.com

The following endpoints are accessible:

- [/swagger](https://fakesurveygenerator.mysecondarydomain.com/swagger/index.html) - The Swagger documentation page for the API
- [/health/live](https://fakesurveygenerator.mysecondarydomain.com/health/live) - Health Checks endpoint used by Azure Front Door health probe
- [/health/ready](https://fakesurveygenerator.mysecondarydomain.com/health/ready) - Health Checks endpoint used by Azure Front Door health probe

The hosted version utilizes the following infrastructure:

- [Azure Container Apps](https://azure.microsoft.com/en-us/services/container-apps/#overview)
- [Azure SQL Database](https://azure.microsoft.com/en-us/services/sql-database/)
- [Azure Cache for Redis](https://azure.microsoft.com/en-us/services/cache/)
- [Azure Container Registry](https://azure.microsoft.com/en-us/services/container-registry/)
- [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/)
- [Azure Log Analytics](https://docs.microsoft.com/en-us/azure/azure-monitor/logs/log-analytics-overview)
- [Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Azure Front Door](https://azure.microsoft.com/en-in/pricing/details/frontdoor/#overview)
- [Azure DNS](https://docs.microsoft.com/en-us/azure/dns/dns-overview)
- [Azure DevOps](https://azure.microsoft.com/en-us/services/devops/) (for CI/CD)

## Authentication

The application makes use of [OpenID Connect](https://openid.net/connect/) for authentication which is implemented by [Auth0](https://auth0.com/). Currently supported connections are:

- Auth0
- Google
- Microsoft

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

## How do I contribute?

If you find a bug, want to add a feature, or want to improve the documentation, open up a PR!

## References

My deepest thanks to all the people who provided these resources as reference:

- [.NET Microservices: Architecture for Containerized .NET Applications](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)
- [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)
- [jasontaylordev/CleanArchitecture](https://github.com/jasontaylordev/CleanArchitecture)
- [nadirbad/VerticalSliceArchitecture](https://github.com/nadirbad/VerticalSliceArchitecture)
- [Vladimir Khorikov - Applying Functional Principles in C#](https://pluralsight.com/courses/csharp-applying-functional-principles)
- [Vladimir Khorikov - Functional C#: Primitive obsession](https://enterprisecraftsmanship.com/posts/functional-c-primitive-obsession/)

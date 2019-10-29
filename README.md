<h1 align="center">
  Fake Survey Generator
</h1>

<p align="center">
This is an app. That generates surveys. Fake ones. For fun. That is all.
</p>

| Component                 | Build Status   |
|---------------------------|----------------|
| Fake Survey Generator API | [![Build Status](https://dev.azure.com/marcelmichau-investec/fake-survey-generator/_apis/build/status/Fake%20Survey%20Generator%20API?branchName=master)](https://dev.azure.com/marcelmichau-investec/fake-survey-generator/_build/latest?definitionId=5&branchName=master) |
| Fake Survey Generator UI  | [![Build Status](https://dev.azure.com/marcelmichau-investec/fake-survey-generator/_apis/build/status/Fake%20Survey%20Generator%20UI?branchName=master)](https://dev.azure.com/marcelmichau-investec/fake-survey-generator/_build/latest?definitionId=6&branchName=master) |
| IdentityServer            | [![Build Status](https://dev.azure.com/marcelmichau-investec/fake-survey-generator/_apis/build/status/IdentityServer?branchName=master)](https://dev.azure.com/marcelmichau-investec/fake-survey-generator/_build/latest?definitionId=7&branchName=master) |
| SQL Server                | [![Build Status](https://dev.azure.com/marcelmichau-investec/fake-survey-generator/_apis/build/status/SQL%20Server?branchName=master)](https://dev.azure.com/marcelmichau-investec/fake-survey-generator/_build/latest?definitionId=9&branchName=master) |
| Redis                     | [![Build Status](https://dev.azure.com/marcelmichau-investec/fake-survey-generator/_apis/build/status/Redis?branchName=master)](https://dev.azure.com/marcelmichau-investec/fake-survey-generator/_build/latest?definitionId=8&branchName=master) |

## What is this?

This is an application of moderate complexity, used as a playground for experimentation. Simply put: This is where I mess around with code.  It is heavily inspired by the [.NET Microservices: Architecture for Containerized .NET Applications](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/) book, as well as its companion reference application [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)

## Why is this here?

I wanted something to try new things out on, without the risk of substantially endangering an actual production environment used by actual people.

It was for this reason that I built the Fake Survey Generator (FSG) app as a way to test out tools, libraries, patterns, frameworks & various other stuff.

It has a very simple domain: it generates surveys. Fake ones. They can be used as a tool for helping you decide what to have for dinner, which book you should read next, where you should go for your next team lunch, or anything that tickles your fancy.

This application is also used as a reference for configuring/wiring up some common things I sometimes forget how to do. _Living Documentation_ if you will. You know the culprits: How do I wire up that database again? What is the syntax for that logging configuration? How do I make thing A talk to thing B?

The domain is kept relatively simple such that it doesn't overwhelm the app with unnecessary complexity. It should be quite easy to wrap your head around without requiring a degree in Computer Science.

## How is this thing built?

FSG consists of two parts:

### Server

The server side consists of the following main components:

- Fake Survey Generator API
  - Domain Project
  - Infrastructure Project
  - Domain Unit Tests Project
  - API Integration Tests Project
  - EF Design Project (used purely for EF Core design-time tooling)
- Identity Provider API

The server side makes use of the following tools, libraries & frameworks:

- Fake Survey Generator API
  - .NET Core 3.0
  - ASP.NET Core 3.0 Web API
  - Entity Framework Core 3.0 with Code-First Migrations
  - Dapper
  - Redis
  - Swagger
  - AutoMapper
  - MediatR
  - AspNetCore.Diagnostics.HealthChecks
  - Docker
- Identity Provider API
  - .NET Core 3.0
  - ASP.NET Core 3.0 MVC
  - IdentityServer

### Client

The client side consists of the following main components:

- UI

The client side makes use of the following tools, libraries & frameworks:

- React
- NGINX
- Docker

### Common

The application is built for Docker, Docker Compose & Kubernetes with Helm. For local development, Docker Compose is used when debugging the application with Visual Studio, and Skaffold is used to package the application into a Helm chart to deploy to a local Kubernetes cluster for running locally.

The hosted version of the application is deployed to three environments:

- Integration - https://aks-integration.fakesurveygenerator.marcelmichau.dev
- Test - https://aks-test.fakesurveygenerator.marcelmichau.dev
- Production - https://aks.fakesurveygenerator.marcelmichau.dev

The following endpoints are accessible:

- [/swagger](https://aks.fakesurveygenerator.marcelmichau.dev/swagger/index.html) - The Swagger documentation page for the API
- [/health](https://aks.fakesurveygenerator.marcelmichau.dev/health) - Health Checks endpoint used by Kubernetes liveness probe

The hosted version utilizes the following:

- Azure Kubernetes Service
- Azure SQL Database
- Azure Redis Cache
- Azure Container Registry
- Docker Hub
- Azure DevOps Services (for CI/CD)

## Authentication
The application makes use of OpenID Connect for authentication which is implemented by IdentityServer.  The Identity Provider has a user database with a couple of test users with the following credentials:

- User 1
  - Username: **alice**
  - Password: **Pass123$**
  
- User 2
  - Username: **bob**
  - Password: **Pass123$**
  
There is no interface to register users - the authentication mechanism serves as an example of how to implement user authentication using OpenID Connect in a .NET microservice architecture.

## How do I run this thing?

In order to run FSG on your local machine, you will need the following prerequisites:

To run on local Kubernetes:

- Docker Desktop with Kubernetes enabled (Ensure that at least 2048 MB of Memory is allocated to Docker Engine)
- NGINX Ingress installed on the Kubernetes cluster
- Skaffold

To deploy to a local Kubernetes cluster:

1. Create an entry in your `hosts` file as follows:

   `127.0.0.1 k8s.local`

2. In a Terminal/Command Prompt/PowerShell window in the project root, run:

   `skaffold run`

3. In a browser, navigate to http://k8s.local to open up the Fake Survey Generator UI

To run with Docker Compose:

- Docker Desktop
- Visual Studio (optional)

To run with Docker Compose:

1. In a Terminal/Command Prompt/PowerShell window in the project root, run:

   `docker-compose up`

2. In a browser, navigate to http://localhost:3000 to open up the Fake Survey Generator UI

or

1. Open `FakeSurveyGenerator.sln` in Visual Studio:

2. Make sure that the `docker-compose` project is selected as the startup project

3. Hit `F5` to debug the application, or `Ctrl` + `F5` to run without debugging

4. In a browser, navigate to http://localhost:3000 to open up the Fake Survey Generator UI

## How do I contribute?

If you find a bug, want to add a feature, or want to improve the documentation, open up a PR!

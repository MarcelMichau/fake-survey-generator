<h1 align="center">
  Fake Survey Generator
</h1>

<p align="center">
This is an app. That generates surveys. Fake ones. For fun. That is all.
</p>

<p align="center">
<a href="https://dev.azure.com/marcelmichau-investec/fake-survey-generator/_apis/build/status/MarcelMichau.fake-survey-generator?branchName=master">
    <img src="https://dev.azure.com/marcelmichau-investec/fake-survey-generator/_apis/build/status/MarcelMichau.fake-survey-generator?branchName=master" alt="Azure DevOps Build Status" />
  </a>
</p>

## What is this?

This is an application of moderate complexity, used as a playground for experimentation. Simply put: This is where I mess around with code.

## Why is this here?

I wanted something to try new things out on, without the risk of substantially endangering an actual production environment used by actual people.

It was for this reason that I built the Fake Survey Generator (FSG) app as a way to test out tools, libraries, patterns, frameworks & various other stuff.

It has a very simple domain: it generates surveys. Fake ones. They can be used as a tool for helping you decide what to have for dinner, which book you should read next, where you should go for your next team lunch, or anything that tickles your fancy.

This application is also used as a reference for configuring/wiring up some common things I sometimes forget how to do. _Living Documentation_ if you will. You know the culprits: How do I wire up that database again? What is the syntax for that logging configuration? How do I make thing A talk to thing B?

The domain is kept relatively simple such that it doesn't overwhelm the app with unnecessary complexity. It should be quite easy to wrap your head around without requiring a degree in Computer Science.

## How is this thing built?

There are two main components to FSG:

### Server

The server side consists of the following main components:

- Web API
- Domain Project
- Infrastructure Project
- Domain Unit Tests Project
- API Integration Tests Project
- EF Design Project (used purely for EF Core design-time tooling)

The server side makes use of the following tools, libraries & frameworks:

- .NET Core 3.0
- ASP.NET Core 3.0 Web API
- Entity Framework Core 3.0 with Code-First Migrations
- Redis
- Swagger
- AutoMapper
- MediatR
- Docker

### Client

The client side consists of the following main components:

- UI

The client side makes use of the following tools, libraries & frameworks:

- React
- Docker

### Common

The application is built for Docker, Docker Compose & Kubernetes with Helm. For local development, Docker Compose is used when debugging the application with Visual Studio, and Skaffold is used to package the application into a Helm chart to deploy to a local Kubernetes cluster for running locally.

The hosted version of the application is deployed to two environments:

- Test - https://aks-test.fakesurveygenerator.marcelmichau.dev
- Production - https://aks.fakesurveygenerator.marcelmichau.dev

The hosted version utilizes the following:

- Azure Kubernetes Service
- Azure SQL Database
- Azure Redis Cache
- Azure Container Registry
- Docker Hub
- Azure DevOps Services (for CI/CD)

## How do I run this thing?

1. Create an entry in your `hosts` file as follows:

   `127.0.0.1 kube.local`

2. In a Terminal/Command Prompt/PowerShell window in the project root, run:

   `skaffold run`

3. In a browser, navigate to http://kube.local

## How do I contribute?

If you find a bug, want to add a feature, or just want to correct a spelling mistake, open up a PR!

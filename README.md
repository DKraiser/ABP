# Booking system

**Author:** Oles Andrela

**Application from:** 8.13.26


## Overview

This project is a small test for job application to ABP company. In this project, simple booking system API with in-memory storage is implemented.

## Tech stack

- .NET 10
- OpenAPI documentation with Scalar for rendering
- NUnit + Moq for unit testing

## Quick start

To run application locally:

```bash
# To run use
dotnet run --project src/api

# To run all tests use
dotnet test
# And for specific tests use
dotnet test tests/[tested_layer]
```

To run in container use:

```bash
# Build an image 
docker build . -t abp:latest

# Run in development mode 
docker run -p 8080:8080 --name abp --rm -ti -e ASPNETCORE_ENVIRONMENT=Development abp:latest
# Or in staging
docker run -p 8080:8080 --name abp --rm -ti -e ASPNETCORE_ENVIRONMENT=Staging abp:latest
# Or in production 
docker run -p 8080:8080 --name abp --rm -ti -e ASPNETCORE_ENVIRONMENT=Production abp:latest
```

API overview: `http://localhost:8080/scalar`

Documentation file: [click here](docs/documentation.md)

## To do

- Metrics

## Done

- Domain
- Applicaion
- Infrastructure
- Api
- Configuration
- Healthchecks
- Containerization
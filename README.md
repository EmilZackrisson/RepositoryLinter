[![Unit tests](https://github.com/EmilZackrisson/RepositoryLinter/actions/workflows/unit-tests.yml/badge.svg?branch=main)](https://github.com/EmilZackrisson/RepositoryLinter/actions/workflows/unit-tests.yml)
[![Build docker image](https://github.com/EmilZackrisson/RepositoryLinter/actions/workflows/docker.yaml/badge.svg)](https://github.com/EmilZackrisson/RepositoryLinter/actions/workflows/docker.yaml)
# RepositoryLinter

This is a very simple Git repository linter for an Software Development Project course.

## Features
- Checks that the repository contains a README file and that it contains a LICENSE file.
- Checks that the repository contains a .gitignore file.
- Checks if there are GitHub WorkFlow files (.github/workflows/).
- Checks all files if they contain "test" and print the file name.
- TODO: Checks the repository for secrets using the [Trufflehog](https://github.com/trufflesecurity/trufflehog) project.
- TODO: Run linter on a batch of repositories.

## Run the program

### Run with Docker

```bash
docker run -it --rm -v ./repolinter:/tmp/repolinter ghcr.io/EmilZackrisson/RepositoryLinter:latest -h
```

### Run with .NET Core Runtime

- Clone the repository
- Ensure .NET Core 9 is installed
- Run ```dotnet restore ``` in the root of the repository to restore dependencies
- Run the program ```dotnet run -- -h ``` for help (the "--" is only required when no commands are given.)

## Run tests
The unit tests are using the [xUnit](https://xunit.net/) framework.

- Clone the repository
- Ensure.NET Core 9 is installed
- Run ```dotnet restore``` in the root of the repository to restore dependencies
- Run ```dotnet test``` in the root of the repository to run tests

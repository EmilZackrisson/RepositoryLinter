[![Unit tests](https://github.com/EmilZackrisson/RepositoryLinter/actions/workflows/unit-tests.yml/badge.svg?branch=main)](https://github.com/EmilZackrisson/RepositoryLinter/actions/workflows/unit-tests.yml)
[![Build docker image](https://github.com/EmilZackrisson/RepositoryLinter/actions/workflows/docker.yaml/badge.svg)](https://github.com/EmilZackrisson/RepositoryLinter/actions/workflows/docker.yaml)

# RepositoryLinter

This is a very simple Git repository linter for a Software Development Project course.

## Features

- Checks that the repository contains a README file and that it contains a LICENSE file.
- Checks that the repository contains a .gitignore file.
- Checks if there are GitHub WorkFlow files (.github/workflows/).
- Checks all files if they contain "test" and print the file name.
- Checks the repository for secrets using [Trufflehog](https://github.com/trufflesecurity/trufflehog).
- Run linter on a batch of repositories.

## Run the program

### Run with Docker

```bash
docker run -it --rm -v ./repolinter:/tmp/repolinter ghcr.io/emilzackrisson/repositorylinter:latest -h
```

### Run without Docker

- Clone the repository
- Ensure .NET Core 9 is installed
- Ensure Git is installed
- Ensure [Trufflehog](https://github.com/trufflesecurity/trufflehog) is installed
- Run ```dotnet restore``` in the root of the repository to restore dependencies
- Run the program ```dotnet run -- -h``` for help (the "--" is only required when no commands are given.)

## How to use

For more information on how to use the program, run the program with the -h or --help flag.

```bash
docker run -it --rm -v ./repolinter:/tmp/repolinter ghcr.io/emilzackrisson/repositorylinter:latest --help
```

### Run the linter on a single repository
These guides use the Docker image, but the same commands can be used without Docker.

#### With Git URL
```bash
docker run -it --rm -v ./repolinter:/tmp/repolinter ghcr.io/emilzackrisson/repositorylinter:latest https://github.com/EmilZackrisson/RepositoryLinter
```

#### With local path
```bash
docker run -it --rm -v ./repolinter:/tmp/repolinter ghcr.io/emilzackrisson/repositorylinter:latest /tmp/repolinter/RepositoryLinter
```

### Run the linter on a batch of repositories
```bash
docker run -it --rm -v ./repolinter:/tmp/repolinter ghcr.io/emilzackrisson/repositorylinter:latest /tmp/repolinter/batch.txt
```

The batch file should contain a list of repositories, one per line. Can be URL or local path. Example:
```
https://github.com/EmilZackrisson/RepositoryLinter
/tmp/repolinter/RepositoryLinter
```

## Run tests

The unit tests are using the [xUnit](https://xunit.net/) framework.

- Clone the repository
- Install all dependencies listed in the [Run without Docker](#run-without-docker) section
- Run ```dotnet restore``` in the root of the repository to restore dependencies
- Run ```dotnet test``` in the root of the repository to run tests



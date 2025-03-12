[![Unit tests](https://github.com/EmilZackrisson/RepositoryLinter/actions/workflows/unit-tests.yml/badge.svg?branch=main)](https://github.com/EmilZackrisson/RepositoryLinter/actions/workflows/unit-tests.yml)
[![Build docker image](https://github.com/EmilZackrisson/RepositoryLinter/actions/workflows/docker.yaml/badge.svg)](https://github.com/EmilZackrisson/RepositoryLinter/actions/workflows/docker.yaml)

# RepositoryLinter

This is a very simple Git repository linter for a Software Development Project course.

## Features

- Checks that the repository contains a README file and that it contains a LICENSE.
- Checks that the repository contains a .gitignore file.
- Checks if there are GitHub WorkFlow files (.github/workflows/).
- Checks if there are any test files in the repository. (Files with the word "test" in the path.)
- Checks the repository for secrets using [Trufflehog](https://github.com/trufflesecurity/trufflehog).
- Run linter on a batch of repositories.
- Configuration using a yaml file.

## Run the program

### Run with Docker

```bash
docker run -it --rm -v ./repolinter:/tmp/repolinter ghcr.io/emilzackrisson/repositorylinter:latest -h
```

Output:
```
Description:
  Simple CLI tool to lint git repositories

Usage:
  RepositoryLinter <url-or-path> [options]

Arguments:
  <url-or-path>  URL or path to lint. Path can be a directory or a batch file containing URLs or paths, one per line.

Options:
  --disable-cleanup                    Do not delete the cloned git repository
  --disable-truncate                   Do not truncate the output. By default, the output is truncated to 10 lines per check.
  --ignore-gitignore                   Ignore .gitignore. Includes all files in the search.
  --path-to-save-to <path-to-save-to>  Directory to save the repository to. If not provided, a temporary directory will be used.
  --config <config>                    Path to configuration file.
  --version                            Show version information
  -?, -h, --help                       Show help and usage information
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

## Configuration
The program can be configured using a yaml and the --config flag. This is optional and the program will use the default configuration if no configuration is provided.

```yaml
Checks:
  - Name: "License Exists" # Name of the check. (See https://github.com/EmilZackrisson/RepositoryLinter/blob/main/RepositoryLinter/LintRunner.cs)
    AllowedToFail: false # Default

# Path to save the git repositories to. Default is a temp directory.
PathToSaveGitRepos: "~/RepositoryLinter"

# Truncate the output of each check to 10 lines. Default is true.
TruncateOutput: true # Default

# Clean up the cloned repositories after the program has finished. Default is true.
CleanUp: true # Default
```

### Run with configuration in Docker
```bash
docker run -it --rm -v ./repolinter:/tmp/repolinter -v ./config.yaml:/app/config.yaml ghcr.io/emilzackrisson/repositorylinter:latest https://github.com/EmilZackrisson/RepositoryLinter --config /app/config.yaml
```
The configuration file in ./config.yaml on the host will be used.

### Configuration priority
The configuration priority is:
1. Command line arguments
2. Configuration file
3. Default configuration



## Run tests

The unit tests are using the [xUnit](https://xunit.net/) framework.

- Clone the repository
- Install all dependencies listed in the [Run without Docker](#run-without-docker) section
- Run ```dotnet restore``` in the root of the repository to restore dependencies
- Run ```dotnet test``` in the root of the repository to run tests



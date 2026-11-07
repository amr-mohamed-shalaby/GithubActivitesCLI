# GitHub Activity CLI

A lightweight command-line application for viewing a GitHub user's recent **public activity**. It uses the GitHub REST API and presents events in a readable terminal format.

This project was created as a solution to the [GitHub User Activity challenge on roadmap.sh](https://roadmap.sh/projects/github-user-activity).

## Features

- Fetch recent public activity for a GitHub username.
- Choose how many events to request (between 1 and 100).
- Display readable descriptions for common GitHub event types, including pushes, issues, stars, repository creation, pull requests, forks, comments, deletions, and releases.
- Show formatted help for the whole application or for an individual command.
- Display terminal output with colors and formatted error messages.
- Handle common errors such as unknown commands, invalid arguments, missing users, API rate limits, network failures, and request timeouts.

## Requirements

- The **.NET 10 SDK** (which includes the .NET runtime needed by this tool).
- An internet connection to access the GitHub API.

Check your installed .NET version with:

```bash
dotnet --version
```

The application currently requests public GitHub activity. It does not require a GitHub personal access token. GitHub's unauthenticated API rate limits still apply.

## Installation

There are two ways to install the CLI. Use the NuGet installation once the package has been published to NuGet.org. Until then, install the `.nupkg` package attached to a project release.

### Option 1: Install from NuGet.org

After `GitHubActivity.Cli` has been published to NuGet.org, install it globally:

```bash
dotnet tool install --global GitHubActivity.Cli
```

The command will then be available from your terminal:

```bash
github-activity --help
```

### Option 2: Install from a downloaded release package

1. Open the project's **Releases** page.
2. Download the `GitHubActivity.Cli.<version>.nupkg` file attached to the release.
3. Put the downloaded `.nupkg` file in a folder on your computer.
4. Run `dotnet tool install` and point `--add-source` to the **folder containing the package**.

For example, in PowerShell on Windows, if the package is in `C:\Users\YourName\Downloads\GitHubActivity`:

```powershell
dotnet tool install --global --add-source "$HOME\Downloads\GitHubActivity" GitHubActivity.Cli
```

On macOS or Linux, the equivalent command might be:

```bash
dotnet tool install --global --add-source "$HOME/Downloads/GitHubActivity" GitHubActivity.Cli
```

If the folder contains more than one version of the package, specify the version explicitly:

```powershell
dotnet tool install --global --add-source "$HOME\Downloads\GitHubActivity" GitHubActivity.Cli --version 1.0.0
```

Replace `1.0.0` with the version of the package you downloaded.

### Check the installation

```bash
dotnet tool list --global
```

Then open a **new terminal window** and run:

```bash
github-activity --help
```

If the command is not recognized, make sure the global .NET tools directory is on your `PATH`:

- **Windows:** `%USERPROFILE%\.dotnet\tools`
- **macOS/Linux:** `$HOME/.dotnet/tools`

After changing `PATH`, restart the terminal.

## Usage

### Show general help

```bash
github-activity
```

or:

```bash
github-activity --help
github-activity -h
github-activity help
```

### Show help for a specific command

```bash
github-activity get --help
```

You can also use the help command:

```bash
github-activity help -command get
```

### Get a user's recent activity

```bash
github-activity get -username amr-mohamed-shalaby
```

By default, the command requests 30 events.

### Choose the number of events

```bash
github-activity get -username amr-mohamed-shalaby -count 10
```

Short parameter aliases are also available:

```bash
github-activity get -un amr-mohamed-shalaby -c 10
```

## Commands and parameters

### `get`

Fetch recent public activity for a GitHub user.

| Parameter | Alias | Required | Default | Description |
|---|---|---:|---:|---|
| `-username` | `-un` | Yes | — | GitHub username to look up. |
| `-count` | `-c` | No | `30` | Number of events to request. Accepted range: 1–100. |

Examples:

```bash
github-activity get -username octocat
github-activity get -un octocat -c 50
```

### `help`

Display the available commands or detailed information about one command.

| Parameter | Alias | Required | Description |
|---|---|---:|---|
| `-command` | `-c` | No | Command to display detailed help for. |

Example:

```bash
github-activity help -command get
```

## Example output

The values below are illustrative; the actual events depend on the selected GitHub account and the data returned by GitHub.

```text
╭────────────────────────────────────────────────────────────╮
│                 GITHUB USER ACTIVITY                       │
│                 @octocat                                    │
├────────────────────────────────────────────────────────────┤

  07 Nov 2026, 02:30 PM   PUSH EVENT
  └─ octocat pushed commits to octocat/hello-world (Branch: main)

  07 Nov 2026, 01:15 PM   ISSUES EVENT
  └─ octocat opened an issue in octocat/hello-world

╰────────────────────────────────────────────────────────────╯
```

## How it works

The `get` command calls the GitHub REST API endpoint below:

```text
GET https://api.github.com/users/{username}/events?per_page={count}
```

The application deserializes the JSON response and formats the returned events for terminal display. It reports recent **public** activity; it is not a complete history of a user's private activity or a full audit log of every action on GitHub. The number requested per page is limited to 100 by this application.

For API details, see the [GitHub REST API documentation for user events](https://docs.github.com/en/rest/activity/events#list-events-for-the-authenticated-user) and the [GitHub REST API documentation](https://docs.github.com/en/rest).

## Updating

If installed from NuGet.org:

```bash
dotnet tool update --global GitHubActivity.Cli
```

If installing an updated `.nupkg` from a release, use the folder containing the new package and its version. For example:

```powershell
dotnet tool update --global --add-source "$HOME\Downloads\GitHubActivity" GitHubActivity.Cli --version 1.0.1
```

Replace `1.0.1` with the version you downloaded.

## Uninstalling

```bash
dotnet tool uninstall --global GitHubActivity.Cli
```

## Building from source

Clone the repository, then run these commands from the project directory:

```bash
dotnet restore
dotnet build --configuration Release
dotnet pack --configuration Release --output ./nupkg
```

To install the locally built package globally:

```bash
dotnet tool install --global --add-source ./nupkg GitHubActivity.Cli
```

## Challenge

This project was inspired by the **[GitHub User Activity](https://roadmap.sh/projects/github-user-activity)** project from roadmap.sh. The challenge is to build a command-line application that accepts a GitHub username, retrieves that user's recent activity from the GitHub API, and displays a human-readable summary in the terminal.

This repository is an independent implementation of that learning challenge using C# and .NET.

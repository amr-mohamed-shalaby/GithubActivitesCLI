using System.Net.Http.Headers;
using System.Net;
using GithubActivity.Exceptions;
using System.Text.Json;
using GithubActivity.Models.Github;
using GithubActivity.Models;
using GithubActivity.Helpers;

namespace GithubActivity.Commands
{
    public class GetHubUserActivitesCommand : ICommand
    {
        public string Command => "get";
        public string Description => "Fetch recent public activity for a GitHub user.";

        public List<CommandParam> Params => new()
        {
            new()
            {
                Name = "username",
                Abbreviation = "un",
                Description = "GitHub username",
                IsRequired = true
            },
            new()
            {
                Name = "count",
                Abbreviation = "c",
                Description = "Number of events to return",
                IsRequired = false,
                DefaultValue = "10"
            }
        };

        public async Task<string> Execute(string[] args)
        {
            var passedParams = CommandParamsHelpers.Extract(
                args.Skip(1).ToArray(),
                Params
            );

            string userName = passedParams["username"];

            if (!int.TryParse(passedParams["count"], out int count) ||
                count < 1 || count > 100)
            {
                throw new ValidationException(
                    "Count must be a number between 1 and 100."
                );
            }
            if (count < 1 || count > 100)
            {
                throw new ValidationException(
                    "Count must be a number between 1 and 100."
                );
            }

            if (userName.Any(char.IsWhiteSpace))
            {
                throw new ValidationException("Invalid GitHub username.");
            }

            var url = GetGithubGetActivitiesUrl(userName, count);

            string responseStringify;

            using var httpClient = new HttpClient();

            httpClient.Timeout = TimeSpan.FromSeconds(15);

            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "GitHubActivity/1.0");

            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github+json")
            );

            try
            {
                using var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    HandleError(response);
                }

                responseStringify = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException)
            {
                throw new ProcessException(
                    ConsoleFormatter.FormatError(
                        "REQUEST TIMEOUT",
                        "GitHub did not respond within the allowed time."
                    ));
            }
            catch (HttpRequestException)
            {
                throw new ProcessException(
                    ConsoleFormatter.FormatError(
                        "NETWORK ERROR",
                        "Could not connect to GitHub. Check your internet connection."
                    ));
            }

            List<GithubUserEventModel> activities;

            try
            {
                activities = System.Text.Json.JsonSerializer
                    .Deserialize<List<GithubUserEventModel>>(
                        responseStringify,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    ) ?? new List<GithubUserEventModel>();
            }
            catch (System.Text.Json.JsonException)
            {
                throw new ProcessException(
                    ConsoleFormatter.FormatError(
                        "INVALID RESPONSE",
                        "GitHub returned data that could not be processed."
                    ));
            }

            return FormatActivities(userName, activities);
        }

        private static void HandleError(HttpResponseMessage response)
        {
            int statusCode = (int)response.StatusCode;

            bool rateLimitReached =
                response.StatusCode == HttpStatusCode.TooManyRequests ||
                (
                    response.StatusCode == HttpStatusCode.Forbidden &&
                    response.Headers.TryGetValues(
                        "X-RateLimit-Remaining", out var values) &&
                    values.Contains("0")
                );

            if (rateLimitReached)
            {
                string message = "GitHub API rate limit reached.";

                var retryAfter = response.Headers.RetryAfter;

                if (retryAfter?.Delta is TimeSpan delay)
                {
                    message += $" Try again in {(int)delay.TotalMinutes + 1} minute(s).";
                }
                else if (retryAfter?.Date is DateTimeOffset date)
                {
                    message += $" Try again after {date.ToLocalTime():hh:mm tt}.";
                }

                throw new ProcessException(
                    ConsoleFormatter.FormatError("RATE LIMIT EXCEEDED", message));
            }

            string errorMessage = statusCode switch
            {
                400 => "The request was invalid.",
                401 => "Authentication with GitHub failed.",
                403 => "GitHub denied access to this resource.",
                404 => "GitHub user was not found.",
                422 => "GitHub could not process the request.",
                >= 500 => "GitHub servers are currently unavailable.",
                _ => $"Unexpected HTTP error: {statusCode} ({response.ReasonPhrase})."
            };

            throw new ProcessException(
                ConsoleFormatter.FormatError("GITHUB API ERROR", errorMessage));
        }

        private static void ValidateArgs(string[] args)
        {
            if (args.Length > 2 || args.Length < 2)
                throw new ValidationException("github-activity accept one paramter \"github user name\"");
            if(args[1].Split(' ').Length > 1)
                throw new ValidationException("invalid github user name");
        }

        private string GetGithubGetActivitiesUrl(string userName, int count = 10)
        {
            return $"https://api.github.com/users/{Uri.EscapeDataString(userName)}/events?per_page={count}";
        }
        
        private static string FormatActivities(
            string username,
            List<GithubUserEventModel> activities)
        {
            var output = new System.Text.StringBuilder();

            output.AppendLine();
            output.AppendLine("╭──────────────────────────────────────────────────────────╮");
            output.AppendLine("│                 GITHUB USER ACTIVITY                     │");
            output.AppendLine($"│                 @{username,-41}│");
            output.AppendLine("├──────────────────────────────────────────────────────────┤");

            if (activities.Count == 0)
            {
                output.AppendLine("│              No recent activity found.                   │");
            }
            else
            {
                foreach (var activity in activities)
                {
                    var date = activity.created_at.ToLocalTime()
                        .ToString("dd MMM yyyy, hh:mm tt");

                    output.AppendLine();
                    output.AppendLine($"  {date}   {GetEventName(activity.type)}");
                    output.AppendLine($"  └─ {DescribeEvent(activity)}");
                }
            }

            output.AppendLine();
            output.AppendLine("╰──────────────────────────────────────────────────────────╯");

            return output.ToString();
        }


        private static string GetEventName(string type)
        {
            return type.Replace("Event", " Event")
                    .ToUpperInvariant();
        }


        private static string DescribeEvent(GithubUserEventModel activity)
        {
            string actor = activity.actor?.login ?? "Someone";
            string repo = activity.repo?.name ?? "unknown repository";

            var payload = activity.payload;

            return activity.type switch
            {
                "PushEvent" =>
                    $"{actor} pushed commits to {repo}" +
                    GetBranch(payload?.@ref),

                "IssuesEvent" =>
                    $"{actor} {payload?.action ?? "updated"} an issue in {repo}",

                "WatchEvent" =>
                    $"{actor} starred {repo}",

                "CreateEvent" =>
                    DescribeCreateEvent(activity),

                "PullRequestEvent" =>
                    $"{actor} {payload?.action ?? "updated"} a pull request in {repo}",

                "ForkEvent" =>
                    $"{actor} forked {repo}",

                "IssueCommentEvent" =>
                    $"{actor} commented on an issue in {repo}",

                "DeleteEvent" =>
                    $"{actor} deleted a {payload?.ref_type ?? "reference"} " +
                    $"in {repo}",

                "ReleaseEvent" =>
                    $"{actor} {payload?.action ?? "published"} a release in {repo}",

                _ =>
                    $"{actor} performed {activity.type} in {repo}"
            };
        }


        private static string DescribeCreateEvent(GithubUserEventModel activity)
        {
            var payload = activity.payload;
            string repo = activity.repo?.name ?? "unknown repository";

            if (payload?.ref_type == "repository")
                return $"Created a new repository: {repo}";

            return $"Created a new {payload?.ref_type ?? "reference"} " +
                $"{payload?.@ref} in {repo}";
        }


        private static string GetBranch(string? reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
                return string.Empty;

            return $" (Branch: {reference.Replace("refs/heads/", "")})";
        }

    }
}
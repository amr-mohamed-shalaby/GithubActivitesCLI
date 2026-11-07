using GithubActivity.Exceptions;
using GithubActivity.Models;
using GithubActivity.Helpers;
using GithubActivity.Data;
using System.Text;

namespace GithubActivity.Commands
{
    public class HelpCommand : ICommand
    {
        public string Command => "help";

        public string Description =>
            "Display available commands and their usage.";

        public List<CommandParam> Params => new()
        {
            new()
            {
                Name = "command",
                Abbreviation = "c",
                Description = "Show detailed help for a specific command",
                IsRequired = false
            }
        };

        public Task<string> Execute(string[] args)
        {
            var passedParams = CommandParamsHelpers.Extract(
                args.Skip(1).ToArray(),
                Params
            );

            var commands = CommandsAvailable.GetCommands();

            // Detailed help for a specific command.
            if (passedParams.TryGetValue("command", out var commandName))
            {
                var selectedCommand = commands.FirstOrDefault(c =>
                    c.Command.Equals(
                        commandName,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

                if (selectedCommand == null)
                {
                    throw new ValidationException(
                        $"Command '{commandName}' does not exist."
                    );
                }

                return Task.FromResult(
                    FormatCommandDetails(selectedCommand)
                );
            }

            // General help.
            return Task.FromResult(FormatOverview(commands));
        }


        private static string FormatOverview(
            IReadOnlyList<ICommand> commands)
        {
            var output = new StringBuilder();

            output.AppendLine();
            output.AppendLine("╭──────────────────────────────────────────────────────────────╮");
            output.AppendLine("│                     GITHUB ACTIVITY CLI                     │");
            output.AppendLine("│                         HELP CENTER                          │");
            output.AppendLine("╰──────────────────────────────────────────────────────────────╯");

            output.AppendLine();
            output.AppendLine(" USAGE");
            output.AppendLine("   GithubActivity <command> [options]");

            output.AppendLine();
            output.AppendLine(" AVAILABLE COMMANDS");
            output.AppendLine(" ─────────────────────────────────────────────────────────────");

            foreach (var command in commands.OrderBy(c => c.Command))
            {
                output.AppendLine(
                    $"   {command.Command,-12} {command.Description}"
                );
            }

            output.AppendLine();
            output.AppendLine(" GETTING HELP");
            output.AppendLine("   GithubActivity help");
            output.AppendLine("       Display all available commands.");
            output.AppendLine();
            output.AppendLine("   GithubActivity help -command get");
            output.AppendLine("       Display detailed information about a command.");
            output.AppendLine();
            output.AppendLine(" COMMAND-SPECIFIC HELP");
            output.AppendLine("   GithubActivity get --help");
            output.AppendLine("   GithubActivity get -h");
            output.AppendLine();
            output.AppendLine("   Display detailed help for any command.");
            output.AppendLine();
            output.AppendLine("╰──────────────────────────────────────────────────────────────╯");

            return output.ToString();
        }


        private static string FormatCommandDetails(ICommand command)
        {
            var output = new StringBuilder();

            output.AppendLine();
            output.AppendLine("╭──────────────────────────────────────────────────────────────╮");
            output.AppendLine($"│  COMMAND: {command.Command.ToUpperInvariant(),-50}│");
            output.AppendLine("╰──────────────────────────────────────────────────────────────╯");

            output.AppendLine();
            output.AppendLine(" DESCRIPTION");
            output.AppendLine($"   {command.Description}");

            output.AppendLine();
            output.AppendLine(" USAGE");
            output.AppendLine($"   {BuildUsage(command)}");

            output.AppendLine();
            output.AppendLine(" PARAMETERS");
            output.AppendLine(" ─────────────────────────────────────────────────────────────");

            if (command.Params.Count == 0)
            {
                output.AppendLine("   This command does not accept parameters.");
            }
            else
            {
                var parameterLabels = command.Params.Select(p =>
                {
                    string aliases = $"-{p.Name}";

                    if (!string.IsNullOrWhiteSpace(p.Abbreviation))
                        aliases += $", -{p.Abbreviation}";

                    return $"{aliases} <{p.Name}>";
                }).ToList();

                int columnWidth = parameterLabels.Max(x => x.Length) + 3;

                for (int i = 0; i < command.Params.Count; i++)
                {
                    var parameter = command.Params[i];

                    string requirement = parameter.IsRequired
                        ? "Required"
                        : parameter.DefaultValue != null
                            ? $"Optional | Default: {parameter.DefaultValue}"
                            : "Optional";

                    output.AppendLine(
                        $"   {parameterLabels[i].PadRight(columnWidth)}" +
                        $"{parameter.Description} [{requirement}]"
                    );
                }
            }

            output.AppendLine();
            output.AppendLine("╰──────────────────────────────────────────────────────────────╯");

            return output.ToString();
        }


        private static string BuildUsage(ICommand command)
        {
            var usage = new StringBuilder("GithubActivity ");
            usage.Append(command.Command);

            foreach (var parameter in command.Params)
            {
                string argument = $"-{parameter.Name} <{parameter.Name}>";

                if (parameter.IsRequired)
                {
                    usage.Append($" {argument}");
                }
                else
                {
                    usage.Append($" [{argument}]");
                }
            }

            return usage.ToString();
        }

        public static string GetCommandHelp(ICommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            return FormatCommandDetails(command);
        }
    }
}
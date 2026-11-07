using GithubActivity.Commands;
using GithubActivity.Exceptions;

namespace GithubActivity.Data
{
    public static class CommandsAvailable
    {
        private static readonly List<ICommand> Commands = new()
        {
            new GetHubUserActivitesCommand(),
            new HelpCommand()
        };

        public static IReadOnlyList<ICommand> GetCommands()
        {
            return Commands;
        }

        public static ICommand GetCommand(string[] args)
        {
            // Show help when no command is provided.
            if (args.Length == 0)
                return Commands.First(c => c.Command == "help");

            string commandName = args[0];

            var command = Commands.FirstOrDefault(c =>
                c.Command.Equals(
                    commandName,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (command == null)
            {
                throw new ValidationException(
                    $"Unknown command '{commandName}'. " +
                    "Run 'GithubActivity help' to see available commands."
                );
            }

            return command;
        }
    }
}
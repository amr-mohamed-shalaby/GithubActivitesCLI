
using GithubActivity.Commands;
using GithubActivity.Data;
using GithubActivity.Exceptions;
using GithubActivity.Helpers;

try
{
    // 1. Handle root-level help.
    if (args.Length == 0 || IsHelpOption(args[0]))
    {
        var helpCommand = CommandsAvailable.GetCommand(
            new[] { "help" }
        );

        var helpResult = await helpCommand.Execute(
            new[] { "help" }
        );

        ConsoleFormatter.WriteOutput(helpResult);
        return;
    }

    // 2. Resolve the requested command.
    var currentCommand = CommandsAvailable.GetCommand(args);

    // 3. Handle command-specific help.
    bool helpRequested = args.Skip(1).Any(IsHelpOption);

    if (helpRequested)
    {
        ConsoleFormatter.WriteOutput(
            HelpCommand.GetCommandHelp(currentCommand)
        );

        return;
    }

    // 4. Execute the command.
    var result = await currentCommand.Execute(args);

    ConsoleFormatter.WriteOutput(result);
}
catch (ValidationException ex)
{
    ConsoleFormatter.WriteError(ex.Message, "INVALID ARGUMENTS");
}
catch (ProcessException ex)
{
    ConsoleFormatter.WriteError(ex.Message, "PROCESSING ERROR");
}
catch (Exception ex)
{
    ConsoleFormatter.WriteError(ex.Message, "UNEXPECTED ERROR");
}

static bool IsHelpOption(string argument)
{
    return argument.Equals("-h", StringComparison.OrdinalIgnoreCase)
        || argument.Equals("--help", StringComparison.OrdinalIgnoreCase);
}

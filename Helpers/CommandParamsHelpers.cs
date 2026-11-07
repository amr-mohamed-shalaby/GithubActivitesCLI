
using GithubActivity.Exceptions;
using GithubActivity.Models;

namespace GithubActivity.Helpers
{
    public static class CommandParamsHelpers
    {
        public static Dictionary<string, string> Extract(
            string[] args,
            IEnumerable<CommandParam> availableParams)
        {
            ArgumentNullException.ThrowIfNull(args);
            ArgumentNullException.ThrowIfNull(availableParams);

            var parameters = availableParams.ToList();

            // Map parameter names and abbreviations to their definitions.
            var definitions = new Dictionary<string, CommandParam>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var param in parameters)
            {
                if (string.IsNullOrWhiteSpace(param.Name))
                    throw new InvalidOperationException(
                        "A command parameter must have a name.");

                definitions.Add(param.Name, param);

                if (!string.IsNullOrWhiteSpace(param.Abbreviation))
                    definitions.Add(param.Abbreviation, param);
            }

            var result = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            int i = 0;

            // 1. Parse supplied arguments.
            while (i < args.Length)
            {
                string? key = NormalizeParameter(args[i]);

                if (key == null)
                {
                    throw new ValidationException(
                        $"Unexpected argument: '{args[i]}'.");
                }

                if (!definitions.TryGetValue(key, out var definition))
                {
                    throw new ValidationException(
                        $"Unknown parameter: '-{key}'.");
                }

                // Prevent duplicate parameters.
                if (result.ContainsKey(definition.Name))
                {
                    throw new ValidationException(
                        $"Parameter '-{definition.Name}' was provided more than once.");
                }

                i++;

                var valueParts = new List<string>();

                // Collect values until another parameter is encountered.
                while (i < args.Length &&
                       NormalizeParameter(args[i]) == null)
                {
                    valueParts.Add(args[i]);
                    i++;
                }

                string value = string.Join(" ", valueParts).Trim();

                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ValidationException(
                        $"Parameter '-{definition.Name}' requires a value.");
                }

                result.Add(definition.Name, value);
            }

            // 2. Apply default values to missing optional parameters.
            foreach (var param in parameters)
            {
                if (!result.ContainsKey(param.Name) &&
                    param.DefaultValue != null)
                {
                    result.Add(param.Name, param.DefaultValue);
                }
            }

            // 3. Validate required parameters.
            foreach (var param in parameters)
            {
                if (param.IsRequired &&
                    (!result.TryGetValue(param.Name, out var value) ||
                     string.IsNullOrWhiteSpace(value)))
                {
                    throw new ValidationException(
                        $"Missing required parameter: '-{param.Name}'.");
                }
            }

            return result;
        }

        private static string? NormalizeParameter(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            if (token.StartsWith("--"))
                return token.Length > 2 ? token[2..] : string.Empty;

            if (token.StartsWith("-"))
                return token.Length > 1 ? token[1..] : string.Empty;

            return null;
        }
    }
}

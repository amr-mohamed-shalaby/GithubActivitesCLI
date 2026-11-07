
using System.Globalization;
using System.Text;

namespace GithubActivity.Helpers
{
    public static class ConsoleFormatter
    {
        private const int ErrorContentWidth = 58;

        // Formats an error into a reusable bordered message.
        public static string FormatError(string title, string message)
        {
            string borderTop = "╭" + new string('─', 60) + "╮";
            string borderBottom = "╰" + new string('─', 60) + "╯";

            var lines = new List<string>
            {
                borderTop,
                FormatBoxLine($"✖ {title}"),
                FormatBoxLine("")
            };

            foreach (var line in WrapText(message, ErrorContentWidth))
            {
                lines.Add(FormatBoxLine(line));
            }

            lines.Add(borderBottom);

            return string.Join(Environment.NewLine, lines);
        }

        // Displays normal output using contextual colors.
        public static void WriteOutput(string? output)
        {
            if (string.IsNullOrEmpty(output))
                return;

            WriteLines(output, GetOutputColor);
        }

        // Displays errors in red.
        public static void WriteError(
            string message,
            string title = "ERROR")
        {
            bool alreadyFormatted =
                message.TrimStart().StartsWith("╭") &&
                message.Contains("╰");

            string formatted = alreadyFormatted
                ? message
                : FormatError(title, message);

            WriteLines(
                formatted,
                line => IsDecorativeBorder(line)
                    ? ConsoleColor.DarkRed
                    : ConsoleColor.Red
            );
        }

        // Displays success messages in green.
        public static void WriteSuccess(string message)
        {
            WriteUniform($"✓ {message}", ConsoleColor.Green);
        }

        // Displays warnings in yellow.
        public static void WriteWarning(string message)
        {
            WriteUniform($"⚠ {message}", ConsoleColor.Yellow);
        }

        // Displays informational messages in cyan.
        public static void WriteInfo(string message)
        {
            WriteUniform($"ℹ {message}", ConsoleColor.Cyan);
        }

        private static void WriteUniform(
            string message,
            ConsoleColor color)
        {
            WriteLines(message, _ => color);
        }

        private static void WriteLines(
            string text,
            Func<string, ConsoleColor> colorSelector)
        {
            // Preserve plain output when redirected to a file or pipe.
            if (Console.IsOutputRedirected)
            {
                Console.Write(text);

                if (!text.EndsWith('\n'))
                    Console.WriteLine();

                return;
            }

            var originalColor = Console.ForegroundColor;

            try
            {
                using var reader = new StringReader(text);

                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    Console.ForegroundColor = colorSelector(line);
                    Console.WriteLine(line);
                }
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }

        private static ConsoleColor GetOutputColor(string line)
        {
            string value = line.Trim();

            if (string.IsNullOrWhiteSpace(value))
                return ConsoleColor.Gray;

            // Main titles.
            if (value.Contains("GITHUB USER ACTIVITY") ||
                value.Contains("GITHUB ACTIVITY CLI") ||
                value.Contains("HELP CENTER") ||
                value.Contains("COMMAND:"))
            {
                return ConsoleColor.Cyan;
            }

            // Decorative borders.
            if (IsDecorativeBorder(line))
                return ConsoleColor.DarkGray;

            // Section headings.
            string[] headings =
            {
                "USAGE",
                "DESCRIPTION",
                "PARAMETERS",
                "AVAILABLE COMMANDS",
                "GETTING HELP",
                "COMMAND-SPECIFIC HELP"
            };

            if (headings.Contains(value, StringComparer.OrdinalIgnoreCase))
                return ConsoleColor.Cyan;

            // Activity timestamps.
            if (HasActivityTimestamp(value))
                return ConsoleColor.Yellow;

            // Activity descriptions.
            if (value.StartsWith("└─"))
                return ConsoleColor.Gray;

            // Parameter requirements and defaults.
            if (value.Contains("Required"))
                return ConsoleColor.Yellow;

            if (value.Contains("Default:"))
                return ConsoleColor.Green;

            // Usage examples.
            if (value.StartsWith(
                "GithubActivity ",
                StringComparison.OrdinalIgnoreCase))
            {
                return ConsoleColor.Green;
            }

            return ConsoleColor.Gray;
        }

        private static bool HasActivityTimestamp(string value)
        {
            string datePart = value.Split(
                "   ",
                StringSplitOptions.None
            )[0];

            return DateTime.TryParseExact(
                datePart,
                "dd MMM yyyy, hh:mm tt",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _
            );
        }

        private static bool IsDecorativeBorder(string line)
        {
            string value = line.Trim();

            return value.Length > 0 &&
                   value.All(c =>
                       "╭╮╰╯├┤─│ ".Contains(c));
        }

        private static string FormatBoxLine(string content)
        {
            return $"│  {content.PadRight(ErrorContentWidth)}│";
        }

        private static IEnumerable<string> WrapText(
            string text,
            int maxWidth)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                yield return "";
                yield break;
            }

            var currentLine = new StringBuilder();

            var words = text.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (var word in words)
            {
                if (word.Length > maxWidth)
                {
                    if (currentLine.Length > 0)
                    {
                        yield return currentLine.ToString();
                        currentLine.Clear();
                    }

                    for (int i = 0; i < word.Length; i += maxWidth)
                    {
                        yield return word.Substring(
                            i,
                            Math.Min(maxWidth, word.Length - i)
                        );
                    }

                    continue;
                }

                if (currentLine.Length == 0)
                {
                    currentLine.Append(word);
                }
                else if (currentLine.Length + word.Length + 1 <= maxWidth)
                {
                    currentLine.Append(' ').Append(word);
                }
                else
                {
                    yield return currentLine.ToString();

                    currentLine.Clear();
                    currentLine.Append(word);
                }
            }

            if (currentLine.Length > 0)
                yield return currentLine.ToString();
        }
    }
}

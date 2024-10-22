using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace RPGGame
{
    internal class CustomConsoleFormatter() : ConsoleFormatter("RPG")
    {
        public const int MaxCategoryLength = 12;

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
        {
            Console.ForegroundColor = logEntry.LogLevel switch
            {
                LogLevel.Trace => ConsoleColor.DarkGray,
                LogLevel.Debug => ConsoleColor.White,
                LogLevel.Information => ConsoleColor.Cyan,
                LogLevel.Warning => ConsoleColor.DarkYellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                _ => ConsoleColor.White,
            };

            string message = logEntry.Formatter.Invoke(logEntry.State, logEntry.Exception);
            string padding = logEntry.Category.Length < MaxCategoryLength
                ? new string(' ', MaxCategoryLength - logEntry.Category.Length + 1)
                : "";
            textWriter.WriteLine($"{logEntry.LogLevel.ToString()[..4]}: [{logEntry.Category}{padding}] {message}");
        }
    }
}

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

namespace RPGGame.GameObject.Entity
{
    [EditorEntity("Logger", "Used to write a message to the console log.", "Tool.Debug")]
    public class Logger(string name, Vector2 position, Vector2 size, string? texture) : Entity(name, position, size, texture)
    {
        private const string messageFormat = "Log from Entity \"{Sender}\" via \"{Name}\": {Message}";

        [ActionMethod("Write a message to the log with the given severity")]
        [ActionMethodParameter("Severity", "The severity of the log entry to write", typeof(LogLevel))]
        [ActionMethodParameter("Message", "The body of the log entry to write", typeof(string))]
        protected void WriteLog(Entity sender, Dictionary<string, object?> parameters)
        {
            logger.Log(
                (LogLevel)(parameters.GetValueOrDefault("Severity", LogLevel.None) ?? LogLevel.None),
                messageFormat, sender.Name, Name,
                parameters.GetValueOrDefault("Message", "No message provided."));
        }
    }
}

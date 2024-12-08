using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject.Entity
{
    [EditorEntity("ActionBroadcast", "Triggers an action method on all matching entities", "Tool.Dynamic.EventAction")]
    public class ActionBroadcast(string name, Vector2 position, Vector2 size) : Entity(name, position, size)
    {
        [JsonProperty]
        [EditorModifiable("Action Method", "The name of the target action method to trigger")]
        public string TargetActionMethod { get; set; } = "";

        [JsonProperty]
        [EditorModifiable("Entity Regex", "A regular expression to determine which entities to fire the given action method on")]
        public string TargetEntityRegex { get; protected set; } = "";

        [JsonProperty]
        [EditorModifiable("Parameters", "A list of parameters to pass to the action method", EditType.EntityLink)]
        public List<string> Parameters { get; protected set; } = new();

        protected Dictionary<string, object?> parameterDictionary = new();

        protected override void InitLogic()
        {
            base.InitLogic();

            // Convert list of names to list of entities
            IActionBroadcastParameter[] parameterEntities = Parameters.Select(t => CurrentRoom?.LoadedNamedEntities.GetValueOrDefault(t))
                .OfType<IActionBroadcastParameter>().ToArray();

            if (parameterEntities.Length != Parameters.Count)
            {
                logger.LogWarning("ActionBroadcast \"{Name}\" had {Count} parameters which either could not be found or were not of type ActionBroadcastParameter.",
                    Name, Parameters.Count - parameterEntities.Length);
            }

            parameterDictionary = parameterEntities.ToDictionary(e => e.ParameterName, e => e.ParameterValueObj);
        }

        [ActionMethod("Fires the given action method on all matching entities")]
        protected void Broadcast(Entity sender, Dictionary<string, object?> parameters)
        {
            if (CurrentRoom is null)
            {
                logger.LogError("Cannot broadcast action methods without a loaded room.");
                return;
            }

            foreach (Entity entity in CurrentRoom.Entities.Where(e => Regex.IsMatch(e.Name, TargetEntityRegex)))
            {
                entity.RunActionMethod(TargetActionMethod, sender, parameterDictionary);
            }
        }
    }

    // Required to be able to access ActionBroadcastParameter<T> instances without knowing the type they hold
    internal interface IActionBroadcastParameter
    {
        public string ParameterName { get; set; }
        public object? ParameterValueObj { get; }
    }

    [EditorEntity("ActionBroadcastParameter", "Holds the name and value of a single parameter for an ActionBroadcast entity", "Tool.Dynamic.EventAction")]
    public class ActionBroadcastParameter<T>(string name, Vector2 position, Vector2 size) : Entity(name, position, size), IActionBroadcastParameter
    {
        [JsonProperty]
        [EditorModifiable("Action Parameter Name", "The name of the parameter to pass to the action method")]
        public string ParameterName { get; set; } = "";

        [JsonProperty]
        [EditorModifiable("Action Parameter Value", "The value of the parameter to pass to the action method")]
        public T? ParameterValue { get; set; }

        public object? ParameterValueObj => ParameterValue;
    }
}

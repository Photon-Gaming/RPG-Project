using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject.Entity
{
    [EditorEntity("Counter", "Holds a value that can be incremented or decremented", "Tool.Numeric")]
    [FiresEvent("OnMinimumReached", "Fired every time the minimum value of the counter is met or exceeded")]
    [FiresEvent("OnMaximumReached", "Fired every time the maximum value of the counter is met or exceeded")]
    public class Counter(string name, Vector2 position, Vector2 size) : Entity(name, position, size)
    {
        [JsonProperty]
        [EditorModifiable("Minimum Value", "The inclusive lower bound of the counter to fire when reached")]
        public long MinimumValue { get; set; } = 0;

        [JsonProperty]
        [EditorModifiable("Maximum Value", "The inclusive upper bound of the counter to fire when reached")]
        public long MaximumValue { get; set; } = 10;

        [JsonProperty]
        [EditorModifiable("Bounding Type", "The logic this counter uses to constrain its value within the minimum and maximum values")]
        public NumericBoundMode BoundMode { get; set; } = NumericBoundMode.Clamp;

        private long _currentValue;
        [JsonProperty]
        [EditorModifiable("Start Value", "The starting value for the counter")]
        public long CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;

                if (_currentValue <= MinimumValue)
                {
                    FireEvent("OnMinimumReached");
                }
                else if (_currentValue >= MaximumValue)
                {
                    FireEvent("OnMaximumReached");
                }

                switch (BoundMode)
                {
                    case NumericBoundMode.Clamp:
                        _currentValue = Math.Clamp(value, MinimumValue, MaximumValue);
                        break;
                    case NumericBoundMode.Wrap:
                        _currentValue = WrapValue(value);
                        break;
                    case NumericBoundMode.Ignore:
                    default:
                        break;
                }
            }
        }

        private long WrapValue(long value)
        {
            return MathExt.Mod(value - MinimumValue, MaximumValue + 1 - MinimumValue) + MinimumValue;
        }

        [ActionMethod("Increment the value of the counter, applying the counter's bound logic and firing any relevant events")]
        [ActionMethodParameter("Value", "The value to add", typeof(long))]
        protected void IncrementValue(Entity sender, Dictionary<string, object?> parameters)
        {
            if (!parameters.TryGetValue("Value", out object? valueObj) || valueObj is not long value)
            {
                logger.LogError("Value parameter for IncrementValue action was not given or was of incorrect type" +
                    " (fired by \"{Source}\" to \"{Target}\")",
                    sender.Name, Name);
                return;
            }

            CurrentValue += value;
        }

        [ActionMethod("Decrement the value of the counter, applying the counter's bound logic and firing any relevant events")]
        [ActionMethodParameter("Value", "The value to subtract", typeof(long))]
        protected void DecrementValue(Entity sender, Dictionary<string, object?> parameters)
        {
            if (!parameters.TryGetValue("Value", out object? valueObj) || valueObj is not long value)
            {
                logger.LogError("Value parameter for DecrementValue action was not given or was of incorrect type" +
                    " (fired by \"{Source}\" to \"{Target}\")",
                    sender.Name, Name);
                return;
            }

            CurrentValue -= value;
        }
    }
}

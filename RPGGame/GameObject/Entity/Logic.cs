using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject.Entity
{
    [FiresEvent("OnTrue", "Fired when the logical operation of the gate is true")]
    public abstract class BooleanLogicBase(string name, Vector2 position, Vector2 size) : Entity(name, position, size)
    {
        private uint inputsThisFrame = 0;
        protected uint inputsLastFrame = 0;

        protected abstract bool LogicalOperation();

        protected override void TickLogic(GameTime gameTime)
        {
            base.TickLogic(gameTime);

            if (LogicalOperation())
            {
                FireEvent("OnTrue");
            }
        }

        public override void AfterTick(GameTime gameTime)
        {
            base.AfterTick(gameTime);

            inputsLastFrame = inputsThisFrame;
            inputsThisFrame = 0;
        }

        [ActionMethod("Input to the logic gate.")]
        protected void Input(Entity sender, Dictionary<string, object?> parameters)
        {
            inputsThisFrame++;
        }
    }

    [EditorEntity("ANDGate", "Fires an event when 2 or more inputs are received in a single frame", "Tool.Logic.Boolean")]
    public class ANDGate(string name, Vector2 position, Vector2 size) : BooleanLogicBase(name, position, size)
    {
        protected override bool LogicalOperation()
        {
            return inputsLastFrame >= 2;
        }
    }

    [EditorEntity("ORGate", "Fires an event when 1 or more inputs are received in a single frame", "Tool.Logic.Boolean")]
    public class ORGate(string name, Vector2 position, Vector2 size) : BooleanLogicBase(name, position, size)
    {
        protected override bool LogicalOperation()
        {
            return inputsLastFrame >= 1;
        }
    }

    [EditorEntity("XORGate", "Fires an event when exactly 1 input is received in a single frame", "Tool.Logic.Boolean")]
    public class XORGate(string name, Vector2 position, Vector2 size) : BooleanLogicBase(name, position, size)
    {
        protected override bool LogicalOperation()
        {
            return inputsLastFrame == 1;
        }
    }

    [FiresEvent("OnTrue", "Fired when the given comparison passes")]
    [FiresEvent("OnFalse", "Fired when the given comparison does not pass")]
    public abstract class ComparisonBase<T>(string name, Vector2 position, Vector2 size) : Entity(name, position, size)
    {
        [JsonProperty]
        [EditorModifiable("Compare To", "The value to compare to when using the comparison action")]
        public T? ComparisonValue { get; set; }

        [JsonProperty]
        [EditorModifiable("Comparison Type", "The type of comparison operation to perform")]
        public ComparisonType CompareMode { get; set; }

        protected abstract bool Equal(T value);
        protected abstract bool NotEqual(T value);
        protected abstract bool Greater(T value);
        protected abstract bool GreaterEqual(T value);
        protected abstract bool Less(T value);
        protected abstract bool LessEqual(T value);

        private bool Compare(T value)
        {
            return CompareMode switch
            {
                ComparisonType.Equal => Equal(value),
                ComparisonType.NotEqual => NotEqual(value),
                ComparisonType.Greater => Greater(value),
                ComparisonType.GreaterEqual => GreaterEqual(value),
                ComparisonType.Less => Less(value),
                ComparisonType.LessEqual => LessEqual(value),
                _ => false
            };
        }

        [ActionMethod("Run the configured comparison operation and fire the relevant event based on the result")]
        // C# doesn't support generic types in attributes, so this method needs to be overridden just to implement the below attribute
        // [ActionMethodParameter("Value", "The value to compare from", typeof(T))]
        protected virtual void RunComparison(Entity sender, Dictionary<string, object?> parameters)
        {
            if (!parameters.TryGetValue("Value", out object? valueObj) || valueObj is not T value)
            {
                logger.LogError("Value parameter for RunComparison action was not given or was of incorrect type" +
                    " (fired by \"{Source}\" to \"{Target}\")",
                    sender.Name, Name);
                return;
            }

            FireEvent(Compare(value) ? "OnTrue" : "OnFalse");
        }
    }

    [EditorEntity("IntCompare", "Compares two integer values, firing either a true or false event", "Tool.Logic.Comparison")]
    public class IntCompare(string name, Vector2 position, Vector2 size) : ComparisonBase<long>(name, position, size)
    {
        protected override bool Equal(long value)
        {
            return value == ComparisonValue;
        }

        protected override bool NotEqual(long value)
        {
            return value != ComparisonValue;
        }

        protected override bool Greater(long value)
        {
            return value > ComparisonValue;
        }

        protected override bool GreaterEqual(long value)
        {
            return value >= ComparisonValue;
        }

        protected override bool Less(long value)
        {
            return value < ComparisonValue;
        }

        protected override bool LessEqual(long value)
        {
            return value <= ComparisonValue;
        }

        [ActionMethodParameter("Value", "The value to compare from", typeof(long))]
        protected override void RunComparison(Entity sender, Dictionary<string, object?> parameters)
        {
            base.RunComparison(sender, parameters);
        }
    }

    [EditorEntity("FloatCompare", "Compares two floating point values, firing either a true or false event", "Tool.Logic.Comparison")]
    public class FloatCompare(string name, Vector2 position, Vector2 size) : ComparisonBase<double>(name, position, size)
    {
        protected override bool Equal(double value)
        {
            return value == ComparisonValue;
        }

        protected override bool NotEqual(double value)
        {
            return value != ComparisonValue;
        }

        protected override bool Greater(double value)
        {
            return value > ComparisonValue;
        }

        protected override bool GreaterEqual(double value)
        {
            return value >= ComparisonValue;
        }

        protected override bool Less(double value)
        {
            return value < ComparisonValue;
        }

        protected override bool LessEqual(double value)
        {
            return value <= ComparisonValue;
        }

        [ActionMethodParameter("Value", "The value to compare from", typeof(double))]
        protected override void RunComparison(Entity sender, Dictionary<string, object?> parameters)
        {
            base.RunComparison(sender, parameters);
        }
    }

    [EditorEntity("StringCompare", "Compares two string values, firing either a true or false event", "Tool.Logic.Comparison")]
    public class StringCompare(string name, Vector2 position, Vector2 size) : ComparisonBase<string>(name, position, size)
    {
        protected override bool Equal(string value)
        {
            return value == ComparisonValue;
        }

        protected override bool NotEqual(string value)
        {
            return value != ComparisonValue;
        }

        protected override bool Greater(string value)
        {
            logger.LogError("Only equal than or not equal to comparisons can be performed on strings.");
            return false;
        }

        protected override bool GreaterEqual(string value)
        {
            logger.LogError("Only equal than or not equal to comparisons can be performed on strings.");
            return false;
        }

        protected override bool Less(string value)
        {
            logger.LogError("Only equal than or not equal to comparisons can be performed on strings.");
            return false;
        }

        protected override bool LessEqual(string value)
        {
            logger.LogError("Only equal than or not equal to comparisons can be performed on strings.");
            return false;
        }

        [ActionMethodParameter("Value", "The value to compare from", typeof(string))]
        protected override void RunComparison(Entity sender, Dictionary<string, object?> parameters)
        {
            base.RunComparison(sender, parameters);
        }
    }
}

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject.Entity
{
    [EditorEntity("Relay", "Consolidates multiple Event->Action links into a single entity that can all be run with a single input", "Tool.Logic")]
    [FiresEvent("OnRelay", "Fired when the FireRelay action method is called")]
    public class Relay(string name, Vector2 position, Vector2 size) : Entity(name, position, size)
    {
        [ActionMethod("Fires the OnRelay event")]
        protected void FireRelay(Entity sender, Dictionary<string, object?> parameters)
        {
            FireEvent("OnRelay");
        }
    }

    [FiresEvent("OnTrue", "Fired when the RunLogic action method is called and the logical operation of the gate is true")]
    [FiresEvent("OnFalse", "Fired when the RunLogic action method is called and the logical operation of the gate is false")]
    public abstract class BooleanLogicBase(string name, Vector2 position, Vector2 size) : Entity(name, position, size)
    {
        protected bool inputOneState = false;

        protected abstract bool LogicalOperation();

        [ActionMethod("Set the state of the first input to the logic gate")]
        [ActionMethodParameter("State", "The state to set the first input to", typeof(bool))]
        protected void SetInputOne(Entity sender, Dictionary<string, object?> parameters)
        {
            if (!parameters.TryGetValue("State", out object? stateObj) || stateObj is not bool state)
            {
                logger.LogError("State parameter for SetInputOne action was not given or was of incorrect type" +
                    " (fired by \"{Source}\" to \"{Target}\")",
                    sender.Name, Name);
                return;
            }

            inputOneState = state;
        }

        [ActionMethod("Fire either OnTrue or OnFalse depending on the logical operation of the gate")]
        protected void RunLogic(Entity sender, Dictionary<string, object?> parameters)
        {
            FireEvent(LogicalOperation() ? "OnTrue" : "OnFalse");
        }
    }

    [EditorEntity("BufferGate", "Fires the OnTrue event when the input is true, otherwise OnFalse", "Tool.Logic.Boolean.OneInput")]
    public class BufferGate(string name, Vector2 position, Vector2 size) : BooleanLogicBase(name, position, size)
    {
        protected override bool LogicalOperation()
        {
            return inputOneState;
        }
    }

    [EditorEntity("NOTGate", "Fires the OnTrue event when the input is false, otherwise OnFalse", "Tool.Logic.Boolean.OneInput")]
    public class NOTGate(string name, Vector2 position, Vector2 size) : BooleanLogicBase(name, position, size)
    {
        protected override bool LogicalOperation()
        {
            return !inputOneState;
        }
    }

    public abstract class TwoInputBooleanLogicBase(string name, Vector2 position, Vector2 size) : BooleanLogicBase(name, position, size)
    {
        protected bool inputTwoState = false;

        [ActionMethod("Set the state of the second input to the logic gate")]
        [ActionMethodParameter("State", "The state to set the second input to", typeof(bool))]
        protected void SetInputTwo(Entity sender, Dictionary<string, object?> parameters)
        {
            if (!parameters.TryGetValue("State", out object? stateObj) || stateObj is not bool state)
            {
                logger.LogError("State parameter for SetInputTwo action was not given or was of incorrect type" +
                    " (fired by \"{Source}\" to \"{Target}\")",
                    sender.Name, Name);
                return;
            }

            inputTwoState = state;
        }
    }

    [EditorEntity("ANDGate", "Fires the OnTrue event when both inputs are true, otherwise OnFalse", "Tool.Logic.Boolean.TwoInput")]
    public class ANDGate(string name, Vector2 position, Vector2 size) : TwoInputBooleanLogicBase(name, position, size)
    {
        protected override bool LogicalOperation()
        {
            return inputOneState && inputTwoState;
        }
    }

    [EditorEntity("ORGate", "Fires the OnTrue event when either or both of the inputs are true, otherwise OnFalse", "Tool.Logic.Boolean.TwoInput")]
    public class ORGate(string name, Vector2 position, Vector2 size) : TwoInputBooleanLogicBase(name, position, size)
    {
        protected override bool LogicalOperation()
        {
            return inputOneState || inputTwoState;
        }
    }

    [EditorEntity("XORGate", "Fires the OnTrue event when only one of the inputs is true, otherwise OnFalse", "Tool.Logic.Boolean.TwoInput")]
    public class XORGate(string name, Vector2 position, Vector2 size) : TwoInputBooleanLogicBase(name, position, size)
    {
        protected override bool LogicalOperation()
        {
            return inputOneState ^ inputTwoState;
        }
    }

    [EditorEntity("NANDGate", "Fires the OnTrue event when zero or one of the inputs are true, otherwise OnFalse", "Tool.Logic.Boolean.TwoInput")]
    public class NANDGate(string name, Vector2 position, Vector2 size) : TwoInputBooleanLogicBase(name, position, size)
    {
        protected override bool LogicalOperation()
        {
            return !(inputOneState && inputTwoState);
        }
    }

    [EditorEntity("NORGate", "Fires the OnTrue event when neither inputs are true, otherwise OnFalse", "Tool.Logic.Boolean.TwoInput")]
    public class NORGate(string name, Vector2 position, Vector2 size) : TwoInputBooleanLogicBase(name, position, size)
    {
        protected override bool LogicalOperation()
        {
            return !(inputOneState || inputTwoState);
        }
    }

    [EditorEntity("XNORGate", "Fires the OnTrue event when none or both of the inputs is true, otherwise OnFalse", "Tool.Logic.Boolean.TwoInput")]
    public class XNORGate(string name, Vector2 position, Vector2 size) : TwoInputBooleanLogicBase(name, position, size)
    {
        protected override bool LogicalOperation()
        {
            return !(inputOneState ^ inputTwoState);
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

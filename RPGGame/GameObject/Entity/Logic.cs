using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RPGGame.GameObject.Entity
{
    [FiresEvent("OnTrue", "Fired when the logical operation of the gate is true")]
    public abstract class BooleanLogicBase(string name, Vector2 position, Vector2 size, string? texture) : Entity(name, position, size, texture)
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
    public class ANDGate(string name, Vector2 position, Vector2 size, string? texture) : BooleanLogicBase(name, position, size, texture)
    {
        protected override bool LogicalOperation()
        {
            return inputsLastFrame >= 2;
        }
    }

    [EditorEntity("ORGate", "Fires an event when 1 or more inputs are received in a single frame", "Tool.Logic.Boolean")]
    public class ORGate(string name, Vector2 position, Vector2 size, string? texture) : BooleanLogicBase(name, position, size, texture)
    {
        protected override bool LogicalOperation()
        {
            return inputsLastFrame >= 1;
        }
    }

    [EditorEntity("XORGate", "Fires an event when exactly 1 input is received in a single frame", "Tool.Logic.Boolean")]
    public class XORGate(string name, Vector2 position, Vector2 size, string? texture) : BooleanLogicBase(name, position, size, texture)
    {
        protected override bool LogicalOperation()
        {
            return inputsLastFrame == 1;
        }
    }
}

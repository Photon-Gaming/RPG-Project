using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace RPGGame.GameObject.Entity
{
    [FiresEvent("OnInputStart", "Fired every time the configured input starts")]
    [FiresEvent("OnInputEnd", "Fired every time the configured input ends")]
    public abstract class InputListenerBase<T>(string name, Vector2 position, Vector2 size, string? texture) : Entity(name, position, size, texture) where T : struct, Enum
    {
        [JsonProperty]
        [EditorModifiable("Input to listen for", "The specific input that this entity instance will listen for")]
        public T ConfiguredInput { get; set; }

        public override void Tick(GameTime gameTime)
        {
            base.Tick(gameTime);

            if (HasInputStarted())
            {
                FireEvent("OnInputStart");
            }
            else if (HasInputEnded())
            {
                FireEvent("OnInputEnd");
            }
        }

        public abstract bool HasInputStarted();

        public abstract bool HasInputEnded();
    }

    [EditorEntity("KeyboardListener", "Reads for input from a specific key on the keyboard", "Input.Global")]
    public class KeyboardListener(string name, Vector2 position, Vector2 size, string? texture) : InputListenerBase<Keys>(name, position, size, texture)
    {
        public override bool HasInputStarted()
        {
            return CurrentRoom?.ContainingWorld?.CurrentPlayer?.PlayerInput.GetKeyboardKeyPressed(ConfiguredInput) ?? false;
        }

        public override bool HasInputEnded()
        {
            return CurrentRoom?.ContainingWorld?.CurrentPlayer?.PlayerInput.GetKeyboardKeyReleased(ConfiguredInput) ?? false;
        }
    }

    [EditorEntity("MouseButtonListener", "Reads for input from a specific button on the mouse", "Input.Global")]
    public class MouseButtonListener(string name, Vector2 position, Vector2 size, string? texture) : InputListenerBase<MouseButton>(name, position, size, texture)
    {
        public override bool HasInputStarted()
        {
            return CurrentRoom?.ContainingWorld?.CurrentPlayer?.PlayerInput.GetMouseButtonPressed(ConfiguredInput) ?? false;
        }

        public override bool HasInputEnded()
        {
            return CurrentRoom?.ContainingWorld?.CurrentPlayer?.PlayerInput.GetMouseButtonReleased(ConfiguredInput) ?? false;
        }
    }
}

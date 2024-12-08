using Microsoft.Xna.Framework.Input;

namespace RPGGame
{
    public class Input
    {
        private KeyboardState oldKeyboardState;
        private KeyboardState newKeyboardState;

        private MouseState oldMouseState;
        private MouseState newMouseState;

        /// <summary>
        /// Should be called once per frame to process new player input.
        /// </summary>
        public void Update()
        {
            oldKeyboardState = newKeyboardState;
            newKeyboardState = Keyboard.GetState();

            oldMouseState = newMouseState;
            newMouseState = Mouse.GetState();
        }

        /// <summary>
        /// Read whether the given key on the keyboard is currently being held down.
        /// </summary>
        public bool GetKeyboardKeyDown(Keys key)
        {
            return newKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Read whether the given key on the keyboard is currently not being held down.
        /// </summary>
        public bool GetKeyboardKeyUp(Keys key)
        {
            return newKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Read whether the given key on the keyboard was just pressed down this frame.
        /// </summary>
        public bool GetKeyboardKeyPressed(Keys key)
        {
            return newKeyboardState.IsKeyDown(key) && oldKeyboardState.IsKeyUp(key);
        }


        /// <summary>
        /// Read whether the given key on the keyboard was just released from being pressed down this frame.
        /// </summary>
        public bool GetKeyboardKeyReleased(Keys key)
        {
            return newKeyboardState.IsKeyUp(key) && oldKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Read whether the given button on the mouse is currently being held down.
        /// </summary>
        public bool GetMouseButtonDown(MouseButton button)
        {
            return GetMouseButtonState(newMouseState, button) == ButtonState.Pressed;
        }

        /// <summary>
        /// Read whether the given button on the mouse is currently not being held down.
        /// </summary>
        public bool GetMouseButtonUp(MouseButton button)
        {
            return GetMouseButtonState(newMouseState, button) == ButtonState.Released;
        }

        /// <summary>
        /// Read whether the given button on the mouse was just pressed down this frame.
        /// </summary>
        public bool GetMouseButtonPressed(MouseButton button)
        {
            return GetMouseButtonState(newMouseState, button) == ButtonState.Pressed
                && GetMouseButtonState(oldMouseState, button) == ButtonState.Released;
        }


        /// <summary>
        /// Read whether the given button on the mouse was just released from being pressed down this frame.
        /// </summary>
        public bool GetMouseButtonReleased(MouseButton button)
        {
            return GetMouseButtonState(newMouseState, button) == ButtonState.Released
                && GetMouseButtonState(oldMouseState, button) == ButtonState.Pressed;
        }

        private static ButtonState GetMouseButtonState(MouseState state, MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => state.LeftButton,
                MouseButton.Right => state.RightButton,
                MouseButton.Middle => state.MiddleButton,
                MouseButton.X1 => state.XButton1,
                MouseButton.X2 => state.XButton2,
                _ => ButtonState.Released
            };
        }
    }
}

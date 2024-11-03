using Microsoft.Xna.Framework.Input;

namespace RPGGame
{
    public class Input
    {
        private KeyboardState oldState;
        private KeyboardState newState;

        /// <summary>
        /// Should be called once per frame to process new player input.
        /// </summary>
        public void Update()
        {
            oldState = newState;
            newState = Keyboard.GetState();
        }

        /// <summary>
        /// Read whether the given key is currently being held down.
        /// </summary>
        public bool GetKeyDown(Keys key)
        {
            return newState.IsKeyDown(key);
        }

        /// <summary>
        /// Read whether the given key is currently not being held down.
        /// </summary>
        public bool GetKeyUp(Keys key)
        {
            return newState.IsKeyUp(key);
        }

        /// <summary>
        /// Read whether the given key was just pressed down this frame.
        /// </summary>
        public bool GetKeyPressed(Keys key)
        {
            return newState.IsKeyDown(key) && oldState.IsKeyUp(key);
        }


        /// <summary>
        /// Read whether the given key was just released from being pressed down this frame.
        /// </summary>
        public bool GetKeyReleased(Keys key)
        {
            return newState.IsKeyUp(key) && oldState.IsKeyDown(key);
        }
    }
}

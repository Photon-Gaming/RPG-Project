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

        public bool GetKeyDown(Keys key)
        {
            return newState.IsKeyDown(key);
        }

        public bool GetKeyUp(Keys key)
        {
            return newState.IsKeyUp(key);
        }

        public bool GetKeyPressed(Keys key)
        {
            return newState.IsKeyDown(key) && oldState.IsKeyUp(key);
        }

        public bool GetKeyReleased(Keys key)
        {
            return newState.IsKeyUp(key) && oldState.IsKeyDown(key);
        }
    }
}

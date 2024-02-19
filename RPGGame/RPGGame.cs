using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RPGGame
{
    public class RPGGame : Game
    {
        public const string TextureFolder = "Textures";

        internal GraphicsDeviceManager Graphics;
        internal SpriteBatch SpriteBatch;

        private const string defaultWorldName = "default";

        private RPGContentLoader rpgContentLoader;
        private GameObject.World currentWorld;

        public RPGGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            rpgContentLoader = new RPGContentLoader(Content.RootDirectory);

            GameObject.WorldFile defaultWorldFile = rpgContentLoader.LoadWorldFile(defaultWorldName);
            GameObject.RoomFile defaultRoomFile = rpgContentLoader.LoadRoomFile(defaultWorldFile.DefaultRoomName);
            currentWorld = new GameObject.World(defaultWorldFile, new GameObject.Player(), new GameObject.Room(defaultRoomFile));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            SpriteBatch.Begin();

            ScreenDrawing.TileDrawing.DrawTile(this, Point.Zero, "black");

            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

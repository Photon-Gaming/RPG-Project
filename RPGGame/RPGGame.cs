using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RPGGame
{
    public class RPGGame : Game
    {
        public const string TextureFolder = "Textures";

        internal GraphicsDeviceManager? graphics;
        internal SpriteBatch? spriteBatch;

        private const string defaultWorldName = "default";

        private RPGContentLoader? rpgContentLoader;
        private GameObject.World? currentWorld;

        private readonly ScreenDrawing.TileDrawing tileDraw;
        private readonly ScreenDrawing.EntityDrawing entityDraw;

        public RPGGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            tileDraw = new ScreenDrawing.TileDrawing(this);
            entityDraw = new ScreenDrawing.EntityDrawing(this);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            rpgContentLoader = new RPGContentLoader(Content.RootDirectory);

            currentWorld = rpgContentLoader.LoadWorld(defaultWorldName);
            currentWorld.ChangePlayer(new GameObject.Entity.Player());
            currentWorld.ChangeRoom(rpgContentLoader.LoadRoom(currentWorld.DefaultRoomName));
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
            if (spriteBatch is null)
            {
                return;
            }

            GraphicsDevice.Clear(currentWorld?.CurrentRoom?.BackgroundColor ?? Color.Purple);

            if (currentWorld?.CurrentRoom is null)
            {
                GraphicsDevice.Clear(currentWorld?.CurrentRoom?.BackgroundColor ?? Color.Magenta);
                return;
            }

            // PointClamp = Use nearest neighbour scaling
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            Point tileGridOffset = tileDraw.DrawTileGridCentered(currentWorld.CurrentRoom.TileMap);

            foreach (GameObject.Entity.Entity entity in currentWorld.CurrentRoom.Entities)
            {
                _ = entityDraw.DrawEntityOnGrid(entity, tileGridOffset, tileDraw.TileSize);
            }

            if (currentWorld.CurrentPlayer is not null)
            {
                Rectangle playerScreenArea = entityDraw.DrawEntityOnGrid(currentWorld.CurrentPlayer, tileGridOffset, tileDraw.TileSize);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

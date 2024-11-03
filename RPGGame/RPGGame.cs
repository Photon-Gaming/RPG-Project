using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RPGGame
{
    public class RPGGame : Game
    {
        public const string TextureFolder = "Textures";

        internal static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(
            b => b.AddConsoleFormatter<CustomConsoleFormatter, ConsoleFormatterOptions>().AddConsole(options =>
            {
                options.FormatterName = "RPG";
            }));

        internal GraphicsDeviceManager? graphics;
        internal SpriteBatch? spriteBatch;

        private const string defaultWorldName = "default";

        private static readonly ILogger logger = loggerFactory.CreateLogger("Core");

        private RPGContentLoader? rpgContentLoader;
        private GameObject.World? currentWorld;

        private readonly ScreenDrawing.TileDrawing tileDraw;
        private readonly ScreenDrawing.EntityDrawing entityDraw;

        private readonly Input playerInput = new();

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
            logger.LogInformation("RPGGame Engine initialising. Default world name is \"{Name}\"", defaultWorldName);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            rpgContentLoader = new RPGContentLoader(Content.RootDirectory);

            currentWorld = rpgContentLoader.LoadWorld(defaultWorldName);
            currentWorld.ChangePlayer(new GameObject.Entity.Player(playerInput));
            currentWorld.ChangeRoom(rpgContentLoader.LoadRoom(currentWorld.DefaultRoomName));
        }

        protected override void Update(GameTime gameTime)
        {
            playerInput.Update();

            if (currentWorld?.CurrentRoom is null)
            {
                logger.LogError("No room loaded. Updates cannot be performed");
                return;
            }

            currentWorld.CurrentRoom.TickLoadedEntities(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (spriteBatch is null)
            {
                logger.LogCritical("Sprite batch is undefined. Render aborted");
                return;
            }

            if (currentWorld?.CurrentRoom is null)
            {
                logger.LogError("No room loaded, nothing to render");
                GraphicsDevice.Clear(Color.Magenta);
                return;
            }

            GraphicsDevice.Clear(currentWorld.CurrentRoom.BackgroundColor);

            // PointClamp = Use nearest neighbour scaling
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            Point tileGridOffset = tileDraw.DrawTileGridCentered(currentWorld.CurrentRoom.TileMap);

            foreach (GameObject.Entity.Entity entity in currentWorld.CurrentRoom.Entities.Where(e => e.Enabled))
            {
                logger.LogTrace("Rendering entity Entity \"{Name}\" at ({PosX}, {PosY})",
                    entity.Name, entity.Position.X, entity.Position.Y);
                _ = entityDraw.DrawEntityOnGrid(entity, tileGridOffset, tileDraw.TileSize);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

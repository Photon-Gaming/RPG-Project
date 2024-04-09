using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RPGGame.ScreenDrawing
{
    public class TileDrawing(RPGGame game)
    {
        public static readonly Point MinTileSize = new(1, 1);
        public static readonly Point MaxTileSize = new(100, 100);
        public static readonly string TileTextureFolder = Path.Join(RPGGame.TextureFolder, "Tiles");

        public Point TileSize { get; private set; } = new(32, 32);

        /// <summary>
        /// Draw a texture with a given name to the screen, with its top left corner at the specified position.
        /// </summary>
        /// <param name="screenPosition">The pixel position on the screen to draw the tile at</param>
        /// <param name="tileTextureName">The name of the texture. Should not include the path to the tile texture folder itself.</param>
        public void DrawTile(Point screenPosition, string tileTextureName)
        {
            Texture2D texture = game.Content.Load<Texture2D>(Path.Join(TileTextureFolder, tileTextureName));
            game.spriteBatch?.Draw(texture, new Rectangle(screenPosition, TileSize), Color.White);
        }

        /// <summary>
        /// Draw a grid of tiles to the screen, with (0, 0) located at a given offset.
        /// </summary>
        public void DrawTileGrid(Point screenOffset, GameObject.Tile[,] tileMap)
        {
            for (int x = 0; x < tileMap.GetLength(0); x++)
            {
                for (int y = 0; y < tileMap.GetLength(1); y++)
                {
                    Point gridPos = new(x, y);
                    DrawTile((gridPos * TileSize) + screenOffset, tileMap[x, y].Texture);
                }
            }
        }

        /// <summary>
        /// Draw a grid of tiles centered in the game window.
        /// </summary>
        /// <returns>
        /// The pixel position that (0, 0) is located at in screen space.
        /// This value may be beyond the boundaries of the screen if the grid does not fully fit on it.
        /// </returns>
        public Point DrawTileGridCentered(GameObject.Tile[,] tileMap)
        {
            Point gridSize = new(tileMap.GetLength(0), tileMap.GetLength(1));
            Point gridScreenSize = TileSize * gridSize;
            Point screenOffset = new((game.GraphicsDevice.Viewport.Width / 2) - (gridScreenSize.X / 2),
                (game.GraphicsDevice.Viewport.Height / 2) - (gridScreenSize.Y / 2));
            DrawTileGrid(screenOffset, tileMap);
            return screenOffset;
        }

        /// <summary>
        /// Set the pixel dimensions of drawn background tiles.
        /// </summary>
        /// <param name="relative">
        /// Whether <paramref name="desiredSize"/> should be treated as an absolute value or added to the existing tile size.
        /// </param>
        /// <returns>
        /// The new tile size. Will be different from <paramref name="desiredSize"/> if it was invalid.
        /// </returns>
        public Point SetTileSize(Point desiredSize, bool relative)
        {
            if (relative)
            {
                desiredSize += TileSize;
            }
            desiredSize = new Point(
                Math.Clamp(desiredSize.X, MinTileSize.X, MaxTileSize.X),
                Math.Clamp(desiredSize.Y, MinTileSize.Y, MaxTileSize.Y));
            TileSize = desiredSize;
            return desiredSize;
        }
    }
}

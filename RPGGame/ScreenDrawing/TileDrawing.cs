using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RPGGame.ScreenDrawing
{
    public class TileDrawing(RPGGame game)
    {
        public static readonly Point TileSize = new(32, 32);
        public static readonly string TileTextureFolder = Path.Join(RPGGame.TextureFolder, "Tiles");

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

        public void DrawTile(Point screenPosition, string tileTextureName)
        {
            Texture2D texture = game.Content.Load<Texture2D>(Path.Join(TileTextureFolder, tileTextureName));
            game.spriteBatch.Draw(texture, new Rectangle(screenPosition, TileSize), Color.White);
        }
    }
}

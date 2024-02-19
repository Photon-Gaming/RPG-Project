using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RPGGame.ScreenDrawing
{
    public static class TileDrawing
    {
        public static readonly Point TileSize = new(32, 32);
        public static readonly string TileTextureFolder = Path.Join(RPGGame.TextureFolder, "Tiles");

        public static void DrawTile(RPGGame game, Point screenPosition, string tileTextureName)
        {
            Texture2D texture = game.Content.Load<Texture2D>(Path.Join(TileTextureFolder, tileTextureName));
            game.SpriteBatch.Draw(texture, new Rectangle(screenPosition, TileSize), Color.White);
        }
    }
}

using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RPGGame.ScreenDrawing
{
    public class EntityDrawing(RPGGame game)
    {
        public static readonly string EntityTextureFolder = Path.Join(RPGGame.TextureFolder, "Entities");

        /// <summary>
        /// Draw a texture with a given name to the screen, with its top left corner at the specified position.
        /// </summary>
        /// <param name="screenArea">The pixel area on the screen to draw the entity at</param>
        /// <param name="entityTextureName">The name of the texture. Should not include the path to the entity texture folder itself.</param>
        public void DrawEntity(Rectangle screenArea, string entityTextureName)
        {
            Texture2D texture = game.Content.Load<Texture2D>(Path.Join(EntityTextureFolder, entityTextureName));
            game.spriteBatch?.Draw(texture, screenArea, Color.White);
        }

        /// <summary>
        /// Draw a given entity onto a tile grid that has already been drawn to the screen.
        /// </summary>
        /// <param name="tileGridOffset">The pixel position on the screen of (0, 0). May be outside the screen boundaries.</param>
        /// <param name="tileSize">The pixel dimensions of a single tile on the screen</param>
        /// <returns></returns>
        public Rectangle DrawEntityOnGrid(GameObject.Entity.Entity entity, Point tileGridOffset, Point tileSize)
        {
            Point entityScreenSize = new((int)(entity.Size.X * tileSize.X), (int)(entity.Size.Y * tileSize.Y));
            Point screenPosition = new(
                (int)(entity.TopLeft.X * tileSize.X + tileGridOffset.X),
                (int)(entity.TopLeft.Y * tileSize.Y + tileGridOffset.Y));
            Rectangle screenArea = new(screenPosition, entityScreenSize);

            if (entity.Texture is not null)
            {
                DrawEntity(screenArea, entity.Texture);
            }

            return screenArea;
        }
    }
}

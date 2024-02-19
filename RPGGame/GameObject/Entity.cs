using System;
using Microsoft.Xna.Framework;

namespace RPGGame.GameObject
{
    [Serializable]
    public record EntityFile(
        Vector2 Position,
        Vector2 Size
    );

    public class Entity
    {
        public EntityFile FileData { get; }

        public Vector2 Position { get; protected set; }
        public Vector2 Size { get; protected set; }

        public Entity(EntityFile entityFile)
        {
            FileData = entityFile;

            Position = FileData.Position;
            Size = FileData.Size;
        }
    }
}

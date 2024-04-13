using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Entity(Vector2 position, Vector2 size, string? texture) : ICloneable
    {
        [JsonProperty]
        public Vector2 Position { get; protected set; } = position;

        [JsonProperty]
        public Vector2 Size { get; protected set; } = size;

        [JsonProperty]
        public string? Texture { get; protected set; } = texture;

        // Entity origin is the bottom middle, tile origin is the top left
        public Vector2 TopLeft => new(Position.X - (Size.X / 2), Position.Y - Size.Y);
        public Vector2 BottomRight => new(TopLeft.X + Size.X, TopLeft.Y + Size.Y);

        public virtual bool Move(Vector2 targetPos, bool relative)
        {
            if (relative)
            {
                targetPos += Position;
            }

            if (targetPos.X < 0 || targetPos.Y < 0)
            {
                return false;
            }

            Position = targetPos;
            return true;
        }

        public bool Collides(Entity other)
        {
            return TopLeft.X < other.BottomRight.X
                && TopLeft.Y < other.BottomRight.Y
                && BottomRight.X > other.TopLeft.X
                && BottomRight.Y > other.TopLeft.Y;
        }

        public bool Collides(Vector2 position)
        {
            return position.X > TopLeft.X
                && position.Y > TopLeft.Y
                && position.X < BottomRight.X
                && position.Y < BottomRight.Y;
        }

        public bool IsOutOfBounds(Room room)
        {
            return TopLeft.X < 0
                || TopLeft.Y < 0
                || BottomRight.X >= room.TileMap.GetLength(0)
                || BottomRight.Y >= room.TileMap.GetLength(1);
        }

        public object Clone()
        {
            return new Entity(Position, Size, Texture);
        }
    }
}

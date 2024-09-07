using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject.Entity
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Entity(string name, Vector2 position, Vector2 size, string? texture) : ICloneable
    {
        [JsonProperty]
        [EditorModifiable("Name", "The unique name of this entity that other entities in this room will refer to it by")]
        public string Name { get; protected set; } = name;

        [JsonProperty]
        [EditorModifiable("Position", "The location of the entity within the room.", EditType.RoomCoordinate)]
        public Vector2 Position { get; protected set; } = position;

        [JsonProperty]
        [EditorModifiable("Size", "The size of the entity relative to the tile grid.")]
        public Vector2 Size { get; protected set; } = size;

        [JsonProperty]
        [EditorModifiable("Render Texture", "The optional name of the texture that the game will draw for this entity.", EditType.EntityTexture)]
        public string? Texture { get; protected set; } = texture;

        [JsonProperty]
        [EditorModifiable("Enabled?", "Whether or not this entity will be rendered and run its Tick function every frame.")]
        public bool Enabled { get; private set; } = true;

        // Entity origin is the bottom middle, tile origin is the top left
        public Vector2 TopLeft => new(Position.X - (Size.X / 2), Position.Y - Size.Y);
        public Vector2 BottomRight => new(TopLeft.X + Size.X, TopLeft.Y + Size.Y);

        /// <summary>
        /// Called every time the entity is loaded or enabled, for example when the player enters its room.
        /// </summary>
        public virtual void Init() { }

        /// <summary>
        /// Called every time the entity is unloaded or disabled, for example when the player leaves its room.
        /// </summary>
        public virtual void Destroy() { }

        /// <summary>
        /// Called every frame while the entity is loaded and enabled.
        /// </summary>
        public virtual void Tick(GameTime gameTime) { }

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

        public virtual object Clone()
        {
            return new Entity(Name, Position, Size, Texture);
        }

        public bool Collides(Entity other)
        {
            return !ReferenceEquals(this, other)  // Entity should not collide with itself
                && TopLeft.X < other.BottomRight.X
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
            // Room.IsOutOfBounds could be used for this, but we can reduce the number of comparisons needed
            // by only checking TopLeft against the lower bound and BottomRight against the upper bound.
            return TopLeft.X < 0
                || TopLeft.Y < 0
                || BottomRight.X >= room.TileMap.GetLength(0)
                || BottomRight.Y >= room.TileMap.GetLength(1);
        }

        public void Enable()
        {
            if (Enabled)
            {
                return;
            }
            Enabled = true;
            Init();
        }

        public void Disable()
        {
            if (!Enabled)
            {
                return;
            }
            Enabled = false;
            Destroy();
        }
    }
}

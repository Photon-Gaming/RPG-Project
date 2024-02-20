using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Entity(Vector2 position, Vector2 size)
    {
        [JsonProperty]
        public Vector2 Position { get; protected set; } = position;

        [JsonProperty]
        public Vector2 Size { get; protected set; } = size;

        public virtual bool Move(Vector2 targetPos, bool relative)
        {
            bool success = true;

            if (relative)
            {
                targetPos += Position;
            }

            if (targetPos.X > 0 && targetPos.Y > 0)
            {
                Position = targetPos;
            }
            else
            {
                success = false;
            }
           
            return success;
        }
    }
}

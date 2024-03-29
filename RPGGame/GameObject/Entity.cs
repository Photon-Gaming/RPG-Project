﻿using System;
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

        public object Clone()
        {
            return new Entity(Position, Size, Texture);
        }
    }
}

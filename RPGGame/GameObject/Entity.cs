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
    }
}

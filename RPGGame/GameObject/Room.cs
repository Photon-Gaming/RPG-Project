using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Room(Tile[,] tileMap, Entity[] entities, Color backgroundColor)
    {
        [JsonProperty]
        public Tile[,] TileMap { get; protected set; } = tileMap;
        [JsonProperty]
        public Entity[] Entities { get; protected set; } = entities;
        [JsonProperty]
        public Color BackgroundColor { get; protected set; } = backgroundColor;
    }
}

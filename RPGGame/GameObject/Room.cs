﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Room(Tile[,] tileMap, Entity[] entities, Color backgroundColor) : ICloneable
    {
        [JsonProperty]
        public Tile[,] TileMap { get; protected set; } = tileMap;
        [JsonProperty]
        public Entity[] Entities { get; protected set; } = entities;
        [JsonProperty]
        public Color BackgroundColor { get; set; } = backgroundColor;

        public object Clone()
        {
            return new Room((Tile[,])TileMap.Clone(), Entities.Select(e => (Entity)e.Clone()).ToArray(), BackgroundColor);
        }
    }
}

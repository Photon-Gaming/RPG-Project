using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Room(Tile[,] tileMap, List<Entity.Entity> entities, Color backgroundColor) : ICloneable
    {
        [JsonProperty]
        public Tile[,] TileMap { get; protected set; } = tileMap;
        [JsonProperty]
        public List<Entity.Entity> Entities { get; protected set; } = entities;
        [JsonProperty]
        public Color BackgroundColor { get; set; } = backgroundColor;

        public Dictionary<string, Entity.Entity> LoadedNamedEntities { get; protected set; } = new();

        public void OnLoad(World world)
        {
            foreach (Entity.Entity entity in Entities.Where(e => e.Enabled))
            {
                entity.Init();
            }

            LoadedNamedEntities = Entities.ToDictionary(e => e.Name, e => e, StringComparer.OrdinalIgnoreCase);
            if (world.CurrentPlayer is not null)
            {
                Entities.Add(world.CurrentPlayer);
                LoadedNamedEntities[Entity.Player.PlayerEntityName] = world.CurrentPlayer;
            }
        }

        public void OnUnload()
        {
            foreach (Entity.Entity entity in Entities.Where(e => e.Enabled))
            {
                entity.Destroy();
            }
        }

        public bool IsOutOfBounds(Vector2 position)
        {
            return position.X < 0
                || position.Y < 0
                || position.X >= TileMap.GetLength(0)
                || position.Y >= TileMap.GetLength(1);
        }

        public object Clone()
        {
            return new Room((Tile[,])TileMap.Clone(), Entities.Select(e => (Entity.Entity)e.Clone()).ToList(), BackgroundColor);
        }
    }
}

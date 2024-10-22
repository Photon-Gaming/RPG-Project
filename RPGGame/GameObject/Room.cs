using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
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

        private static readonly ILogger logger = RPGGame.loggerFactory.CreateLogger("Room");

        public void OnLoad(World world)
        {
            LoadedNamedEntities = Entities.ToDictionary(e => e.Name, e => e, StringComparer.OrdinalIgnoreCase);

            foreach (Entity.Entity entity in Entities.Where(entity => entity.Enabled))
            {
                logger.LogDebug("Loading Entity \"{Name}\"", entity.Name);
                entity.Init();
            }

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
                logger.LogDebug("Unloading Entity \"{Name}\"", entity.Name);
                entity.Destroy();
            }
        }

        public void TickLoadedEntities(GameTime gameTime)
        {
            foreach (Entity.Entity entity in Entities.Where(e => e.Enabled))
            {
                try
                {
                    logger.LogTrace("Ticking entity Entity \"{Name}\" at ({PosX}, {PosY})",
                        entity.Name, entity.Position.X, entity.Position.Y);
                    entity.Tick(gameTime);
                }
                catch (Exception exc)
                {
                    logger.LogCritical(exc, "Uncaught error in Tick function for Entity \"{Name}\" at ({PosX}, {PosY})",
                        entity.Name, entity.Position.X, entity.Position.Y);
                }
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

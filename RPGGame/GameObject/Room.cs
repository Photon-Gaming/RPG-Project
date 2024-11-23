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
        public Tile[,] TileMap { get; } = tileMap;
        [JsonProperty]
        public List<Entity.Entity> Entities { get; } = entities;
        [JsonProperty]
        public Color BackgroundColor { get; set; } = backgroundColor;

        public Dictionary<string, Entity.Entity> LoadedNamedEntities { get; private set; } = new();

        public World? ContainingWorld { get; private set; }

        private static readonly ILogger logger = RPGGame.loggerFactory.CreateLogger("Room");

        public void OnLoad(World world)
        {
            ContainingWorld = world;
            LoadedNamedEntities = Entities.ToDictionary(e => e.Name, e => e, StringComparer.OrdinalIgnoreCase);

            // Use ToArray so that entity Init methods can mutate the entity list while we're still iterating
            foreach (Entity.Entity entity in Entities.Where(entity => entity.Enabled).ToArray())
            {
                logger.LogDebug("Loading Entity \"{Name}\"", entity.Name);
                entity.Init();
            }

            if (world.CurrentPlayer is not null)
            {
                world.CurrentPlayer.CurrentRoom = this;
                Entities.Add(world.CurrentPlayer);
                LoadedNamedEntities[Entity.Player.PlayerEntityName] = world.CurrentPlayer;
                // If there are multiple enabled player spawn points, pick a random one to spawn the player at
                Entity.PlayerSpawn? spawnPoint = Entities.OfType<Entity.PlayerSpawn>().Where(e => e.Enabled)
                    .ChooseRandomOrDefault(null);
                if (spawnPoint is not null)
                {
                    world.CurrentPlayer.Move(spawnPoint.Position, false, true);
                    spawnPoint.EntitySpawned();
                }
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

        public void AddEntity(Entity.Entity entity)
        {
            if (LoadedNamedEntities.ContainsKey(entity.Name))
            {
                logger.LogWarning("An entity already exists with the name {Name}. The new entity will not be loaded.", entity.Name);
                return;
            }

            Entities.Add(entity);
            LoadedNamedEntities[entity.Name] = entity;
            entity.CurrentRoom = this;
            if (entity.Enabled)
            {
                logger.LogDebug("Loading Entity \"{Name}\"", entity.Name);
                entity.Init();
            }
        }

        public void RemoveEntity(Entity.Entity entity)
        {
            if (!Entities.Remove(entity) && !LoadedNamedEntities.Remove(entity.Name))
            {
                logger.LogWarning("Entity with name {Name} is not loaded but an attempt was made to unload it.", entity.Name);
                return;
            }

            if (entity.Enabled)
            {
                logger.LogDebug("Unloading Entity \"{Name}\"", entity.Name);
                entity.Destroy();
            }
        }

        public void TickLoadedEntities(GameTime gameTime)
        {
            Entity.Entity[] enabledEntities = Entities.Where(e => e.Enabled).ToArray();

            foreach (Entity.Entity entity in enabledEntities)
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

            // Each AfterTick method needs to run after every entity has had its Tick method executed
            foreach (Entity.Entity entity in enabledEntities)
            {
                try
                {
                    logger.LogTrace("After-Ticking entity Entity \"{Name}\" at ({PosX}, {PosY})",
                        entity.Name, entity.Position.X, entity.Position.Y);
                    entity.AfterTick(gameTime);
                }
                catch (Exception exc)
                {
                    logger.LogCritical(exc, "Uncaught error in AfterTick function for Entity \"{Name}\" at ({PosX}, {PosY})",
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
            return new Room((Tile[,])TileMap.Clone(), Entities.Select(e => e.Clone(e.Name)).ToList(), BackgroundColor);
        }
    }
}

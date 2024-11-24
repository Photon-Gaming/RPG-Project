using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject.Entity
{
    [EditorEntity("EntitySpawner", "Creates copies of a template entity", "Tool.Spawning")]
    public class EntitySpawner(string name, Vector2 position, Vector2 size, string? texture) : Entity(name, position, size, texture)
    {
        [JsonProperty]
        [EditorModifiable("Spawn Points", "A list of points that will be randomly chosen from to spawn new entities at", EditType.EntityLink)]
        public List<string> SpawnPoints { get; protected set; } = new();

        [JsonProperty]
        [EditorModifiable("Template Entity", "The entity to clone when creating new copies", EditType.EntityLink)]
        public string Template { get; set; } = "";

        [JsonProperty]
        [EditorModifiable("Name Template", "The base name to use for new entities")]
        public string BaseName { get; set; } = "";

        protected EntitySpawn[] spawnPointEntities = Array.Empty<EntitySpawn>();

        protected Entity? templateEntity = null;

        private ulong entitiesSpawned = 0;

        protected override void InitLogic()
        {
            base.InitLogic();

            // Convert list of names to list of entities
            spawnPointEntities = SpawnPoints.Select(t => CurrentRoom?.LoadedNamedEntities.GetValueOrDefault(t))
                .OfType<EntitySpawn>().ToArray();

            if (spawnPointEntities.Length != SpawnPoints.Count)
            {
                logger.LogWarning("EntitySpawner \"{Name}\" had {Count} spawn point entities which either could not be found or were not of type EntitySpawn.",
                    Name, SpawnPoints.Count - spawnPointEntities.Length);
            }

            templateEntity = CurrentRoom?.LoadedNamedEntities.GetValueOrDefault(Template);

            if (templateEntity is null)
            {
                logger.LogWarning("EntitySpawner \"{Name}\" will not function as the template entity could not be found", Name);
            }
            else
            {
                // Template entities shouldn't themselves appear in the room
                CurrentRoom?.RemoveEntity(templateEntity);
            }
        }

        [ActionMethod("Spawn a copy of the template entity at a randomly selected spawn point")]
        public void SpawnEntity(Entity sender, Dictionary<string, object?> parameters)
        {
            if (templateEntity is null || CurrentRoom is null)
            {
                return;
            }

            EntitySpawn? spawnPoint = spawnPointEntities.Where(e => e.Enabled).ChooseRandomOrDefault();

            if (spawnPoint is null)
            {
                logger.LogWarning("EntitySpawner \"{Name}\" failed to spawn entity as there were no active spawn points to choose from", Name);
                return;
            }

            Entity newEntity = templateEntity.Clone($"{BaseName}_{++entitiesSpawned}");
            newEntity.Move(spawnPoint.Position, false, true);

            CurrentRoom.AddEntity(newEntity);
            spawnPoint.EntitySpawned();
        }
    }
}

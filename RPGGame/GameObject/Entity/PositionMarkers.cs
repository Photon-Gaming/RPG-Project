using Microsoft.Xna.Framework;

namespace RPGGame.GameObject.Entity
{
    [EditorEntity("EntitySpawn", "Marks the position where an entity can be spawned by an EntitySpawner.", "Spawning.Positioning")]
    [FiresEvent("OnSpawn", "Fired when an entity spawns at this spawn point")]
    public class EntitySpawn(string name, Vector2 position, Vector2 size, string? texture) : Entity(name, position, size, texture)
    {
        internal void EntitySpawned()
        {
            FireEvent("OnSpawn");
        }
    }

    [EditorEntity("PlayerSpawn", "Marks the position where the player should spawn within the room.", "Spawning.Positioning")]
    public class PlayerSpawn(string name, Vector2 position, Vector2 size, string? texture) : EntitySpawn(name, position, size, texture);
}

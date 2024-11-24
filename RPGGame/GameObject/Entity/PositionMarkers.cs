using Microsoft.Xna.Framework;

namespace RPGGame.GameObject.Entity
{
    [EditorEntity("EntitySpawn", "Marks the position where an entity can be spawned by an EntitySpawner.", "Tool.Spawning.Positioning")]
    [FiresEvent("OnSpawn", "Fired when an entity spawns at this spawn point")]
    public class EntitySpawn(string name, Vector2 position, Vector2 size) : Entity(name, position, size)
    {
        internal void EntitySpawned()
        {
            FireEvent("OnSpawn");
        }
    }

    [EditorEntity("PlayerSpawn", "Marks the position where the player should spawn within the room.", "Tool.Spawning.Positioning")]
    public class PlayerSpawn(string name, Vector2 position, Vector2 size) : EntitySpawn(name, position, size);
}

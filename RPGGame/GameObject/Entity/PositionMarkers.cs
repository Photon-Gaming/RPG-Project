using Microsoft.Xna.Framework;

namespace RPGGame.GameObject.Entity
{
    [EditorEntity("PlayerSpawn", "Marks the position where the player should spawn within the room.", "Positioning.Player")]
    public class PlayerSpawn(string name, Vector2 position, Vector2 size, string? texture) : Entity(name, position, size, texture);
}

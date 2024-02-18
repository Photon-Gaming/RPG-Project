using Microsoft.Xna.Framework;

namespace RPGGame.GameObject
{
    public class Player(Room currentRoom) : Entity(new EntityFile(Vector2.Zero, Vector2.One))
    {
        public Room CurrentRoom { get; private set; } = currentRoom;
    }
}

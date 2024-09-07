using Microsoft.Xna.Framework;

namespace RPGGame.GameObject.Entity
{
    public class Player() : Entity(PlayerEntityName, new Vector2(0.5f, 1), Vector2.One, PlayerTexture)
    {
        public const string PlayerEntityName = "Player";
        public const string PlayerTexture = "player";
    };
}

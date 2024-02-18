using System;

namespace RPGGame.GameObject
{
    [Serializable]
    public class Tile
    {
        public string Texture { get; set; } = "";
        public bool IsCollision { get; set; } = false;
    }
}

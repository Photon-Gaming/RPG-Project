using System;

namespace RPGGame.GameObject
{
    [Serializable]
    public readonly record struct Tile(
        string Texture,
        bool IsCollision
    );
}

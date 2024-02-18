using System;
using Microsoft.Xna.Framework;

namespace RPGGame.GameObject
{
    [Serializable]
    public record RoomFile(
        string[,] TileMap,
        bool[,] CollisionMap,
        Point Size
    );

    public class Room(RoomFile roomFile)
    {
        public RoomFile FileData { get; } = roomFile;
    }
}

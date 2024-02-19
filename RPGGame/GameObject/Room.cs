using System;

namespace RPGGame.GameObject
{
    [Serializable]
    public record RoomFile(
        Tile[,] TileMap,
        Entity[] Entities
    );

    public class Room(RoomFile roomFile)
    {
        public RoomFile FileData { get; } = roomFile;
    }
}

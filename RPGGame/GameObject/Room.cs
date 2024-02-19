using System;
using Microsoft.Xna.Framework;

namespace RPGGame.GameObject
{
    [Serializable]
    public record RoomFile(
        Tile[,] TileMap,
        Entity[] Entities,
        Color BackgroundColor
    );

    public class Room
    {
        public RoomFile FileData { get; }

        public Color BackgroundColor { get; protected set; }

        public Room(RoomFile roomFile)
        {
            FileData = roomFile;

            BackgroundColor = FileData.BackgroundColor;
        }
    }
}

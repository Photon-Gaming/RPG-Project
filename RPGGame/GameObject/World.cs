using System;
using Microsoft.Xna.Framework;

namespace RPGGame.GameObject
{
    [Serializable]
    public record WorldFile(
        (string RoomName, Vector2 MapPosition)[] RoomPositions,
        string DefaultRoomName
    );

    public class World(WorldFile worldFile)
    {
        public WorldFile FileData { get; } = worldFile;
    }
}

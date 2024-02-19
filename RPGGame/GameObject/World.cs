using System;

namespace RPGGame.GameObject
{
    [Serializable]
    public record WorldFile(
        string DefaultRoomName
    );

    public class World(WorldFile worldFile, Player defaultPlayer, Room defaultRoom)
    {
        public Player CurrentPlayer { get; } = defaultPlayer;

        public Room CurrentRoom { get; private set; } = defaultRoom;

        public WorldFile FileData { get; } = worldFile;
    }
}

using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RPGGame.GameObject
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class World(string defaultRoomName)
    {
        [JsonProperty]
        public string DefaultRoomName { get; } = defaultRoomName;

        public Entity.Player? CurrentPlayer { get; protected set; }

        public Room? CurrentRoom { get; protected set; }

        private static readonly ILogger logger = RPGGame.loggerFactory.CreateLogger("World");

        public void ChangePlayer(Entity.Player player)
        {
            logger.LogInformation("Player entity is changing to \"{Name}\"", player.Name);

            CurrentPlayer = player;
        }

        public void ChangeRoom(Room room)
        {
            logger.LogInformation("Loaded room is changing");

            CurrentRoom?.OnUnload();
            CurrentRoom = room;
            CurrentRoom.OnLoad(this);
        }
    }
}

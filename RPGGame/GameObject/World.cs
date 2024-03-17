using System;
using Newtonsoft.Json;

namespace RPGGame.GameObject
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class World(string defaultRoomName)
    {
        [JsonProperty]
        public string DefaultRoomName { get; } = defaultRoomName;

        public Player? CurrentPlayer { get; protected set; }

        public Room? CurrentRoom { get; protected set; }

        public void ChangePlayer(Player player)
        {
            CurrentPlayer = player;
        }

        public void ChangeRoom(Room room)
        {
            CurrentRoom = room;
        }
    }
}

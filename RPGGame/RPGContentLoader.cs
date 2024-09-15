using System;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Logging;

namespace RPGGame
{
    public class RPGContentLoader(string contentFolderPath)
    {
        public string ContentFolderPath { get; } = contentFolderPath;

        private string worldFolder => Path.Join(ContentFolderPath, "Worlds");
        private string roomFolder => Path.Join(ContentFolderPath, "Rooms");

        private static readonly ILogger logger = RPGGame.loggerFactory.CreateLogger("ContentLoader");

        public GameObject.World LoadWorld(string worldName)
        {
            logger.LogInformation("Loading world \"{World}\"", worldName);

            string worldFilePath = Path.Join(worldFolder, worldName);
            worldFilePath = Path.ChangeExtension(worldFilePath, "json");

            if (!File.Exists(worldFilePath))
            {
                logger.LogCritical("World file at \"{Path}\" does not exist", worldFilePath);
                throw new FileNotFoundException();
            }

            try
            {
                GameObject.World? world = JsonConvert.DeserializeObject<GameObject.World>(File.ReadAllText(worldFilePath));
                if (world is null)
                {
                    throw new JsonException();
                }
                return world;
            }
            catch (Exception exc)
            {
                logger.LogCritical(exc, "Unexpected error deserializing world file at \"{Path}\"", worldFilePath);
                throw;
            }
        }

        public GameObject.Room LoadRoom(string roomName, bool nameIsPath = false)
        {
            logger.LogInformation("Loading room \"{Room}\"", roomName);

            string roomFilePath = nameIsPath ? roomName : Path.ChangeExtension(Path.Join(roomFolder, roomName), "json");

            if (!File.Exists(roomFilePath))
            {
                logger.LogCritical("Room file at \"{Path}\" does not exist", roomFilePath);
                throw new FileNotFoundException();
            }

            try
            {
                GameObject.Room? room = JsonConvert.DeserializeObject<GameObject.Room>(File.ReadAllText(roomFilePath));
                if (room is null)
                {
                    throw new JsonException();
                }
                foreach (GameObject.Entity.Entity entity in room.Entities)
                {
                    entity.CurrentRoom = room;
                }
                return room;
            }
            catch (Exception exc)
            {
                logger.LogCritical(exc, "Unexpected error deserializing room file at \"{Path}\"", roomFilePath);
                throw;
            }
        }
    }
}

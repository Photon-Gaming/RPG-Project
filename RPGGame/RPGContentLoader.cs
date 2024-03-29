﻿using Newtonsoft.Json;
using System.IO;

namespace RPGGame
{
    public class RPGContentLoader(string contentFolderPath)
    {
        public string ContentFolderPath { get; } = contentFolderPath;

        private string worldFolder => Path.Join(ContentFolderPath, "Worlds");
        private string roomFolder => Path.Join(ContentFolderPath, "Rooms");

        public GameObject.World LoadWorld(string worldName)
        {
            string worldFilePath = Path.Join(worldFolder, worldName);
            worldFilePath = Path.ChangeExtension(worldFilePath, "json");

            if (!File.Exists(worldFilePath))
            {
                throw new FileNotFoundException();
            }

            return JsonConvert.DeserializeObject<GameObject.World>(File.ReadAllText(worldFilePath))
                ?? throw new JsonException();
        }

        public GameObject.Room LoadRoom(string roomName)
        {
            string roomFilePath = Path.Join(roomFolder, roomName);
            roomFilePath = Path.ChangeExtension(roomFilePath, "json");

            if (!File.Exists(roomFilePath))
            {
                throw new FileNotFoundException();
            }

            return JsonConvert.DeserializeObject<GameObject.Room>(File.ReadAllText(roomFilePath))
                ?? throw new JsonException();
        }
    }
}

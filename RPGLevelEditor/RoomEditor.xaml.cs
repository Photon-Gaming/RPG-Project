using System.IO;
using System.Windows;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGLevelEditor
{
    /// <summary>
    /// Interaction logic for RoomEditor.xaml
    /// </summary>
    public partial class RoomEditor : Window
    {
        public string RoomPath { get; }
        public RPGGame.GameObject.Room OpenRoom { get; }

        public RoomEditor(string roomPath, Window parent)
        {
            InitializeComponent();

            RoomPath = roomPath;
            Owner = parent;

            if (File.Exists(RoomPath))
            {
                try
                {
                    RPGGame.GameObject.Room? deserialized = JsonConvert.DeserializeObject<RPGGame.GameObject.Room>(File.ReadAllText(RoomPath));
                    if (deserialized is not null)
                    {
                        OpenRoom = deserialized;
                    }
                }
                catch (Exception exc)
                {
                    _ = MessageBox.Show(this,
                        $"An error occured loading the specified room file. A new room will be created at the specified path if you save." +
                        $"\n\n{exc.GetType().Name}: {exc.Message}",
                        "Room Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            OpenRoom ??= new RPGGame.GameObject.Room(new RPGGame.GameObject.Tile[0, 0], Array.Empty<RPGGame.GameObject.Entity>(), Color.CornflowerBlue);

            Title += " - " + RoomPath;
        }
    }
}

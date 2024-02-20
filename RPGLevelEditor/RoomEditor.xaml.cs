using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Color = Microsoft.Xna.Framework.Color;

namespace RPGLevelEditor
{
    /// <summary>
    /// Interaction logic for RoomEditor.xaml
    /// </summary>
    public partial class RoomEditor : Window
    {
        public const string TileTextureFolder = "Tiles";

        public MainWindow ParentWindow { get; }
        public string RoomPath { get; }
        public RPGGame.GameObject.Room OpenRoom { get; }

        public Microsoft.Xna.Framework.Point TileSize { get; set; } = new(32, 32);

        public RoomEditor(string roomPath, MainWindow parent)
        {
            InitializeComponent();

            RoomPath = roomPath;
            Owner = parent;
            ParentWindow = parent;

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

            RefreshTileGrid();
        }

        public void RefreshTileGrid()
        {
            tileMapGrid.Background = new SolidColorBrush(new System.Windows.Media.Color()
            {
                R = OpenRoom.BackgroundColor.R,
                G = OpenRoom.BackgroundColor.G,
                B = OpenRoom.BackgroundColor.B,
                A = OpenRoom.BackgroundColor.A
            });

            tileGridDisplay.Children.Clear();

            for (int x = 0; x < OpenRoom.TileMap.GetLength(0); x++)
            {
                for (int y = 0; y < OpenRoom.TileMap.GetLength(1); y++)
                {
                    Microsoft.Xna.Framework.Point gridPos = new(x, y);
                    gridPos *= TileSize;
                    string texturePath = Path.Join(
                        ParentWindow.EditorConfig.TextureFolderPath, TileTextureFolder, OpenRoom.TileMap[x, y].Texture);
                    texturePath = Path.ChangeExtension(texturePath, "png");

                    if (!File.Exists(texturePath))
                    {
                        texturePath = "pack://application:,,,/Resources/placeholder.png";
                    }

                    _ = tileGridDisplay.Children.Add(new Image()
                    {
                        Margin = new Thickness(gridPos.X, gridPos.Y, 0, 0),
                        Source = new BitmapImage(new Uri(texturePath)),
                        Width = TileSize.X,
                        Height = TileSize.Y,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Stretch = Stretch.Fill
                    });
                }
            }
        }
    }
}

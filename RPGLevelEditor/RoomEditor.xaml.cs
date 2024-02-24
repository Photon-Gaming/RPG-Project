using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace RPGLevelEditor
{
    /// <summary>
    /// Interaction logic for RoomEditor.xaml
    /// </summary>
    public partial class RoomEditor : Window
    {
        public const string TileTextureFolderName = "Tiles";
        public readonly string TileTextureFolderPath;

        public MainWindow ParentWindow { get; }
        public string RoomPath { get; }
        public RPGGame.GameObject.Room OpenRoom { get; private set; }

        public Point TileSize { get; set; } = new(32, 32);
        public string? SelectedTextureName { get; set; }

        private bool _unsavedChanges = false;
        public bool UnsavedChanges
        {
            get => _unsavedChanges;
            set
            {
                _unsavedChanges = value;
                unsavedChangesLabel.Content = _unsavedChanges ? "UNSAVED" : "";
            }
        }

        private readonly Stack<RPGGame.GameObject.Room> undoStack = new();
        private readonly Stack<RPGGame.GameObject.Room> redoStack = new();

        public RoomEditor(string roomPath, MainWindow parent, bool forceCreateNew = false)
        {
            InitializeComponent();

            RoomPath = roomPath;
            Owner = parent;
            ParentWindow = parent;

            TileTextureFolderPath = Path.Join(ParentWindow.EditorConfig.ContentFolderPath,
                MainWindow.TextureFolderName, TileTextureFolderName);
            if (!Directory.Exists(TileTextureFolderPath))
            {
                _ = MessageBox.Show(this,
                    @"The specified Content folder does not contain a Textures\Tiles folder. " +
                    "Please make sure you have configured the correct folder path in the main window.",
                    "Room Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (!forceCreateNew && File.Exists(RoomPath))
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
            OpenRoom ??= new RPGGame.GameObject.Room(new RPGGame.GameObject.Tile[0, 0],
                Array.Empty<RPGGame.GameObject.Entity>(),
                Microsoft.Xna.Framework.Color.CornflowerBlue);

            UnsavedChanges = forceCreateNew;

            Title += " - " + RoomPath;

            UpdateTextureSelectionPanel();
            RefreshTileGrid();
        }

        public void RefreshTileGrid()
        {
            undoItem.IsEnabled = undoStack.Count > 0;
            redoItem.IsEnabled = redoStack.Count > 0;

            tileMapScroll.Background = new SolidColorBrush(new Color()
            {
                R = OpenRoom.BackgroundColor.R,
                G = OpenRoom.BackgroundColor.G,
                B = OpenRoom.BackgroundColor.B,
                A = OpenRoom.BackgroundColor.A
            });

            tileGridDisplay.Children.Clear();

            int xSize = OpenRoom.TileMap.GetLength(0);
            int ySize = OpenRoom.TileMap.GetLength(1);
            tileGridDisplay.Width = xSize * TileSize.X;
            tileGridDisplay.Height = ySize * TileSize.Y;

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    Point gridPos = new(x, y);
                    string texturePath = Path.Join(TileTextureFolderPath, OpenRoom.TileMap[x, y].Texture);
                    texturePath = Path.ChangeExtension(texturePath, "png");

                    if (!File.Exists(texturePath))
                    {
                        texturePath = "pack://application:,,,/Resources/placeholder.png";
                    }

                    Image newElement = new()
                    {
                        Margin = new Thickness(gridPos.X * TileSize.X, gridPos.Y * TileSize.Y, 0, 0),
                        Source = new BitmapImage(new Uri(texturePath)),
                        Width = TileSize.X,
                        Height = TileSize.Y,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Stretch = Stretch.Fill,
                        SnapsToDevicePixels = true,
                        Tag = gridPos
                    };
                    RenderOptions.SetBitmapScalingMode(newElement, BitmapScalingMode.NearestNeighbor);
                    newElement.MouseDown += GridSquare_MouseDown;
                    newElement.MouseEnter += GridSquare_MouseEnter;
                    _ = tileGridDisplay.Children.Add(newElement);
                }
            }

            if (gridOverlayItem.IsChecked)
            {
                for (int x = 1; x < xSize; x++)
                {
                    _ = tileGridDisplay.Children.Add(new System.Windows.Shapes.Rectangle()
                    {
                        Height = tileGridDisplay.Height,
                        Width = 3,
                        Margin = new Thickness(x * TileSize.X - 1, 0, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Fill = Brushes.DarkGoldenrod
                    });
                }

                for (int y = 1; y < ySize; y++)
                {
                    _ = tileGridDisplay.Children.Add(new System.Windows.Shapes.Rectangle()
                    {
                        Height = 3,
                        Width = tileGridDisplay.Width,
                        Margin = new Thickness(0, y * TileSize.Y - 1, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        Fill = Brushes.DarkGoldenrod
                    });
                }
            }
        }

        public void UpdateTextureSelectionPanel()
        {
            textureSelectPanel.Children.Clear();

            if (!Directory.Exists(TileTextureFolderPath))
            {
                return;
            }

            foreach (string texturePath in Directory.EnumerateFiles(TileTextureFolderPath, "*.png"))
            {
                string textureName = Path.GetFileNameWithoutExtension(texturePath);

                Border newElement = new()
                {
                    Margin = new Thickness(3),
                    Width = 32,
                    Height = 32,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderBrush = Brushes.OrangeRed,
                    BorderThickness = new Thickness(textureName == SelectedTextureName ? 2 : 0),
                    Child = new Image()
                    {
                        Source = new BitmapImage(new Uri(texturePath)),
                        Stretch = Stretch.Fill
                    },
                    ToolTip = textureName,
                    Tag = textureName
                };
                newElement.MouseUp += TextureSelect_MouseUp;
                _ = textureSelectPanel.Children.Add(newElement);
            }
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(RoomPath, JsonConvert.SerializeObject(OpenRoom, Formatting.Indented));
                UnsavedChanges = false;
            }
            catch (Exception exc)
            {
                _ = MessageBox.Show(this,
                    $"An error occured saving the room file. Your changes have not been saved." +
                    $"\n\n{exc.GetType().Name}: {exc.Message}",
                    "Room Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool Undo()
        {
            if (undoStack.TryPop(out RPGGame.GameObject.Room? previousRoom))
            {
                redoStack.Push((RPGGame.GameObject.Room)OpenRoom.Clone());
                OpenRoom = previousRoom;
                RefreshTileGrid();
                return true;
            }
            return false;
        }

        public bool Redo()
        {
            if (redoStack.TryPop(out RPGGame.GameObject.Room? previousRoom))
            {
                undoStack.Push((RPGGame.GameObject.Room)OpenRoom.Clone());
                OpenRoom = previousRoom;
                RefreshTileGrid();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Change the size of the currently open room.
        /// </summary>
        /// <remarks>
        /// If the target dimensions are smaller than the current dimensions, out of bounds tiles and entities will be deleted.
        /// </remarks>
        public void ChangeDimensions(int xSize, int ySize)
        {
            PushUndoStack();

            int oldXSize = OpenRoom.TileMap.GetLength(0);
            int oldYSize = OpenRoom.TileMap.GetLength(1);
            RPGGame.GameObject.Tile[,] newTileMap = new RPGGame.GameObject.Tile[xSize, ySize];

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    newTileMap[x, y] = x < oldXSize && y < oldYSize
                        ? OpenRoom.TileMap[x, y]
                        : new RPGGame.GameObject.Tile(SelectedTextureName ?? "", false);
                }
            }

            OpenRoom = new RPGGame.GameObject.Room(newTileMap,
                OpenRoom.Entities.Where(e => e.Position.X < xSize && e.Position.Y < ySize)
                    .Select(e => (RPGGame.GameObject.Entity)e.Clone()).ToArray(),
                OpenRoom.BackgroundColor);

            RefreshTileGrid();
        }

        private void PushUndoStack()
        {
            UnsavedChanges = true;
            redoStack.Clear();
            undoStack.Push((RPGGame.GameObject.Room)OpenRoom.Clone());
        }

        private void ReplaceTileAtPosition(Point position)
        {
            if (SelectedTextureName is null)
            {
                return;
            }

            int x = (int)position.X;
            int y = (int)position.Y;

            if (SelectedTextureName == OpenRoom.TileMap[x, y].Texture)
            {
                // Nothing to change
                return;
            }

            PushUndoStack();

            OpenRoom.TileMap[x, y] = OpenRoom.TileMap[x, y] with { Texture = SelectedTextureName };

            RefreshTileGrid();
        }

        private void TextureSelect_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectedTextureName = (sender as Border)?.Tag as string;
            UpdateTextureSelectionPanel();
        }

        private void tileMapScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Prevent zoom from also scrolling
                e.Handled = true;

                // Logarithmic zooming makes zoom look more "linear" to the eye
                double newX = Math.Exp(Math.Log(TileSize.X) + (e.Delta * 0.0007));
                double newY = Math.Exp(Math.Log(TileSize.Y) + (e.Delta * 0.0007));
                if (TileSize.X + newX > 0 && TileSize.Y + newY > 0)
                {
                    TileSize = new Point(newX, newY);
                    RefreshTileGrid();
                }
            }
        }

        private void GridSquare_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e is { ChangedButton: MouseButton.Left, ButtonState: MouseButtonState.Pressed }
                && sender is FrameworkElement { Tag: Point position })
            {
                ReplaceTileAtPosition(position);
            }
        }

        private void GridSquare_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e is { LeftButton: MouseButtonState.Pressed }
                && sender is FrameworkElement { Tag: Point position })
            {
                ReplaceTileAtPosition(position);
            }
        }

        private void tileMapScroll_MouseMove(object sender, MouseEventArgs e)
        {
            Point relativeMousePos = e.GetPosition(tileGridDisplay);
            relativeMousePos = new Point(relativeMousePos.X / TileSize.X, relativeMousePos.Y / TileSize.Y);

            mousePositionLabel.Content = $"Mouse: ({relativeMousePos.X:N2}, {relativeMousePos.Y:N2})";
            gridPositionLabel.Content = $"Grid: ({(int)relativeMousePos.X:N0}, {(int)relativeMousePos.Y:N0})";

            if (relativeMousePos.X >= 0 && relativeMousePos.X < OpenRoom.TileMap.GetLength(0)
                && relativeMousePos.Y >= 0 && relativeMousePos.Y < OpenRoom.TileMap.GetLength(1))
            {
                mouseTextureLabel.Content = $"Texture: {OpenRoom.TileMap[(int)relativeMousePos.X, (int)relativeMousePos.Y].Texture}";
                mouseCollisionLabel.Content = $"Collision: {OpenRoom.TileMap[(int)relativeMousePos.X, (int)relativeMousePos.Y].IsCollision}";
            }
            else
            {
                mouseTextureLabel.Content = "Texture: N/A";
                mouseCollisionLabel.Content = "Collision: N/A";
            }
        }

        private void SaveItem_OnClick(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void undoItem_OnClick(object sender, RoutedEventArgs e)
        {
            _ = Undo();
        }

        private void redoItem_OnClick(object sender, RoutedEventArgs e)
        {
            _ = Redo();
        }

        private void gridOverlayItem_OnClick(object sender, RoutedEventArgs e)
        {
            RefreshTileGrid();
        }

        private void DimensionsItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedTextureName is null)
            {
                _ = MessageBox.Show(this, "Please select a texture before changing room dimensions",
                    "No texture", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int xLength = OpenRoom.TileMap.GetLength(0);
            int yLength = OpenRoom.TileMap.GetLength(1);

            ToolWindows.DimensionsDialog dialog = new(xLength, yLength);
            if (dialog.ShowDialog() ?? false)
            {
                if (dialog.X < xLength || dialog.Y < yLength)
                {
                    MessageBoxResult result = MessageBox.Show(this, 
                        "The entered dimensions are smaller than the current dimensions. " +
                        "Shrinking the room will cause tiles and entities outside the new boundaries to be lost.\n\n" +
                        "Are you sure you want to continue?",
                        "Potential loss", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                ChangeDimensions(dialog.X, dialog.Y);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (UnsavedChanges)
            {
                MessageBoxResult result =  MessageBox.Show(this, "You have unsaved changes. Would you like to save them before closing?",
                    "Unsaved changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
                if (result == MessageBoxResult.Cancel)
                {
                    // Stop window from closing if cancel is pressed
                    e.Cancel = true;
                    return;
                }
                if (result == MessageBoxResult.Yes)
                {
                    Save();
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Z when e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control):
                    if (!Undo())
                    {
                        SystemSounds.Exclamation.Play();
                    }
                    break;
                case Key.Y when e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control):
                    if (!Redo())
                    {
                        SystemSounds.Exclamation.Play();
                    }
                    break;
            }
        }
    }
}

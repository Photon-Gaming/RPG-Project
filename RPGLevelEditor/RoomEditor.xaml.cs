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
        public readonly struct StateStackFrame(int x, int y, RPGGame.GameObject.Tile tile)
        {
            public readonly int X = x;
            public readonly int Y = y;
            public readonly RPGGame.GameObject.Tile Tile = tile;
        }

        public const string TileTextureFolderName = "Tiles";
        public readonly string TileTextureFolderPath;

        public static readonly Point TileSize = new(32, 32);

        public MainWindow ParentWindow { get; }
        public string RoomPath { get; }
        public RPGGame.GameObject.Room OpenRoom { get; private set; }

        public string? SelectedTextureName { get; private set; }

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

        private static readonly BitmapImage collisionImage = new(new Uri("pack://application:,,,/Resources/collision.png"));
        private static readonly BitmapImage transparentImage = new(new Uri("pack://application:,,,/Resources/transparent.png"));

        private readonly Stack<StateStackFrame> undoStack = new();
        private readonly Stack<StateStackFrame> redoStack = new();

        private readonly Dictionary<string, BitmapSource> imageCache = new();

        private Point? lastDrawnPoint = new();
        // When editing collision, whether or not moving the mouse removes or adds collision is based on the initially clicked tile
        private bool collisionDrawType = false;

        private WriteableBitmap? tileGridBitmap;
        private WriteableBitmap? collisionGridBitmap;

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
            CreateTileGrid();
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
            if (undoStack.TryPop(out StateStackFrame state))
            {
                redoStack.Push(new StateStackFrame(state.X, state.Y, OpenRoom.TileMap[state.X, state.Y]));
                OpenRoom.TileMap[state.X, state.Y] = state.Tile;

                UpdateTileTexture(state.X, state.Y);

                undoItem.IsEnabled = undoStack.Count > 0;
                redoItem.IsEnabled = true;

                return true;
            }
            return false;
        }

        public bool Redo()
        {
            if (redoStack.TryPop(out StateStackFrame state))
            {
                undoStack.Push(new StateStackFrame(state.X, state.Y, OpenRoom.TileMap[state.X, state.Y]));
                OpenRoom.TileMap[state.X, state.Y] = state.Tile;

                UpdateTileTexture(state.X, state.Y);

                redoItem.IsEnabled = redoStack.Count > 0;
                undoItem.IsEnabled = true;

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
            undoStack.Clear();
            redoStack.Clear();
            undoItem.IsEnabled = false;
            redoItem.IsEnabled = false;

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

            CreateTileGrid();
        }

        private void CreateTileGrid()
        {
            UpdateGridBackground();

            int xSize = OpenRoom.TileMap.GetLength(0);
            int ySize = OpenRoom.TileMap.GetLength(1);

            tileGridBitmap = new WriteableBitmap(
                // Having a pixel dimension be 0 is not possible,
                // so use 1 pixel instead if room is 0 tiles in size
                Math.Max(1, (int)TileSize.X * xSize),
                Math.Max(1, (int)TileSize.Y * ySize),
                96,
                96,
                PixelFormats.Bgra32,
                null);
            tileGridDisplay.Source = tileGridBitmap;

            tileGridDisplay.Width = xSize * TileSize.X;
            tileGridDisplay.Height = ySize * TileSize.Y;

            collisionGridBitmap = new WriteableBitmap(
                // Having a pixel dimension be 0 is not possible,
                // so use 1 pixel instead if room is 0 tiles in size
                Math.Max(1, (int)TileSize.X * xSize),
                Math.Max(1, (int)TileSize.Y * ySize),
                96,
                96,
                PixelFormats.Bgra32,
                null);
            collisionGridDisplay.Source = collisionGridBitmap;

            collisionGridDisplay.Width = xSize * TileSize.X;
            collisionGridDisplay.Height = ySize * TileSize.Y;

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    UpdateTileTexture(x, y);
                }
            }

            CreateGridOverlay();
        }

        private void UpdateTileTexture(int x, int y)
        {
            if (tileGridBitmap is null || collisionGridBitmap is null)
            {
                CreateTileGrid();
                return;
            }

            string textureName = OpenRoom.TileMap[x, y].Texture;

            if (!imageCache.TryGetValue(textureName, out BitmapSource? imageSource))
            {
                string texturePath = Path.Join(TileTextureFolderPath, textureName);
                texturePath = Path.ChangeExtension(texturePath, "png");

                if (!File.Exists(texturePath))
                {
                    texturePath = "pack://application:,,,/Resources/placeholder.png";
                }

                imageSource = new BitmapImage(new Uri(texturePath));

                imageCache[textureName] = imageSource;
            }

            tileGridBitmap.CopyImage(
                imageSource, x * (int)TileSize.X, y * (int)TileSize.Y, (int)TileSize.X, (int)TileSize.Y);
            collisionGridBitmap.CopyImage(
                collisionEditItem.IsChecked && OpenRoom.TileMap[x, y].IsCollision ? collisionImage : transparentImage,
                x * (int)TileSize.X, y * (int)TileSize.Y, (int)TileSize.X, (int)TileSize.Y);
        }

        private void UpdateGridBackground()
        {
            tileMapScroll.Background = new SolidColorBrush(new Color()
            {
                R = OpenRoom.BackgroundColor.R,
                G = OpenRoom.BackgroundColor.G,
                B = OpenRoom.BackgroundColor.B,
                A = OpenRoom.BackgroundColor.A
            });
        }

        private void CreateGridOverlay()
        {
            gridOverlayXDisplay.Children.Clear();
            gridOverlayYDisplay.Children.Clear();

            int xSize = OpenRoom.TileMap.GetLength(0);
            int ySize = OpenRoom.TileMap.GetLength(1);
            gridOverlayXDisplay.Width = xSize * TileSize.X;
            gridOverlayXDisplay.Height = ySize * TileSize.Y;
            gridOverlayYDisplay.Width = xSize * TileSize.X;
            gridOverlayYDisplay.Height = ySize * TileSize.Y;

            if (gridOverlayItem.IsChecked)
            {
                for (int x = 1; x < xSize; x++)
                {
                    _ = gridOverlayXDisplay.Children.Add(new System.Windows.Shapes.Rectangle()
                    {
                        Height = gridOverlayXDisplay.Height,
                        Width = 3,
                        Margin = new Thickness(x * TileSize.X - 1, 0, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        IsHitTestVisible = false,
                        Fill = Brushes.DarkGoldenrod
                    });
                }

                for (int y = 1; y < ySize; y++)
                {
                    _ = gridOverlayYDisplay.Children.Add(new System.Windows.Shapes.Rectangle()
                    {
                        Height = 3,
                        Width = gridOverlayYDisplay.Width,
                        Margin = new Thickness(0, y * TileSize.Y - 1, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        IsHitTestVisible = false,
                        Fill = Brushes.DarkGoldenrod
                    });
                }
            }
        }

        private void UpdateTextureSelectionPanel()
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

        private void PushUndoStack(int x, int y)
        {
            UnsavedChanges = true;

            redoStack.Clear();
            undoStack.Push(new StateStackFrame(x, y, OpenRoom.TileMap[x, y]));

            undoItem.IsEnabled = true;
            redoItem.IsEnabled = false;
        }

        private void EditTileAtPosition(int x, int y)
        {
            if (x < 0 || y < 0 || x >= OpenRoom.TileMap.GetLength(0) || y >= OpenRoom.TileMap.GetLength(1))
            {
                return;
            }

            if (collisionEditItem.IsChecked)
            {
                if (collisionDrawType == OpenRoom.TileMap[x, y].IsCollision)
                {
                    // Nothing to change
                    return;
                }

                PushUndoStack(x, y);

                OpenRoom.TileMap[x, y] = OpenRoom.TileMap[x, y] with { IsCollision = collisionDrawType };
            }
            else
            {
                if (SelectedTextureName is null)
                {
                    return;
                }

                if (SelectedTextureName == OpenRoom.TileMap[x, y].Texture)
                {
                    // Nothing to change
                    return;
                }

                PushUndoStack(x, y);

                OpenRoom.TileMap[x, y] = OpenRoom.TileMap[x, y] with { Texture = SelectedTextureName };
            }

            UpdateTileTexture(x, y);
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
                double newScale = Math.Exp(Math.Log(tileMapScaleTransform.ScaleX) + (e.Delta * 0.0007));
                if (newScale > 0)
                {
                    tileMapScaleTransform.ScaleX = newScale;
                    tileMapScaleTransform.ScaleY = newScale;
                }
            }
        }

        private void tileGridDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e is { ChangedButton: MouseButton.Left, ButtonState: MouseButtonState.Pressed })
            {
                Point relativeMousePos = e.GetPosition(tileGridDisplay);
                relativeMousePos = new Point(relativeMousePos.X / TileSize.X, relativeMousePos.Y / TileSize.Y);

                lastDrawnPoint = relativeMousePos;
                if (collisionEditItem.IsChecked)
                {
                    collisionDrawType = !OpenRoom.TileMap[(int)relativeMousePos.X, (int)relativeMousePos.Y].IsCollision;
                }
                EditTileAtPosition((int)relativeMousePos.X, (int)relativeMousePos.Y);
            }
        }

        private void tileGridDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point relativeMousePos = e.GetPosition(tileGridDisplay);
                relativeMousePos = new Point(relativeMousePos.X / TileSize.X, relativeMousePos.Y / TileSize.Y);

                // Bresenham's line algorithm - removes gaps between drawn points when moving cursor quickly
                int x1 = (int)(lastDrawnPoint?.X ?? relativeMousePos.X);
                int y1 = (int)(lastDrawnPoint?.Y ?? relativeMousePos.Y);
                int x2 = (int)relativeMousePos.X;
                int y2 = (int)relativeMousePos.Y;
                int dx = Math.Abs(x2 - x1);
                int dy = Math.Abs(y2 - y1);
                int sx = x1 < x2 ? 1 : -1;
                int sy = y1 < y2 ? 1 : -1;
                int err = dx - dy;
                while (true)
                {
                    EditTileAtPosition(x1, y1);
                    if (x1 == x2 && y1 == y2)
                    {
                        // End reached
                        break;
                    }
                    int e2 = err * 2;
                    if (e2 > -dy)
                    {
                        err -= dy;
                        x1 += sx;
                    }
                    if (e2 < dx)
                    {
                        err += dx;
                        y1 += sy;
                    }
                }
                lastDrawnPoint = relativeMousePos;
            }
        }

        private void tileGridDisplay_MouseLeave(object sender, MouseEventArgs e)
        {
            lastDrawnPoint = null;
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
            CreateGridOverlay();
        }

        private void collisionEditItem_OnClick(object sender, RoutedEventArgs e)
        {
            CreateTileGrid();
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

            ToolWindows.DimensionsDialog dialog = new(xLength, yLength)
            {
                Owner = this
            };
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

        private void BackgroundColourItem_OnClick(object sender, RoutedEventArgs e)
        {
            ToolWindows.ColorDialog dialog = new()
            {
                SelectedColor = new Color()
                {
                    R = OpenRoom.BackgroundColor.R,
                    G = OpenRoom.BackgroundColor.G,
                    B = OpenRoom.BackgroundColor.B,
                    A = byte.MaxValue
                },
                StartFullOpen = true
            };
            if (dialog.ShowDialog(this))
            {
                OpenRoom.BackgroundColor = new Microsoft.Xna.Framework.Color()
                {
                    R = dialog.SelectedColor.R,
                    G = dialog.SelectedColor.G,
                    B = dialog.SelectedColor.B,
                    A = byte.MaxValue
                };
                UnsavedChanges = true;
                UpdateGridBackground();
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
                case Key.S when e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control):
                    Save();
                    break;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Prevent main window from being hidden when this window closes
            _ = ParentWindow.Activate();
        }
    }
}

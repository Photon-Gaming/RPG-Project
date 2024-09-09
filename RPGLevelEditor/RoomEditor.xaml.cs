using System.IO;
using System.Media;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using RPGGame.GameObject.Entity;

namespace RPGLevelEditor
{
    /// <summary>
    /// Interaction logic for RoomEditor.xaml
    /// </summary>
    public partial class RoomEditor : Window
    {
        private enum ToolType
        {
            Tile,
            Collision,
            Entity
        }

        private enum ConnectionType
        {
            None,
            Vert,
            Side
        }

        public const string TileTextureFolderName = "Tiles";
        public const string AutoTileTextureFolderName = "AutoTiled";
        public const string EntityTextureFolderName = "Entities";
        public const string ToolEntityTextureFolderPath = "pack://application:,,,/Resources/ToolEntity/";
        public readonly string TileTextureFolderPath;
        public readonly string AutoTileTextureFolderPath;
        public readonly string EntityTextureFolderPath;

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

        private float _currentGridInterval = 1;
        public float CurrentGridInterval
        {
            get => _currentGridInterval;
            set
            {
                _currentGridInterval = value;
                gridSizeLabel.Content = value < 1 ? $"Grid Size: 1/{1 / value}" : $"Grid Size: {value}";
            }
        }

        private static readonly BitmapImage placeholderImage = new(new Uri("pack://application:,,,/Resources/placeholder.png"));
        private static readonly BitmapImage collisionImage = new(new Uri("pack://application:,,,/Resources/collision.png"));
        private static readonly BitmapImage transparentImage = new(new Uri("pack://application:,,,/Resources/transparent.png"));

        private static readonly Dictionary<(ConnectionType North, ConnectionType East,
            ConnectionType South, ConnectionType West), string> tiledTextureNames = new()
        {
            { (ConnectionType.None, ConnectionType.None, ConnectionType.None, ConnectionType.None), "c" },
            { (ConnectionType.None, ConnectionType.None, ConnectionType.None, ConnectionType.Vert), "c-w" },
            { (ConnectionType.None, ConnectionType.None, ConnectionType.Vert, ConnectionType.None), "c-s" },
            { (ConnectionType.None, ConnectionType.None, ConnectionType.Vert, ConnectionType.Vert), "c-sw" },
            { (ConnectionType.None, ConnectionType.Vert, ConnectionType.None, ConnectionType.None), "c-e" },
            { (ConnectionType.None, ConnectionType.Vert, ConnectionType.None, ConnectionType.Vert), "c-ew" },
            { (ConnectionType.None, ConnectionType.Vert, ConnectionType.Vert, ConnectionType.None), "c-es" },
            { (ConnectionType.None, ConnectionType.Vert, ConnectionType.Vert, ConnectionType.Vert), "c-esw" },
            { (ConnectionType.Vert, ConnectionType.None, ConnectionType.None, ConnectionType.None), "c-n" },
            { (ConnectionType.Vert, ConnectionType.None, ConnectionType.None, ConnectionType.Vert), "c-nw" },
            { (ConnectionType.Vert, ConnectionType.None, ConnectionType.Vert, ConnectionType.None), "c-ns" },
            { (ConnectionType.Vert, ConnectionType.None, ConnectionType.Vert, ConnectionType.Vert), "c-nsw" },
            { (ConnectionType.Vert, ConnectionType.Vert, ConnectionType.None, ConnectionType.None), "c-ne" },
            { (ConnectionType.Vert, ConnectionType.Vert, ConnectionType.None, ConnectionType.Vert), "c-new" },
            { (ConnectionType.Vert, ConnectionType.Vert, ConnectionType.Vert, ConnectionType.None), "c-nes" },
            { (ConnectionType.Vert, ConnectionType.Vert, ConnectionType.Vert, ConnectionType.Vert), "c-nesw" },

            { (ConnectionType.None, ConnectionType.None, ConnectionType.None, ConnectionType.Side), "w" },
            { (ConnectionType.None, ConnectionType.Vert, ConnectionType.None, ConnectionType.Side), "w-e" },
            { (ConnectionType.Vert, ConnectionType.None, ConnectionType.None, ConnectionType.Side), "w-n" },
            { (ConnectionType.Vert, ConnectionType.Vert, ConnectionType.None, ConnectionType.Side), "w-ne" },

            { (ConnectionType.None, ConnectionType.None, ConnectionType.Side, ConnectionType.None), "s" },
            { (ConnectionType.None, ConnectionType.None, ConnectionType.Side, ConnectionType.Vert), "s-w" },
            { (ConnectionType.Vert, ConnectionType.None, ConnectionType.Side, ConnectionType.None), "s-n" },
            { (ConnectionType.Vert, ConnectionType.None, ConnectionType.Side, ConnectionType.Vert), "s-nw" },

            { (ConnectionType.None, ConnectionType.None, ConnectionType.Side, ConnectionType.Side), "sw" },
            { (ConnectionType.Vert, ConnectionType.None, ConnectionType.Side, ConnectionType.Side), "sw-n" },

            { (ConnectionType.None, ConnectionType.Side, ConnectionType.None, ConnectionType.None), "e" },
            { (ConnectionType.None, ConnectionType.Side, ConnectionType.None, ConnectionType.Vert), "e-w" },
            { (ConnectionType.None, ConnectionType.Side, ConnectionType.Vert, ConnectionType.None), "e-s" },
            { (ConnectionType.None, ConnectionType.Side, ConnectionType.Vert, ConnectionType.Vert), "e-sw" },

            { (ConnectionType.None, ConnectionType.Side, ConnectionType.None, ConnectionType.Side), "ew" },

            { (ConnectionType.None, ConnectionType.Side, ConnectionType.Side, ConnectionType.None), "es" },
            { (ConnectionType.None, ConnectionType.Side, ConnectionType.Side, ConnectionType.Vert), "es-w" },

            { (ConnectionType.None, ConnectionType.Side, ConnectionType.Side, ConnectionType.Side), "esw" },

            { (ConnectionType.Side, ConnectionType.None, ConnectionType.None, ConnectionType.None), "n" },
            { (ConnectionType.Side, ConnectionType.None, ConnectionType.Vert, ConnectionType.None), "n-s" },
            { (ConnectionType.Side, ConnectionType.Vert, ConnectionType.None, ConnectionType.None), "n-e" },
            { (ConnectionType.Side, ConnectionType.Vert, ConnectionType.Vert, ConnectionType.None), "n-es" },

            { (ConnectionType.Side, ConnectionType.None, ConnectionType.None, ConnectionType.Side), "nw" },
            { (ConnectionType.Side, ConnectionType.Vert, ConnectionType.None, ConnectionType.Side), "nw-e" },

            { (ConnectionType.Side, ConnectionType.None, ConnectionType.Side, ConnectionType.None), "ns" },

            { (ConnectionType.Side, ConnectionType.None, ConnectionType.Side, ConnectionType.Side), "nsw" },

            { (ConnectionType.Side, ConnectionType.Side, ConnectionType.None, ConnectionType.None), "ne" },
            { (ConnectionType.Side, ConnectionType.Side, ConnectionType.Vert, ConnectionType.None), "ne-s" },

            { (ConnectionType.Side, ConnectionType.Side, ConnectionType.None, ConnectionType.Side), "new" },

            { (ConnectionType.Side, ConnectionType.Side, ConnectionType.Side, ConnectionType.None), "nes" },

            { (ConnectionType.Side, ConnectionType.Side, ConnectionType.Side, ConnectionType.Side), "nesw" },
        };

        // Use center texture as tile set preview
        private static readonly string tileThumbnailName =
            tiledTextureNames[(ConnectionType.None, ConnectionType.None, ConnectionType.None, ConnectionType.None)];

        private readonly Stack<StateStackFrame> undoStack = new();
        private readonly Stack<StateStackFrame> redoStack = new();

        private readonly Dictionary<string, BitmapSource> tileImageCache = new();
        private readonly Dictionary<string, BitmapSource> entityImageCache = new();

        private ToolType currentToolType = ToolType.Tile;

        private Entity? selectedEntity = null;
        private bool movingEntity = false;
        private Point moveStartOffset = new();

        private bool selectingPosition = false;
        private PropertyInfo? selectedPositionTarget = null;

        private bool selectingEntity = false;
        private PropertyInfo? selectedEntityTarget = null;

        private Point? lastDrawnPoint = new();
        // When editing collision, whether or not moving the mouse removes or adds collision is based on the initially clicked tile
        private bool collisionDrawType = false;

        private WriteableBitmap? tileGridBitmap;
        private WriteableBitmap? collisionGridBitmap;
        private WriteableBitmap? entityBitmap;

        public RoomEditor(string roomPath, MainWindow parent, bool forceCreateNew = false)
        {
            InitializeComponent();

            RoomPath = roomPath;
            Owner = parent;
            ParentWindow = parent;

            TileTextureFolderPath = Path.Join(ParentWindow.EditorConfig.ContentFolderPath,
                MainWindow.TextureFolderName, TileTextureFolderName);
            AutoTileTextureFolderPath = Path.Join(TileTextureFolderPath, AutoTileTextureFolderName);
            EntityTextureFolderPath = Path.Join(ParentWindow.EditorConfig.ContentFolderPath,
                MainWindow.TextureFolderName, EntityTextureFolderName);
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
                new List<Entity>(),
                Microsoft.Xna.Framework.Color.CornflowerBlue);

            UnsavedChanges = forceCreateNew;

            Title += " - " + RoomPath;

            UpdateTextureSelectionPanel();
            SelectEntity(null);
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
            if (undoStack.TryPop(out StateStackFrame? state))
            {
                state.RestoreState(true);

                undoItem.IsEnabled = undoStack.Count > 0;
                redoItem.IsEnabled = redoStack.Count > 0;

                return true;
            }
            return false;
        }

        public bool Redo()
        {
            if (redoStack.TryPop(out StateStackFrame? state))
            {
                state.RestoreState(false);

                redoItem.IsEnabled = redoStack.Count > 0;
                undoItem.IsEnabled = redoStack.Count > 0;

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
                OpenRoom.Entities.Where(e => e.Position.X < xSize && e.Position.Y < ySize).ToList(),
                OpenRoom.BackgroundColor);

            SelectEntity(null);
            CreateTileGrid();
        }

        private void PromptDimensionChange()
        {
            if (SelectedTextureName is null)
            {
                _ = MessageBox.Show(this, "Please select a tile texture before changing room dimensions",
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

        private void PromptBackgroundColourChange()
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

            collisionGridBitmap = tileGridBitmap.Clone();
            collisionGridDisplay.Source = collisionGridBitmap;
            collisionGridDisplay.Width = xSize * TileSize.X;
            collisionGridDisplay.Height = ySize * TileSize.Y;

            entityBitmap = tileGridBitmap.Clone();
            entityDisplay.Source = entityBitmap;
            entityDisplay.Width = xSize * TileSize.X;
            entityDisplay.Height = ySize * TileSize.Y;

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    UpdateTileTexture(x, y);
                }
            }

            foreach (Entity entity in OpenRoom.Entities)
            {
                DrawEntity(entity, false);
            }

            CreateGridOverlay();
            UpdateBitmapVisibility();
        }

        private void UpdateBitmapVisibility()
        {
            collisionGridDisplay.Visibility = currentToolType == ToolType.Collision || alwaysShowCollisionItem.IsChecked
                ? Visibility.Visible : Visibility.Collapsed;
            entityDisplay.Visibility = currentToolType == ToolType.Entity || alwaysShowEntitiesItem.IsChecked
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateTileTexture(int x, int y)
        {
            if (tileGridBitmap is null || collisionGridBitmap is null)
            {
                CreateTileGrid();
                return;
            }

            BitmapSource imageSource = LoadTileTexture(x, y);

            tileGridBitmap.CopyImage(
                imageSource, x * (int)TileSize.X, y * (int)TileSize.Y, (int)TileSize.X, (int)TileSize.Y);
            collisionGridBitmap.CopyImage(
                OpenRoom.TileMap[x, y].IsCollision ? collisionImage : transparentImage,
                x * (int)TileSize.X, y * (int)TileSize.Y, (int)TileSize.X, (int)TileSize.Y);
        }

        private BitmapSource LoadTileTexture(int x, int y)
        {
            string textureName = OpenRoom.TileMap[x, y].Texture;

            if (!tileImageCache.TryGetValue(textureName, out BitmapSource? imageSource))
            {
                string texturePath = Path.Join(TileTextureFolderPath, textureName);
                texturePath = Path.ChangeExtension(texturePath, "png");

                if (!File.Exists(texturePath))
                {
                    imageSource = placeholderImage;
                }
                else
                {
                    imageSource = new BitmapImage(new Uri(texturePath));
                    tileImageCache[textureName] = imageSource;
                }
            }

            return imageSource;
        }

        private BitmapSource LoadEntityTexture(Entity entity)
        {
            if (entity.Texture is not null)
            {
                if (!entityImageCache.TryGetValue(entity.Texture, out BitmapSource? imageSource))
                {
                    string texturePath = Path.Join(EntityTextureFolderPath, entity.Texture);
                    texturePath = Path.ChangeExtension(texturePath, "png");

                    if (!File.Exists(texturePath))
                    {
                        imageSource = placeholderImage;
                    }
                    else
                    {
                        imageSource = new BitmapImage(new Uri(texturePath));
                        entityImageCache[entity.Texture] = imageSource;
                    }
                }

                return imageSource;
            }

            return hideInvisibleEntitiesItem.IsChecked
                ? transparentImage
                : new BitmapImage(new Uri(ToolEntityTextureFolderPath + entity.GetType().Name + ".png"));
        }

        private void DrawEntity(Entity entity, bool erase)
        {
            if (entityBitmap is null)
            {
                CreateTileGrid();
                return;
            }

            if (entity.IsOutOfBounds(OpenRoom))
            {
                return;
            }

            BitmapSource imageSource = erase ? transparentImage : LoadEntityTexture(entity);
            entityBitmap.CopyImage(imageSource,
                (int)(entity.TopLeft.X * TileSize.X), (int)(entity.TopLeft.Y * TileSize.Y),
                (int)(entity.Size.X * TileSize.X), (int)(entity.Size.Y * TileSize.Y));

            if (erase)
            {
                // Redraw any overlapped entities
                foreach (Entity overlappedEntity in OpenRoom.Entities.Where(e => e.Collides(entity)))
                {
                    DrawEntity(overlappedEntity, false);
                }
            }
        }

        private void UpdateSelectedEntity(bool updatePropertiesPanel = true)
        {
            if (updatePropertiesPanel)
            {
                UpdateEntityPropertiesPanel();
            }

            if (selectedEntity is null)
            {
                selectedEntityContainer.Visibility = Visibility.Collapsed;
                return;
            }

            selectedEntityContainer.Visibility = Visibility.Visible;

            selectedEntityBorder.Width = selectedEntity.Size.X * TileSize.X + selectedEntityBorder.StrokeThickness;
            selectedEntityBorder.Height = selectedEntity.Size.Y * TileSize.Y + selectedEntityBorder.StrokeThickness;
            selectedEntityBorder.Margin = new Thickness(
                selectedEntity.TopLeft.X * TileSize.X - (selectedEntityBorder.StrokeThickness / 2),
                selectedEntity.TopLeft.Y * TileSize.Y - (selectedEntityBorder.StrokeThickness / 2), 0, 0);

            selectedEntityOrigin.Margin = new Thickness(
                selectedEntity.Position.X * TileSize.X - (selectedEntityOrigin.Width / 2),
                selectedEntity.Position.Y * TileSize.Y - (selectedEntityOrigin.Height / 2), 0, 0);

            selectedEntityImage.Width = selectedEntity.Size.X * TileSize.X;
            selectedEntityImage.Height = selectedEntity.Size.Y * TileSize.Y;
            selectedEntityImage.Margin = new Thickness(
                selectedEntity.TopLeft.X * TileSize.X, selectedEntity.TopLeft.Y * TileSize.Y, 0, 0);
            selectedEntityImage.Source = LoadEntityTexture(selectedEntity);

            selectedEntityImage.BringIntoView();
        }

        private static IEnumerable<(PropertyInfo Property, EditorModifiableAttribute EditorAttribute)> GetEditableEntityProperties(Entity entity)
        {
            foreach (PropertyInfo property in entity.GetType().GetProperties())
            {
                List<EditorModifiableAttribute> attributes = property
                    .GetCustomAttributes(typeof(EditorModifiableAttribute))
                    .Cast<EditorModifiableAttribute>().ToList();
                if (attributes.Count == 0)
                {
                    // Only properties with the EditorModifiable attribute should be shown
                    continue;
                }

                yield return (property, attributes[0]);
            }
        }

        private void UpdateEntityPropertiesPanel()
        {
            entityPropertiesPanel.Children.Clear();

            if (selectedEntity is null)
            {
                entityApplyButton.IsEnabled = false;
                return;
            }

            entityApplyButton.IsEnabled = true;

            foreach ((PropertyInfo property, EditorModifiableAttribute editorAttribute) in GetEditableEntityProperties(selectedEntity))
            {
                _ = entityPropertiesPanel.Children.Add(CreatePropertyEditBox(property, editorAttribute, selectedEntity));
                _ = entityPropertiesPanel.Children.Add(new Separator());
            }
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
                for (float x = CurrentGridInterval; x < xSize; x += CurrentGridInterval)
                {
                    _ = gridOverlayXDisplay.Children.Add(new System.Windows.Shapes.Rectangle()
                    {
                        Height = gridOverlayXDisplay.Height,
                        Width = CurrentGridInterval,
                        Margin = new Thickness(x * TileSize.X - (CurrentGridInterval / 2), 0, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        IsHitTestVisible = false,
                        Fill = Brushes.DarkGoldenrod
                    });
                }

                for (float y = CurrentGridInterval; y < ySize; y += CurrentGridInterval)
                {
                    _ = gridOverlayYDisplay.Children.Add(new System.Windows.Shapes.Rectangle()
                    {
                        Height = CurrentGridInterval,
                        Width = gridOverlayYDisplay.Width,
                        Margin = new Thickness(0, y * TileSize.Y - (CurrentGridInterval / 2), 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        IsHitTestVisible = false,
                        Fill = Brushes.DarkGoldenrod
                    });
                }
            }
        }

        private void GridOverlayEnlarge()
        {
            if (CurrentGridInterval >= 128)
            {
                return;
            }

            CurrentGridInterval *= 2;

            CreateGridOverlay();
        }

        private void GridOverlayShrink()
        {
            if (CurrentGridInterval <= 0.03125)
            {
                return;
            }

            CurrentGridInterval /= 2;

            CreateGridOverlay();
        }

        private void UpdateTextureSelectionPanel()
        {
            tileTextureSelectPanel.Children.Clear();

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
                _ = tileTextureSelectPanel.Children.Add(newElement);
            }

            foreach (string textureFolder in Directory.EnumerateDirectories(AutoTileTextureFolderPath))
            {
                string textureSetName = Path.GetFileName(textureFolder);
                string centerTextureName = Path.Join(textureSetName, tileThumbnailName);
                string centerTextureRelative = Path.Join(AutoTileTextureFolderName, centerTextureName);
                string centerTexture = Path.Join(AutoTileTextureFolderPath, centerTextureName);

                Border newElement = new()
                {
                    Margin = new Thickness(3),
                    Width = 32,
                    Height = 32,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderBrush = Brushes.OrangeRed,
                    BorderThickness = new Thickness(centerTextureRelative == SelectedTextureName ? 2 : 0),
                    Child = new Image()
                    {
                        Source = new BitmapImage(new Uri(Path.ChangeExtension(centerTexture, "png"))),
                        Stretch = Stretch.Fill
                    },
                    ToolTip = $"{textureSetName} (Auto Tiled)",
                    Tag = centerTextureRelative
                };
                newElement.MouseUp += TextureSelect_MouseUp;
                _ = tileTextureSelectPanel.Children.Add(newElement);
            }
        }

        private void PushUndoStack(StateStackFrame stackFrame)
        {
            UnsavedChanges = true;

            redoStack.Clear();
            undoStack.Push(stackFrame);

            undoItem.IsEnabled = true;
            redoItem.IsEnabled = false;
        }

        private void PushTileUndoStack(int x, int y)
        {
            PushUndoStack(new TileEditStackFrame(this, x, y, OpenRoom.TileMap[x, y]));
        }

        private void PushEntityMoveUndoStack(Entity entity)
        {
            PushUndoStack(new EntityMoveStackFrame(this, entity, entity.Position.X, entity.Position.Y));
        }

        private void PushEntityCreateUndoStack(float x, float y)
        {
            PushUndoStack(new EntityCreateStackFrame(this, x, y));
        }

        private void PushEntityPropertyEditUndoStack(Entity entity)
        {
            PushUndoStack(new EntityPropertyEditStackFrame(this, entity));
        }

        private void PushEntityDeleteUndoStack(Entity entity)
        {
            PushUndoStack(new EntityDeleteStackFrame(this, entity));
        }

        private void EditTileAtPosition(int x, int y)
        {
            if (OpenRoom.IsOutOfBounds(new Microsoft.Xna.Framework.Vector2(x, y)))
            {
                return;
            }

            if (currentToolType == ToolType.Collision)
            {
                if (collisionDrawType == OpenRoom.TileMap[x, y].IsCollision)
                {
                    // Nothing to change
                    return;
                }

                PushTileUndoStack(x, y);

                OpenRoom.TileMap[x, y] = OpenRoom.TileMap[x, y] with { IsCollision = collisionDrawType };
            }
            else if (currentToolType == ToolType.Tile)
            {
                if (SelectedTextureName is null)
                {
                    return;
                }

                string currentTexture = OpenRoom.TileMap[x, y].Texture;
                if (SelectedTextureName == currentTexture
                    || (SelectedTextureName.StartsWith(AutoTileTextureFolderName, StringComparison.OrdinalIgnoreCase)
                        // Only consider the tile set, not the exact texture, for auto tiled textures
                        && Path.GetDirectoryName(SelectedTextureName) == Path.GetDirectoryName(currentTexture)))
                {
                    // Nothing to change
                    return;
                }

                PushTileUndoStack(x, y);

                OpenRoom.TileMap[x, y] = OpenRoom.TileMap[x, y] with { Texture = SelectedTextureName };

                UpdateTiling(x, y);
            }
            else
            {
                return;
            }

            UpdateTileTexture(x, y);
        }

        private void UpdateTiling(int x, int y, bool updateAdjacent = true)
        {
            // To check if adjacent tiles are part of the same tile set, the directory name is used instead of the file name,
            // as the file name changes depending on the connected sides of the tile.
            string? currentTile = Path.GetDirectoryName(OpenRoom.TileMap[x, y].Texture);

            bool onNorthEdge = y <= 0;
            bool onEastEdge = x >= OpenRoom.TileMap.GetLength(0) - 1;
            bool onSouthEdge = y >= OpenRoom.TileMap.GetLength(1) - 1;
            bool onWestEdge = x <= 0;

            if (currentTile is not null && currentTile.StartsWith(AutoTileTextureFolderName, StringComparison.OrdinalIgnoreCase))
            {
                bool north = onNorthEdge || Path.GetDirectoryName(OpenRoom.TileMap[x, y - 1].Texture) != currentTile;
                bool east = onEastEdge || Path.GetDirectoryName(OpenRoom.TileMap[x + 1, y].Texture) != currentTile;
                bool south = onSouthEdge || Path.GetDirectoryName(OpenRoom.TileMap[x, y + 1].Texture) != currentTile;
                bool west = onWestEdge || Path.GetDirectoryName(OpenRoom.TileMap[x - 1, y].Texture) != currentTile;

                ConnectionType northConnection = north
                    ? ConnectionType.Side
                    : !east && (onEastEdge || Path.GetDirectoryName(OpenRoom.TileMap[x + 1, y - 1].Texture) != currentTile)
                        ? ConnectionType.Vert
                        : ConnectionType.None;
                ConnectionType eastConnection = east
                    ? ConnectionType.Side
                    : !south && (onSouthEdge || Path.GetDirectoryName(OpenRoom.TileMap[x + 1, y + 1].Texture) != currentTile)
                        ? ConnectionType.Vert
                        : ConnectionType.None;
                ConnectionType southConnection = south
                    ? ConnectionType.Side
                    : !west && (onWestEdge || Path.GetDirectoryName(OpenRoom.TileMap[x - 1, y + 1].Texture) != currentTile)
                        ? ConnectionType.Vert
                        : ConnectionType.None;
                ConnectionType westConnection = west
                    ? ConnectionType.Side
                    : !north && (onNorthEdge || Path.GetDirectoryName(OpenRoom.TileMap[x - 1, y - 1].Texture) != currentTile)
                        ? ConnectionType.Vert
                        : ConnectionType.None;

                OpenRoom.TileMap[x, y] = OpenRoom.TileMap[x, y] with
                {
                    Texture = Path.Join(currentTile, tiledTextureNames[(northConnection, eastConnection, southConnection, westConnection)])
                };
            }

            UpdateTileTexture(x, y);

            if (updateAdjacent)
            {
                if (!onNorthEdge)
                {
                    UpdateTiling(x, y - 1, false);
                    if (!onEastEdge)
                    {
                        UpdateTiling(x + 1, y - 1, false);
                    }
                    if (!onWestEdge)
                    {
                        UpdateTiling(x - 1, y - 1, false);
                    }
                }
                if (!onEastEdge)
                {
                    UpdateTiling(x + 1, y, false);
                }
                if (!onSouthEdge)
                {
                    UpdateTiling(x, y + 1, false);
                    if (!onEastEdge)
                    {
                        UpdateTiling(x + 1, y + 1, false);
                    }
                    if (!onWestEdge)
                    {
                        UpdateTiling(x - 1, y + 1, false);
                    }
                }
                if (!onWestEdge)
                {
                    UpdateTiling(x - 1, y, false);
                }
            }
        }

        private void SelectEntity(Entity? entity)
        {
            if (selectingEntity && selectedEntity is not null && entity is not null)
            {
                selectingEntity = false;
                tileGridDisplay.Cursor = null;

                PushEntityPropertyEditUndoStack(selectedEntity);

                selectedEntityTarget!.SetValue(selectedEntity, entity.Name);

                selectedEntityTarget = null;

                UpdateSelectedEntity();
                return;
            }

            if (ReferenceEquals(entity, selectedEntity))
            {
                // Entity is already selected
                // - update the selected entity elements but skip everything else
                UpdateSelectedEntity();
                return;
            }

            selectingPosition = false;
            selectedPositionTarget = null;
            selectingEntity = false;
            selectedEntityTarget = null;
            tileGridDisplay.Cursor = null;

            if (selectedEntity is not null)
            {
                // Draw any previously selected entity back onto the entity canvas
                DrawEntity(selectedEntity, false);
            }

            if (entity is not null)
            {
                // Remove the newly selected entity from the entity canvas and put it into the separate selection elements
                DrawEntity(entity, true);
                // If an entity is being selected, switch to entity tool mode
                toolPanel.SelectedIndex = 2;
            }

            selectedEntity = entity;

            UpdateSelectedEntity();
        }

        private void SelectEntityAtPosition(float x, float y)
        {
            SelectEntity(OpenRoom.Entities.LastOrDefault(e =>
                e.Collides(new Microsoft.Xna.Framework.Vector2(x, y))));
        }

        private void CreateEntityAtPosition(float x, float y, bool pushToUndoStack)
        {
            Entity newEntity = new(
                $"Entity_{Guid.NewGuid()}",
                new Microsoft.Xna.Framework.Vector2(x, y),
                Microsoft.Xna.Framework.Vector2.One,
                null);

            if (newEntity.IsOutOfBounds(OpenRoom) || OpenRoom.Entities.Any(e => e.Collides(newEntity)))
            {
                return;
            }

            if (pushToUndoStack)
            {
                PushEntityCreateUndoStack(x, y);
            }

            OpenRoom.Entities.Add(newEntity);
            SelectEntity(newEntity);
        }

        private void DeleteEntity(Entity entity)
        {
            PushEntityDeleteUndoStack(entity);
            SelectEntity(null);
            DrawEntity(entity, true);
            _ = OpenRoom.Entities.Remove(entity);
        }

        private void ProcessEntityMove()
        {
            if (!movingEntity)
            {
                return;
            }

            if (selectedEntity is null)
            {
                movingEntity = false;
                return;
            }

            Point relativeMousePos = Mouse.GetPosition(tileGridDisplay);
            relativeMousePos = new Point((relativeMousePos.X - moveStartOffset.X) / TileSize.X, (relativeMousePos.Y - moveStartOffset.Y) / TileSize.Y);

            Microsoft.Xna.Framework.Vector2 oldPos = selectedEntity.Position;

            Microsoft.Xna.Framework.Vector2 newPos = new((float)relativeMousePos.X, (float)relativeMousePos.Y);
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                // Snap to nearest grid intersection based on current grid size
                newPos.X = MathF.Round(newPos.X / CurrentGridInterval) * CurrentGridInterval;
                newPos.Y = MathF.Round(newPos.Y / CurrentGridInterval) * CurrentGridInterval;
            }

            _ = selectedEntity.Move(newPos, false);
            if (selectedEntity.IsOutOfBounds(OpenRoom) || OpenRoom.Entities.Any(ent => ent.Collides(selectedEntity)))
            {
                _ = selectedEntity.Move(oldPos, false);
            }
            UpdateSelectedEntity(false);
        }

        private void ShowProblems()
        {
            StringBuilder problems = new();

            // Missing tile textures
            for (int x = 0; x < OpenRoom.TileMap.GetLength(0); x++)
            {
                for (int y = 0; y < OpenRoom.TileMap.GetLength(1); y++)
                {
                    if (ReferenceEquals(LoadTileTexture(x, y), placeholderImage))
                    {
                        _ = problems.AppendLine(
                            $"Tile at ({x}, {y}) uses texture \"{OpenRoom.TileMap[x, y].Texture}\", which doesn't exist.");
                    }
                }
            }

            // Missing entity textures
            foreach (Entity entity in OpenRoom.Entities.Where(e => ReferenceEquals(LoadEntityTexture(e), placeholderImage)))
            {
                _ = problems.AppendLine(
                    $"Entity \"{entity.Name}\" of type \"{entity.GetType().Name}\" at ({entity.Position.X}, {entity.Position.Y}) " +
                    $"uses texture \"{entity.Texture}\", which doesn't exist.");
            }

            // Out of bounds entities
            foreach (Entity entity in OpenRoom.Entities.Where(e => e.IsOutOfBounds(OpenRoom)))
            {
                _ = problems.AppendLine(
                    $"Entity \"{entity.Name}\" of type \"{entity.GetType().Name}\" at ({entity.Position.X}, {entity.Position.Y}) is out of bounds.");
            }

            // Overlapping entities
            foreach (Entity entity in OpenRoom.Entities.Where(e => OpenRoom.Entities.Any(o => o.Collides(e))))
            {
                _ = problems.AppendLine(
                    $"Entity \"{entity.Name}\" of type \"{entity.GetType().Name}\" at ({entity.Position.X}, {entity.Position.Y}) collides with another entity.");
            }

            // Duplicate names
            HashSet<string> seenNames = new(StringComparer.OrdinalIgnoreCase);
            foreach (Entity entity in OpenRoom.Entities)
            {
                if (!seenNames.Add(entity.Name))
                {
                    _ = problems.AppendLine(
                        $"Entity \"{entity.Name}\" of type \"{entity.GetType().Name}\" at ({entity.Position.X}, {entity.Position.Y}) uses the same name as another entity.");
                }
            }

            // Use of reserved name
            foreach (Entity entity in OpenRoom.Entities.Where(e => e.Name.Equals(Player.PlayerEntityName, StringComparison.OrdinalIgnoreCase)))
            {
                _ = problems.AppendLine(
                    $"Entity \"{entity.Name}\" of type \"{entity.GetType().Name}\" at ({entity.Position.X}, {entity.Position.Y}) uses a reserved entity name.");
            }

            if (problems.Length == 0)
            {
                _ = problems.AppendLine("There are no issues with the current room");
            }

            new ToolWindows.ScrollableMessageBox(this, "Detected problems", problems.ToString(),
                "pack://application:,,,/Resources/MenuIcons/script--exclamation.png").Show();
        }

        private void StartPositionSelection(PropertyInfo targetProperty)
        {
            tileGridDisplay.Cursor = Cursors.Cross;
            selectingPosition = true;
            selectedPositionTarget = targetProperty;
        }

        private void StartEntitySelection(PropertyInfo targetProperty)
        {
            tileGridDisplay.Cursor = Cursors.ScrollNW;
            selectingEntity = true;
            selectedEntityTarget = targetProperty;
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
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                // Prevent horizontal scroll from also scrolling vertically
                e.Handled = true;

                tileMapScroll.ScrollToHorizontalOffset(tileMapScroll.HorizontalOffset - (e.Delta / 2f));
            }
        }

        private void tileGridDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e is { ChangedButton: MouseButton.Left, ButtonState: MouseButtonState.Pressed })
            {
                Point relativeMousePos = e.GetPosition(tileGridDisplay);
                relativeMousePos = new Point(relativeMousePos.X / TileSize.X, relativeMousePos.Y / TileSize.Y);

                if (selectingPosition && selectedPositionTarget is not null && selectedEntity is not null)
                {
                    selectingPosition = false;
                    tileGridDisplay.Cursor = null;

                    PushEntityPropertyEditUndoStack(selectedEntity);

                    selectedPositionTarget.SetValue(selectedEntity,
                        new Microsoft.Xna.Framework.Vector2((float)relativeMousePos.X, (float)relativeMousePos.Y));

                    selectedPositionTarget = null;

                    UpdateSelectedEntity();
                    return;
                }

                lastDrawnPoint = relativeMousePos;
                if (currentToolType == ToolType.Collision)
                {
                    collisionDrawType = !OpenRoom.TileMap[(int)relativeMousePos.X, (int)relativeMousePos.Y].IsCollision;
                }
                switch (currentToolType)
                {
                    case ToolType.Tile:
                    case ToolType.Collision:
                        EditTileAtPosition((int)relativeMousePos.X, (int)relativeMousePos.Y);
                        break;
                    case ToolType.Entity when Keyboard.Modifiers == ModifierKeys.Control:
                        CreateEntityAtPosition((float)relativeMousePos.X, (float)relativeMousePos.Y, true);
                        break;
                    case ToolType.Entity:
                        SelectEntityAtPosition((float)relativeMousePos.X, (float)relativeMousePos.Y);
                        break;
                }
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

            ProcessEntityMove();
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

            if (!OpenRoom.IsOutOfBounds(new Microsoft.Xna.Framework.Vector2((int)relativeMousePos.X, (int)relativeMousePos.Y)))
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

        private void gridOverlayLarger_OnClick(object sender, RoutedEventArgs e)
        {
            GridOverlayEnlarge();
        }

        private void gridOverlaySmaller_OnClick(object sender, RoutedEventArgs e)
        {
            GridOverlayShrink();
        }

        private void DimensionsItem_OnClick(object sender, RoutedEventArgs e)
        {
            PromptDimensionChange();
        }

        private void BackgroundColourItem_OnClick(object sender, RoutedEventArgs e)
        {
            PromptBackgroundColourChange();
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
                // Undo/redo
                case Key.Z when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    if (!Undo())
                    {
                        SystemSounds.Exclamation.Play();
                    }
                    break;
                case Key.Y when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    if (!Redo())
                    {
                        SystemSounds.Exclamation.Play();
                    }
                    break;
                // Other edit options
                case Key.D when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    PromptDimensionChange();
                    break;
                case Key.B when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    PromptBackgroundColourChange();
                    break;
                // File options
                case Key.S when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    Save();
                    break;
                case Key.P when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    ShowProblems();
                    break;
                // Grid
                case Key.G when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    gridOverlayItem.IsChecked = !gridOverlayItem.IsChecked;
                    CreateGridOverlay();
                    break;
                case Key.OemOpenBrackets when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    GridOverlayShrink();
                    break;
                case Key.OemCloseBrackets when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    GridOverlayEnlarge();
                    break;
                // Additional view options
                case Key.E when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    alwaysShowEntitiesItem.IsChecked = !alwaysShowEntitiesItem.IsChecked;
                    UpdateBitmapVisibility();
                    break;
                case Key.C when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    alwaysShowCollisionItem.IsChecked = !alwaysShowCollisionItem.IsChecked;
                    UpdateBitmapVisibility();
                    break;
                case Key.H when e.KeyboardDevice.Modifiers == ModifierKeys.Control:
                    hideInvisibleEntitiesItem.IsChecked = !hideInvisibleEntitiesItem.IsChecked;
                    SelectEntity(null);
                    foreach (Entity entity in OpenRoom.Entities)
                    {
                        DrawEntity(entity, false);
                    }
                    break;
                // Entity edit shortcuts
                case Key.Delete when e.KeyboardDevice.Modifiers == ModifierKeys.Shift:
                    if (selectedEntity is not null)
                    {
                        DeleteEntity(selectedEntity);
                    }
                    break;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Prevent main window from being hidden when this window closes
            _ = ParentWindow.Activate();
        }

        private void toolPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReferenceEquals(toolPanel.SelectedContent, tileTextureScroll))
            {
                currentToolType = ToolType.Tile;
                SelectEntity(null);
            }
            else if (ReferenceEquals(toolPanel.SelectedContent, collisionOptionsPanel))
            {
                currentToolType = ToolType.Collision;
                SelectEntity(null);
            }
            else if (ReferenceEquals(toolPanel.SelectedContent, entityPropertiesGrid))
            {
                currentToolType = ToolType.Entity;
            }

            UpdateBitmapVisibility();
        }

        private void selectedEntityContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectedEntity is null)
            {
                return;
            }

            PushEntityMoveUndoStack(selectedEntity);

            movingEntity = true;
            moveStartOffset = Mouse.GetPosition(selectedEntityOrigin);
            moveStartOffset.X -= selectedEntityOrigin.Width / 2;
            moveStartOffset.Y -= selectedEntityOrigin.Width / 2;
        }

        private void selectedEntityContainer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            movingEntity = false;
            UpdateEntityPropertiesPanel();
        }

        private void selectedEntityContainer_MouseMove(object sender, MouseEventArgs e)
        {
            ProcessEntityMove();
        }

        private void tileGridDisplay_MouseUp(object sender, MouseButtonEventArgs e)
        {
            movingEntity = false;
            UpdateEntityPropertiesPanel();
        }

        private void hideInvisibleEntitiesItem_OnClick(object sender, RoutedEventArgs e)
        {
            SelectEntity(null);
            foreach (Entity entity in OpenRoom.Entities)
            {
                DrawEntity(entity, false);
            }
        }

        private void alwaysShowEntitiesItem_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateBitmapVisibility();
        }

        private void alwaysShowCollisionItem_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateBitmapVisibility();
        }

        private void ProblemsItem_OnClick(object sender, RoutedEventArgs e)
        {
            ShowProblems();
        }

        private void entityApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEntity is null)
            {
                return;
            }

            PushEntityPropertyEditUndoStack(selectedEntity);

            foreach (PropertyEditBox.PropertyEditBox editBox in
                entityPropertiesPanel.Children.OfType<PropertyEditBox.PropertyEditBox>())
            {
                if (!editBox.IsValueValid)
                {
                    continue;
                }
                editBox.Property.SetValue(selectedEntity, editBox.ObjectValue);
            }

            UpdateSelectedEntity();
        }
    }
}

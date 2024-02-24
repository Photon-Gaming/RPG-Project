using System.IO;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace RPGLevelEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string ConfigFileName = "editor_config.json";
        public const string TextureFolderName = "Textures";

        public Config EditorConfig { get; set; }

        private readonly string configPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(configPath))
            {
                try
                {
                    Config? deserialized = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
                    if (deserialized is not null)
                    {
                        EditorConfig = deserialized;
                    }
                }
                catch
                {
                    _ = MessageBox.Show(this, "Config file could not be read. Default settings will be used.",
                        "Config File Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            if (EditorConfig is null)
            {
                EditorConfig = new Config();
                _ = MessageBox.Show(this, "New config file created. Please configure the path to the game Content folder.",
                    "First run", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            if (EditorConfig.ContentFolderPath == "")
            {
                PromptSetContentPath();
            }
        }

        private void PromptSetContentPath()
        {
            OpenFolderDialog dialog = new()
            {
                ValidateNames = true,
                Title = @"Select the RPGGame\Content folder"
            };
            if (dialog.ShowDialog() ?? false)
            {
                EditorConfig.ContentFolderPath = dialog.FolderName;
            }
        }

        private void OpenRoomItem_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "JSON files (*.json)|*.json",
                Title = "Open Room"
            };
            if (dialog.ShowDialog() ?? false)
            {
                new RoomEditor(dialog.FileName, this).Show();
            }
        }

        private void NewRoomItem_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new()
            {
                CheckFileExists = false,
                CheckPathExists = true,
                AddExtension = true,
                Filter = "JSON file (*.json)|*.json",
                Title = "Create Room"
            };
            if (dialog.ShowDialog() ?? false)
            {
                new RoomEditor(dialog.FileName, this, true).Show();
            }
        }

        private void ContentPathItem_OnClick(object sender, RoutedEventArgs e)
        {
            PromptSetContentPath();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            File.WriteAllText(ConfigFileName, JsonConvert.SerializeObject(EditorConfig, Formatting.Indented));
        }
    }
}

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
                _ = MessageBox.Show(this, "New config file created. Please configure the paths to the game resources.",
                    "First run", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenRoomItem_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                CheckFileExists = true,
                CheckPathExists = true
            };
            if (dialog.ShowDialog() ?? false)
            {
                new RoomEditor(dialog.FileName, this).Show();
            }
        }

        private void TexturePathItem_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new()
            {
                ValidateNames = true
            };
            if (dialog.ShowDialog() ?? false)
            {
                EditorConfig.TextureFolderPath = dialog.FolderName;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            File.WriteAllText(ConfigFileName, JsonConvert.SerializeObject(EditorConfig, Formatting.Indented));
        }
    }
}

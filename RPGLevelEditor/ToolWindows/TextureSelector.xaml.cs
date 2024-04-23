using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RPGLevelEditor.ToolWindows
{
    /// <summary>
    /// Interaction logic for TextureSelector.xaml
    /// </summary>
    public partial class TextureSelector : Window
    {
        public string TextureFolder { get; }

        public string? SelectedTextureName { get; private set; }

        public TextureSelector(string textureFolder)
        {
            TextureFolder = textureFolder;

            InitializeComponent();

            foreach (string filepath in Directory.EnumerateFiles(TextureFolder, "*.png"))
            {
                string filename = Path.GetFileNameWithoutExtension(filepath);

                Image textureImage = new()
                {
                    Source = new BitmapImage(new Uri(filepath)),
                    Margin = new Thickness(5),
                    Width = 50,
                    Height = 50
                };
                RenderOptions.SetBitmapScalingMode(textureImage, BitmapScalingMode.NearestNeighbor);

                textureImage.MouseUp += (_, _) => SelectTexture(filepath, filename);

                _ = texturesPanel.Children.Add(textureImage);
            }
        }

        private void SelectTexture(string texturePath, string textureName)
        {
            SelectedTextureName = textureName;
            selectedImagePreview.Source = new BitmapImage(new Uri(texturePath));
            selectedImageInfo.Text = $"{textureName}\n{texturePath}";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

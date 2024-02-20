using System.Windows;
using Microsoft.Win32;

namespace RPGLevelEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
    }
}

using System.Windows;
using System.Windows.Media.Imaging;

namespace RPGLevelEditor.ToolWindows
{
    /// <summary>
    /// Interaction logic for ScrollableMessageBox.xaml
    /// </summary>
    public partial class ScrollableMessageBox : Window
    {
        public ScrollableMessageBox(Window parent, string title, string message, string? icon = null)
        {
            InitializeComponent();

            Owner = parent;
            Title = title;
            textBlock.Text = message;

            if (icon is not null)
            {
                Icon = new BitmapImage(new Uri(icon));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

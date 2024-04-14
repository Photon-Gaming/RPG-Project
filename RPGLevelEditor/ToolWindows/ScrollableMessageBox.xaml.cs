using System.Windows;

namespace RPGLevelEditor.ToolWindows
{
    /// <summary>
    /// Interaction logic for ScrollableMessageBox.xaml
    /// </summary>
    public partial class ScrollableMessageBox : Window
    {
        public ScrollableMessageBox(Window parent, string title, string message)
        {
            InitializeComponent();

            Owner = parent;
            Title = title;
            textBlock.Text = message;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

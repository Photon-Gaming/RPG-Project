using System.Windows;
using System.Windows.Controls;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for DeleteButtonContainer.xaml
    /// </summary>
    public partial class DeleteButtonContainer : UserControl
    {
        public UIElement Child { get; }

        public event RoutedEventHandler? Delete;

        public DeleteButtonContainer(UIElement child)
        {
            InitializeComponent();

            containerGrid.Children.Add(child);
            Child = child;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Delete?.Invoke(this, e);
        }
    }
}

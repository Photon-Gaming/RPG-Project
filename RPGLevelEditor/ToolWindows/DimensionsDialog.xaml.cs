using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RPGLevelEditor.ToolWindows
{
    /// <summary>
    /// Interaction logic for DimensionsDialog.xaml
    /// </summary>
    public partial class DimensionsDialog : Window
    {
        public int X => (int)xSlider.Value;
        public int Y => (int)ySlider.Value;

        public DimensionsDialog(int initX, int initY)
        {
            InitializeComponent();

            xSlider.Value = initX;
            ySlider.Value = initY;
            xTextBox.Text = initX.ToString();
            yTextBox.Text = initY.ToString();
        }

        public DimensionsDialog() : this(1, 1) { }

        private void xSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsInitialized)
            {
                return;
            }

            xTextBox.Text = ((int)xSlider.Value).ToString();
        }

        private void ySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!IsInitialized)
            {
                return;
            }

            yTextBox.Text = ((int)ySlider.Value).ToString();
        }

        private void xTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            if (int.TryParse(xTextBox.Text, out int value))
            {
                xTextBox.Background = null;
                xSlider.Value = value;
            }
            else
            {
                xTextBox.Background = Brushes.Salmon;
            }
        }

        private void yTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            if (int.TryParse(yTextBox.Text, out int value))
            {
                yTextBox.Background = null;
                ySlider.Value = value;
            }
            else
            {
                yTextBox.Background = Brushes.Salmon;
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

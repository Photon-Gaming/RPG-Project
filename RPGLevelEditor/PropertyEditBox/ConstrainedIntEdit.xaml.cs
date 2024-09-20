using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for ConstrainedIntEdit.xaml
    /// </summary>
    public sealed partial class ConstrainedIntEdit : ConstrainedNumericPropertyEditBox<int>
    {
        private bool doTextBoxUpdate = true;

        public override string LabelText
        {
            get => propertyName.Text;
            set => propertyName.Text = value;
        }

        public override string LabelTooltip
        {
            get => (string)propertyName.ToolTip;
            set => propertyName.ToolTip = value;
        }

        public override PropertyInfo? Property { get; init; }

        public override int Value
        {
            get => int.Parse(propertyValue.Text);
            set
            {
                propertyValue.Text = value.ToString();

                doTextBoxUpdate = false;
                propertySlider.Value = value;
                doTextBoxUpdate = true;
            }
        }

        public override object ObjectValue => Value;

        public override Predicate<int> ExtraValidityCheck { get; set; }

        public override bool IsValueValid => int.TryParse(propertyValue.Text, out int value)
            && value >= MinValue && value <= MaxValue
            && ExtraValidityCheck(value);

        public override int MaxValue
        {
            get => (int)propertySlider.Maximum;
            set => propertySlider.Maximum = value;
        }

        public override int MinValue
        {
            get => (int)propertySlider.Minimum;
            set => propertySlider.Minimum = value;
        }

        public ConstrainedIntEdit(string labelText, string labelTooltip, PropertyInfo? property, int initialValue,
            Predicate<int> extraValidityCheck, int maxValue, int minValue)
        {
            ExtraValidityCheck = extraValidityCheck;

            InitializeComponent();

            LabelText = labelText;
            LabelTooltip = labelTooltip;
            Property = property;
            MaxValue = maxValue;
            MinValue = minValue;
            Value = initialValue;
        }

        private void propertyValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValueValid)
            {
                propertyValue.Background = Brushes.White;

                doTextBoxUpdate = false;
                propertySlider.Value = int.Parse(propertyValue.Text);
                doTextBoxUpdate = true;
            }
            else
            {
                propertyValue.Background = Brushes.Salmon;
            }
        }

        private void propertySlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (!doTextBoxUpdate)
            {
                return;
            }

            Value = (int)propertySlider.Value;
        }
    }
}

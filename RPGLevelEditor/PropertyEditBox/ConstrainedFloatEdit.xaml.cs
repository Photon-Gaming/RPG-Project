using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for ConstrainedFloatEdit.xaml
    /// </summary>
    public sealed partial class ConstrainedFloatEdit : ConstrainedNumericPropertyEditBox<float>
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

        public override PropertyInfo Property { get; init; }

        public override float Value
        {
            get => float.Parse(propertyValue.Text);
            set
            {
                propertyValue.Text = value.ToString(CultureInfo.InvariantCulture);

                doTextBoxUpdate = false;
                propertySlider.Value = value;
                doTextBoxUpdate = true;
            }
        }

        public override object ObjectValue => Value;

        public override Predicate<float> ExtraValidityCheck { get; set; }

        public override bool IsValueValid => float.TryParse(propertyValue.Text, out float value)
            && value >= MinValue && value <= MaxValue
            && ExtraValidityCheck(value);

        public override float MaxValue
        {
            get => (float)propertySlider.Maximum;
            set => propertySlider.Maximum = value;
        }

        public override float MinValue
        {
            get => (float)propertySlider.Minimum;
            set => propertySlider.Minimum = value;
        }

        public ConstrainedFloatEdit(string labelText, string labelTooltip, PropertyInfo property, float initialValue,
            Predicate<float> extraValidityCheck, float maxValue, float minValue)
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
                propertySlider.Value = float.Parse(propertyValue.Text);
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

            Value = (float)propertySlider.Value;
        }
    }
}

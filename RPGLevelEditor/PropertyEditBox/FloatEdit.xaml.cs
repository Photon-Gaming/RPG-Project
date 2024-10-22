using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for FloatEdit.xaml
    /// </summary>
    public sealed partial class FloatEdit : PropertyEditBox<float>
    {
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

        public override float Value
        {
            get => float.Parse(propertyValue.Text);
            set => propertyValue.Text = value.ToString(CultureInfo.InvariantCulture);
        }

        public override object ObjectValue => Value;

        public override Predicate<float> ExtraValidityCheck { get; set; }

        public override bool IsValueValid => float.TryParse(propertyValue.Text, out float value) && ExtraValidityCheck(value);

        public FloatEdit(string labelText, string labelTooltip, PropertyInfo? property, float initialValue,
            Predicate<float> extraValidityCheck)
        {
            ExtraValidityCheck = extraValidityCheck;

            InitializeComponent();

            LabelText = labelText;
            LabelTooltip = labelTooltip;
            Property = property;
            Value = initialValue;
        }

        private void propertyValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            propertyValue.Background = !IsValueValid ? Brushes.Salmon : Brushes.White;
        }
    }
}

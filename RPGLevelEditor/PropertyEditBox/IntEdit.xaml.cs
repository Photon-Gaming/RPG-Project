using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for IntEdit.xaml
    /// </summary>
    public sealed partial class IntEdit : PropertyEditBox<int>
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

        public override int Value
        {
            get => int.Parse(propertyValue.Text);
            set => propertyValue.Text = value.ToString(CultureInfo.InvariantCulture);
        }

        public override object ObjectValue => Value;

        public override Predicate<int> ExtraValidityCheck { get; set; }

        public override bool IsValueValid => int.TryParse(propertyValue.Text, out int value) && ExtraValidityCheck(value);

        public IntEdit(string labelText, string labelTooltip, PropertyInfo? property, int initialValue,
            Predicate<int> extraValidityCheck)
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

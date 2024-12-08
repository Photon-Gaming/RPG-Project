using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for ULongEdit.xaml
    /// </summary>
    public sealed partial class ULongEdit : PropertyEditBox<ulong>
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

        public override ulong Value
        {
            get => ulong.Parse(propertyValue.Text);
            set => propertyValue.Text = value.ToString(CultureInfo.InvariantCulture);
        }

        public override object ObjectValue => Value;

        public override Predicate<ulong> ExtraValidityCheck { get; set; }

        public override bool IsValueValid => ulong.TryParse(propertyValue.Text, out ulong value) && ExtraValidityCheck(value);

        public ULongEdit(string labelText, string labelTooltip, PropertyInfo? property, ulong initialValue,
            Predicate<ulong> extraValidityCheck)
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

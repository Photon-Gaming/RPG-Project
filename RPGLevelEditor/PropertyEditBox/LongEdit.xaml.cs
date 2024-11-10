using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for LongEdit.xaml
    /// </summary>
    public sealed partial class LongEdit : PropertyEditBox<long>
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

        public override long Value
        {
            get => long.Parse(propertyValue.Text);
            set => propertyValue.Text = value.ToString(CultureInfo.InvariantCulture);
        }

        public override object ObjectValue => Value;

        public override Predicate<long> ExtraValidityCheck { get; set; }

        public override bool IsValueValid => long.TryParse(propertyValue.Text, out long value) && ExtraValidityCheck(value);

        public LongEdit(string labelText, string labelTooltip, PropertyInfo? property, long initialValue,
            Predicate<long> extraValidityCheck)
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

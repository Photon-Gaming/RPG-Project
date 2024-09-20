using System.Reflection;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for StringEdit.xaml
    /// </summary>
    public sealed partial class StringEdit : PropertyEditBox<string>
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

        public override string Value
        {
            get => propertyValue.Text;
            set => propertyValue.Text = value;
        }

        public override object ObjectValue => Value;

        public override Predicate<string> ExtraValidityCheck { get; set; }

        public override bool IsValueValid => ExtraValidityCheck(Value);

        public StringEdit(string labelText, string labelTooltip, PropertyInfo? property, string initialValue,
            Predicate<string> extraValidityCheck)
        {
            ExtraValidityCheck = extraValidityCheck;

            InitializeComponent();

            LabelText = labelText;
            LabelTooltip = labelTooltip;
            Property = property;
            Value = initialValue;
        }
    }
}

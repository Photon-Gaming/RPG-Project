using System.Reflection;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for BoolEdit.xaml
    /// </summary>
    public sealed partial class BoolEdit : PropertyEditBox<bool>
    {
        public override string LabelText
        {
            get => (string)propertyValue.Content;
            set => propertyValue.Content = value;
        }

        public override string LabelTooltip
        {
            get => (string)propertyValue.ToolTip;
            set => propertyValue.ToolTip = value;
        }

        public override PropertyInfo? Property { get; init; }

        public override bool Value
        {
            get => propertyValue.IsChecked ?? false;
            set => propertyValue.IsChecked = value;
        }

        public override object ObjectValue => Value;

        public override Predicate<bool> ExtraValidityCheck { get; set; }

        public override bool IsValueValid => ExtraValidityCheck(Value);

        public BoolEdit(string labelText, string labelTooltip, PropertyInfo? property, bool initialValue,
            Predicate<bool> extraValidityCheck)
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

using System.Reflection;
using System.Windows.Controls;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for EnumEdit.xaml
    /// </summary>
    public sealed partial class EnumEdit : PropertyEditBox<Enum>
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

        public override Enum Value
        {
            get => (Enum)Enum.Parse(EnumType, propertyValue.Text);
            set => propertyValue.Text = Enum.GetName(EnumType, value);
        }

        public override object ObjectValue => Value;

        public override Predicate<Enum> ExtraValidityCheck { get; set; }

        public override bool IsValueValid => ExtraValidityCheck(Value);

        public Type EnumType { get; }

        public EnumEdit(string labelText, string labelTooltip, PropertyInfo? property, Enum initialValue,
            Predicate<Enum> extraValidityCheck, Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Type given to EnumEdit was not an Enum type");
            }

            EnumType = enumType;

            ExtraValidityCheck = extraValidityCheck;

            InitializeComponent();

            LabelText = labelText;
            LabelTooltip = labelTooltip;
            Property = property;

            foreach (string name in Enum.GetNames(EnumType))
            {
                propertyValue.Items.Add(new ComboBoxItem()
                {
                    Content = name
                });
            }

            Value = initialValue;
        }
    }
}

using System.Reflection;
using System.Windows;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for EntityTextureEdit.xaml
    /// </summary>
    public sealed partial class EntityTextureEdit : EntityTexturePropertyEditBox<string?>
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

        public override PropertyInfo Property { get; init; }

        public override string? Value
        {
            get => propertyValue.Text == "" ? null : propertyValue.Text;
            set => propertyValue.Text = value ?? "";
        }

        public override object? ObjectValue => Value;

        public override Predicate<string?> ExtraValidityCheck { get; set; }

        public override bool IsValueValid => ExtraValidityCheck(Value);

        public override event EventHandler<RoutedEventArgs>? TextureSelectButtonClick;

        public EntityTextureEdit(string labelText, string labelTooltip, PropertyInfo property, string initialValue,
            Predicate<string?> extraValidityCheck)
        {
            ExtraValidityCheck = extraValidityCheck;

            InitializeComponent();

            LabelText = labelText;
            LabelTooltip = labelTooltip;
            Property = property;
            Value = initialValue;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            TextureSelectButtonClick?.Invoke(this, e);
        }
    }
}

using System.Reflection;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for ErrorEdit.xaml
    /// </summary>
    public sealed partial class ErrorEdit : PropertyEditBox<object>
    {
        public override string LabelText
        {
            get => throw new InvalidOperationException();
            set => throw new InvalidOperationException();
        }

        public override string LabelTooltip
        {
            get => throw new InvalidOperationException();
            set => throw new InvalidOperationException();
        }

        public override PropertyInfo? Property { get; init; }

        public override object Value
        {
            get => throw new InvalidOperationException();
            set => throw new InvalidOperationException();
        }

        public override object ObjectValue => Value;

        public override Predicate<object> ExtraValidityCheck
        {
            get => throw new InvalidOperationException();
            set => throw new InvalidOperationException();
        }

        public override bool IsValueValid => false;

        public ErrorEdit(PropertyInfo? property, string errorText)
        {
            InitializeComponent();

            Property = property;
            errorLabel.ToolTip = errorText;
        }
    }
}

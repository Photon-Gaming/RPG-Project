using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Xna.Framework;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for Vector2Edit.xaml
    /// </summary>
    public sealed partial class Vector2Edit : PropertyEditBox<Vector2>
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

        public override Vector2 Value
        {
            get => new(float.Parse(propertyValueX.Text), float.Parse(propertyValueY.Text));
            set
            {
                propertyValueX.Text = value.X.ToString(CultureInfo.InvariantCulture);
                propertyValueY.Text = value.Y.ToString(CultureInfo.InvariantCulture);
            }
        }

        public override object ObjectValue => Value;

        public override Predicate<Vector2> ExtraValidityCheck { get; set; }

        public override bool IsValueValid => float.TryParse(propertyValueX.Text, out float x)
            && float.TryParse(propertyValueX.Text, out float y)
            && ExtraValidityCheck(new Vector2(x, y));

        public Vector2Edit(string labelText, string labelTooltip, PropertyInfo property, Vector2 initialValue,
            Predicate<Vector2> extraValidityCheck)
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
            Brush background = !IsValueValid ? Brushes.Salmon : Brushes.White;
            propertyValueX.Background = background;
            propertyValueY.Background = background;
        }
    }
}

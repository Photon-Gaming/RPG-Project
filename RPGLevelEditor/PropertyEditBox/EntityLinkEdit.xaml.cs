using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for EntityLinkEdit.xaml
    /// </summary>
    public sealed partial class EntityLinkEdit : EntityLinkPropertyEditBox<string>
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

        public override bool IsValueValid =>
            (Value.Equals(RPGGame.GameObject.Entity.Player.PlayerEntityName, StringComparison.OrdinalIgnoreCase)
                || ContainingRoom.Entities.Any(e => e.Name.Equals(Value, StringComparison.OrdinalIgnoreCase)))
            && ExtraValidityCheck(Value);

        public override RPGGame.GameObject.Room ContainingRoom { get; }

        public override event EventHandler<RoutedEventArgs>? EntitySelectButtonClick;

        public EntityLinkEdit(string labelText, string labelTooltip, PropertyInfo? property, string initialValue,
            Predicate<string> extraValidityCheck, RPGGame.GameObject.Room containingRoom,
            IEnumerable<RPGGame.GameObject.Entity.Entity> entities)
        {
            ExtraValidityCheck = extraValidityCheck;

            ContainingRoom = containingRoom;

            InitializeComponent();

            foreach (RPGGame.GameObject.Entity.Entity entity in entities)
            {
                propertyValue.Items.Add(new ComboBoxItem()
                {
                    Content = entity.Name,
                    ToolTip = entity.GetType().Name
                });
            }

            LabelText = labelText;
            LabelTooltip = labelTooltip;
            Property = property;
            Value = initialValue;

            ValidateName();
        }

        private void ValidateName()
        {
            propertyValue.Foreground = IsValueValid ? Brushes.Black : Brushes.Red;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            EntitySelectButtonClick?.Invoke(this, e);
        }

        private void propertyValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateName();
        }
    }
}

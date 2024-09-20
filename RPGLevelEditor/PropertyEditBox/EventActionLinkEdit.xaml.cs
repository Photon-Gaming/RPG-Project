using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RPGGame.GameObject.Entity;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for EventActionLinkEdit.xaml
    /// </summary>
    public sealed partial class EventActionLinkEdit : UserControl
    {
        public string TargetEvent
        {
            get => eventValue.Text;
            set => eventValue.Text = value;
        }

        public string TargetEntity
        {
            get => targetEntityValue.Text;
            set => targetEntityValue.Text = value;
        }

        public string TargetAction
        {
            get => actionValue.Text;
            set => actionValue.Text = value;
        }

        public Entity? TargetEntityInstance =>
            SourceEntity.CurrentRoom?.Entities.FirstOrDefault(e => e.Name.Equals(TargetEntity, StringComparison.OrdinalIgnoreCase));

        public bool IsTargetEntityValid => TargetEntityInstance is not null
            || TargetEntity.Equals(Player.PlayerEntityName, StringComparison.OrdinalIgnoreCase);
        public bool IsTargetActionValid => actionValue.Items.OfType<ComboBoxItem>().Any(i => i.Content is string content && content == TargetAction);

        public bool IsValueValid => IsTargetEntityValid && IsTargetActionValid &&
            parameterEditStack.Children.OfType<PropertyEditBox>().All(b => b.IsValueValid);

        public Entity SourceEntity { get; }
        public RoomEditor EditorWindow { get; }

        public event EventHandler<RoutedEventArgs>? EntitySelectButtonClick;

        public EventActionLinkEdit(string initialEvent, string initialTarget, string initialAction,
            IEnumerable<FiresEventAttribute> events, IEnumerable<Entity> entities, Entity sourceEntity,
            Dictionary<string, object?>? initialParameters, RoomEditor editorWindow)
        {
            SourceEntity = sourceEntity;
            EditorWindow = editorWindow;

            InitializeComponent();

            foreach (FiresEventAttribute possibleEvent in events)
            {
                eventValue.Items.Add(new ComboBoxItem()
                {
                    Content = possibleEvent.Name,
                    ToolTip = possibleEvent.Description
                });
            }
            foreach (Entity entity in entities)
            {
                targetEntityValue.Items.Add(new ComboBoxItem()
                {
                    Content = entity.Name,
                    ToolTip = entity.GetType().Name
                });
            }

            TargetEvent = initialEvent;
            TargetEntity = initialTarget;

            UpdateActionDropdown();
            TargetAction = initialAction;

            ValidateName();

            UpdateParameterEditBoxes(initialParameters);
        }

        public Dictionary<string, object?> GetActionParameters()
        {
            return parameterEditStack.Children.OfType<PropertyEditBox>()
                .ToDictionary(b => b.LabelText, b => b.ObjectValue);
        }

        private void ValidateName()
        {
            if (!IsTargetEntityValid)
            {
                targetEntityValue.Foreground = Brushes.Red;
                actionValue.Foreground = Brushes.Black;
            }
            else if (!IsTargetActionValid)
            {
                targetEntityValue.Foreground = Brushes.Black;
                actionValue.Foreground = Brushes.Red;
            }
            else
            {
                targetEntityValue.Foreground = Brushes.Black;
                actionValue.Foreground = Brushes.Black;
            }
        }

        private void UpdateActionDropdown()
        {
            string currentAction = TargetAction;

            actionValue.Items.Clear();

            if (!IsTargetEntityValid)
            {
                return;
            }

            foreach ((string methodName, ActionMethodAttribute actionAttribute,
                ActionMethodParameterAttribute[] parameterAttributes) in RoomEditor.GetEntityActionMethods(TargetEntityInstance))
            {
                actionValue.Items.Add(new ComboBoxItem()
                {
                    Content = methodName,
                    ToolTip = actionAttribute.Description,
                    Tag = parameterAttributes
                });

                if (methodName == currentAction)
                {
                    // Prevent action method name from being cleared if the action method also exists on the new entity
                    TargetAction = currentAction;
                }
            }
        }

        private void UpdateParameterEditBoxes(Dictionary<string, object?>? initialValues = null)
        {
            // If initial values aren't given,
            // carry the current values of any existing edit boxes to any new edit boxes that match
            initialValues ??= GetActionParameters();

            parameterEditStack.Children.Clear();

            if (!IsTargetActionValid || actionValue.SelectedItem is not ComboBoxItem
                {
                    Tag: ActionMethodParameterAttribute[] parameterAttributes
                })
            {
                return;
            }

            foreach (ActionMethodParameterAttribute parameter in parameterAttributes)
            {
                parameterEditStack.Children.Add(EditorWindow.CreateEditBox(
                    parameter.ParameterType, parameter.EditorEditType,
                    initialValues.GetValueOrDefault(parameter.Name, null),
                    parameter.Description, parameter.Name));
                parameterEditStack.Children.Add(new Separator());
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            EntitySelectButtonClick?.Invoke(this, e);
        }

        private void targetEntityValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateName();
            UpdateActionDropdown();
        }

        private void actionValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateName();
            UpdateParameterEditBoxes();
        }
    }
}

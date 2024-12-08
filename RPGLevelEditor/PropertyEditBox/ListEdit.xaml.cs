using System.Collections;
using System.Reflection;
using System.Windows;

namespace RPGLevelEditor.PropertyEditBox
{
    /// <summary>
    /// Interaction logic for ListEdit.xaml
    /// </summary>
    public sealed partial class ListEdit : PropertyEditBox<IEnumerable>
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

        public override IEnumerable Value
        {
            get
            {
                // Create a new list of the correct type while still keeping the return value non-generic
                IList newList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(ListContentType))!;
                foreach (DeleteButtonContainer container in valuesPanel.Children.OfType<DeleteButtonContainer>())
                {
                    if (container.Child is PropertyEditBox box)
                    {
                        newList.Add(box.ObjectValue);
                    }
                }
                return newList;
            }
            set
            {
                valuesPanel.Children.Clear();

                int i = 0;
                foreach (object obj in value)
                {
                    AddNewEditBox(i, obj);
                    i++;
                }
            }
        }

        public override object ObjectValue => Value;

        public override Predicate<IEnumerable> ExtraValidityCheck { get; set; }

        public override bool IsValueValid => ExtraValidityCheck(Value);

        public Type ListContentType { get; }
        public RPGGame.GameObject.Entity.EditType ListEditType { get; }

        private readonly RoomEditor parentWindow;

        public ListEdit(string labelText, string labelTooltip, PropertyInfo? property, IEnumerable initialValues,
            Predicate<IEnumerable> extraValidityCheck, Type listContentType, RPGGame.GameObject.Entity.EditType listEditType,
            RoomEditor editor)
        {
            ListContentType = listContentType;
            ListEditType = listEditType;

            parentWindow = editor;

            ExtraValidityCheck = extraValidityCheck;

            InitializeComponent();

            LabelText = labelText;
            LabelTooltip = labelTooltip;
            Property = property;

            Value = initialValues;
        }

        private void AddNewEditBox(int index, object? value)
        {
            DeleteButtonContainer container = new(
                parentWindow.CreateEditBox(ListContentType, ListEditType, value,
                    $"Item at index {index} of the list", index.ToString()))
            {
                Tag = index
            };

            container.Delete += Container_Delete;

            valuesPanel.Children.Add(container);
        }

        private void Container_Delete(object sender, RoutedEventArgs e)
        {
            if (sender is not DeleteButtonContainer { Tag: int index })
            {
                return;
            }

            valuesPanel.Children.RemoveAt(index);

            // Update the indices of all values after the deleted one
            for (int i = index; i < valuesPanel.Children.Count; i++)
            {
                DeleteButtonContainer container = (DeleteButtonContainer)valuesPanel.Children[i];
                container.Tag = i;
                if (container.Child is PropertyEditBox box)
                {
                    box.LabelText = i.ToString();
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewEditBox(valuesPanel.Children.Count, null);
        }
    }
}

using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using RPGGame.GameObject.Entity;

namespace RPGLevelEditor.ToolWindows
{
    /// <summary>
    /// Interaction logic for EntityClassSelector.xaml
    /// </summary>
    public partial class EntityClassSelector : Window
    {
        public Type? SelectedEntityClass => (selectionTree.SelectedItem as TreeViewItem)?.Tag as Type;

        private readonly Dictionary<string, TreeViewItem> treeCategories = new();
        private readonly Dictionary<string, int> immediateSubcategoryCount = new();

        public EntityClassSelector()
        {
            InitializeComponent();

            PopulateSelectionTree();
        }

        private ItemsControl GetOrCreateTreeCategory(string categories)
        {
            if (categories == "")
            {
                // An empty categories list should return the root,
                // i.e. the selection tree control itself
                return selectionTree;
            }

            if (treeCategories.TryGetValue(categories, out TreeViewItem? item))
            {
                return item;
            }

            int dotIndex = categories.LastIndexOf('.');

            string parentCategories = dotIndex == -1 ? "" : categories[..dotIndex];
            string immediateCategory = categories[(dotIndex + 1)..];

            TreeViewItem newItem = new()
            {
                Header = immediateCategory,
                FontWeight = FontWeights.Bold,
                Focusable = false  // Prevent categories from being selected directly
            };

            // Sort categories before selectable items
            _ = immediateSubcategoryCount.TryAdd(parentCategories, 0);
            GetOrCreateTreeCategory(parentCategories).Items.Insert(immediateSubcategoryCount[parentCategories]++, newItem);
            treeCategories[categories] = newItem;

            return newItem;
        }

        private void PopulateSelectionTree()
        {
            foreach ((Type entityClass, EditorEntityAttribute attribute) in GetEntityClasses())
            {
                GetOrCreateTreeCategory(attribute.Categories).Items.Add(new TreeViewItem()
                {
                    Header = entityClass.Name != attribute.Name
                        ? $"{attribute.Name} ({entityClass.Name})"
                        : attribute.Name,
                    FontWeight = FontWeights.Normal,
                    ToolTip = attribute.Description,
                    Tag = entityClass
                });
            }
        }

        private static (Type EntityClass, EditorEntityAttribute Attribute)[] GetEntityClasses()
        {
            List<(Type EntityClass, EditorEntityAttribute Attribute)> entityClasses = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    Attribute? attribute =
                        type.GetCustomAttributes(typeof(EditorEntityAttribute)).FirstOrDefault(defaultValue: null);

                    if (attribute is not EditorEntityAttribute entityAttribute)
                    {
                        continue;
                    }

                    entityClasses.Add((type, entityAttribute));
                }
            }

            return entityClasses
                .OrderBy(c => c.Attribute.Categories, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(c => c.Attribute.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();
        }

        private void selectionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            okButton.IsEnabled = true;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

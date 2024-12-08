using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace RPGLevelEditor.ToolWindows
{
    /// <summary>
    /// Interaction logic for TypeSelector.xaml
    /// </summary>
    public partial class TypeSelector : Window
    {
        public Type? SelectedType => (selectionTree.SelectedItem as TreeViewItem)?.Tag as Type;

        private readonly Type? genericTypeConstraint;

        private readonly Dictionary<string, TreeViewItem> treeCategories = new();
        private readonly Dictionary<Type, TreeViewItem> typeTreeItems = new();
        private readonly Dictionary<string, int> immediateNamespaceCount = new();

        public TypeSelector(Type? genericTypeConstraint)
        {
            this.genericTypeConstraint = genericTypeConstraint;

            InitializeComponent();

            PopulateSelectionTree();
        }

        private ItemsControl GetOrCreateTreeNamespace(string namespaces)
        {
            if (namespaces == "")
            {
                // An empty namespaces list should return the root,
                // i.e. the selection tree control itself
                return selectionTree;
            }

            if (treeCategories.TryGetValue(namespaces, out TreeViewItem? item))
            {
                return item;
            }

            int dotIndex = namespaces.LastIndexOf('.');

            string parentNamespaces = dotIndex == -1 ? "" : namespaces[..dotIndex];
            string immediateNamespace = namespaces[(dotIndex + 1)..];

            TreeViewItem newItem = new()
            {
                Header = immediateNamespace,
                FontWeight = FontWeights.Bold,
                Focusable = false  // Prevent namespaces from being selected directly
            };

            // Sort namespaces before selectable items
            _ = immediateNamespaceCount.TryAdd(parentNamespaces, 0);
            GetOrCreateTreeNamespace(parentNamespaces).Items.Insert(immediateNamespaceCount[parentNamespaces]++, newItem);
            treeCategories[namespaces] = newItem;

            return newItem;
        }

        private void PopulateSelectionTree()
        {
            Type[] allValidTypes = GetAllValidTypes();
            HashSet<Type> validTypeSet = new(allValidTypes);

            Queue<Type> typeQueue = new();

            foreach (Type type in allValidTypes)
            {
                typeQueue.Enqueue(type);
            }

            while (typeQueue.TryDequeue(out Type? type))
            {
                if (!typeTreeItems.TryGetValue(type, out TreeViewItem? newItem))
                {
                    newItem = new TreeViewItem()
                    {
                        Header = type.Name,
                        FontWeight = FontWeights.Normal,
                        ToolTip = type.AssemblyQualifiedName,
                        Tag = type
                    };
                    typeTreeItems[type] = newItem;
                }

                if (type.DeclaringType is not null)
                {
                    if (typeTreeItems.TryGetValue(type.DeclaringType, out TreeViewItem? parentItem))
                    {
                        parentItem.Items.Add(newItem);
                    }
                    else
                    {
                        if (!validTypeSet.Contains(type.DeclaringType))
                        {
                            // Outer class isn't valid so will never be added - treat it like a namespace instead
                            GetOrCreateTreeNamespace(type.Namespace ?? "<no namespace>" + GetDeclaringClassNames(type)).Items.Add(newItem);
                        }
                        else
                        {
                            // Outer class hasn't been added yet but will be at some point - try again later
                            typeQueue.Enqueue(type);
                        }
                    }
                }
                else
                {
                    GetOrCreateTreeNamespace(type.Namespace ?? "<no namespace>").Items.Add(newItem);
                }
            }
        }

        private static string GetDeclaringClassNames(Type type)
        {
            StringBuilder builder = new();

            while (true)
            {
                if (type.DeclaringType is null)
                {
                    break;
                }
                type = type.DeclaringType;
                builder.Insert(0, '.').Insert(0, type.Name);
            }

            return builder.ToString();
        }

        private Type[] GetAllValidTypes()
        {
            List<Type> types = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type potentialType in assembly.GetTypes())
                {
                    if (potentialType.IsGenericType)
                    {
                        // Do not allow nested generics
                        continue;
                    }

                    if (genericTypeConstraint is not null)
                    {
                        try
                        {
                            _ = genericTypeConstraint.MakeGenericType(potentialType);
                        }
                        catch (ArgumentException)
                        {
                            // Type is not a valid type parameter for the entity class
                            continue;
                        }
                    }

                    types.Add(potentialType);
                }
            }

            return types
                .OrderBy(t => t.Namespace, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(GetDeclaringClassNames, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(t => t.Name)
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

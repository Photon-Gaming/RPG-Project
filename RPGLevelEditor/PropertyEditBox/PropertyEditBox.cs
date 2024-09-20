using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace RPGLevelEditor.PropertyEditBox
{
    public abstract class PropertyEditBox : UserControl
    {
        public abstract string LabelText { get; set; }
        public abstract string LabelTooltip { get; set; }
        public abstract PropertyInfo? Property { get; init; }
        public abstract bool IsValueValid { get; }
        public abstract object? ObjectValue { get; }
    }

    public abstract class PropertyEditBox<T> : PropertyEditBox
    {
        public abstract T Value { get; set; }
        public abstract Predicate<T> ExtraValidityCheck { get; set; }
    }

    public abstract class ConstrainedNumericPropertyEditBox<T> : PropertyEditBox<T>
    {
        public abstract T MaxValue { get; set; }
        public abstract T MinValue { get; set; }
    }

    public abstract class RoomCoordinatePropertyEditBox<T> : PropertyEditBox<T>
    {
        public abstract RPGGame.GameObject.Room Room { get; init; }

        public abstract event EventHandler<RoutedEventArgs>? CoordinateSelectButtonClick;
    }

    public abstract class EntityTexturePropertyEditBox<T> : PropertyEditBox<T>
    {
        public abstract event EventHandler<RoutedEventArgs>? TextureSelectButtonClick;
    }

    public abstract class EntityLinkPropertyEditBox<T> : PropertyEditBox<T>
    {
        public abstract RPGGame.GameObject.Room ContainingRoom { get; }

        public abstract event EventHandler<RoutedEventArgs>? EntitySelectButtonClick;
    }
}

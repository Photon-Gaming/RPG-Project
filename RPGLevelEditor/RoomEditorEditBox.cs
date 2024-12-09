using System.Collections;
using System.Reflection;
using RPGGame.GameObject.Entity;

namespace RPGLevelEditor
{
    public partial class RoomEditor
    {
        public PropertyEditBox.PropertyEditBox CreatePropertyEditBox(PropertyInfo property,
            EditorModifiableAttribute editorAttribute, Entity entity)
        {
            string labelText = editorAttribute.Name == property.Name
                ? editorAttribute.Name
                : $"{editorAttribute.Name} ({property.Name})";

            return CreateEditBox(property.PropertyType, editorAttribute.EditorEditType, property.GetValue(entity),
                editorAttribute.Description, labelText, property);
        }

        public PropertyEditBox.PropertyEditBox CreateEditBox(Type propertyType, EditType editType, object? initialValue,
            string description, string labelText, PropertyInfo? property = null)
        {
            // The EditType of a list affects the list contents, not the list itself
            if (propertyType.IsConstructedGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type listContentType = propertyType.GenericTypeArguments[0];
                return new PropertyEditBox.ListEdit(
                    labelText, description, property,
                    (IEnumerable)(initialValue ?? Enumerable.Empty<object>()), _ => true, listContentType, editType, this);
            }

            switch (editType)
            {
                case EditType.Default:
                default:
                    if (propertyType == typeof(float))
                    {
                        return new PropertyEditBox.FloatEdit(
                            labelText, description, property,
                            (float)(initialValue ?? 0f), _ => true);
                    }
                    if (propertyType == typeof(int))
                    {
                        return new PropertyEditBox.IntEdit(
                            labelText, description, property,
                            (int)(initialValue ?? 0), _ => true);
                    }
                    if (propertyType == typeof(long))
                    {
                        return new PropertyEditBox.LongEdit(
                            labelText, description, property,
                            (long)(initialValue ?? 0L), _ => true);
                    }
                    if (propertyType == typeof(ulong))
                    {
                        return new PropertyEditBox.ULongEdit(
                            labelText, description, property,
                            (ulong)(initialValue ?? 0UL), _ => true);
                    }
                    if (propertyType == typeof(string))
                    {
                        return new PropertyEditBox.StringEdit(
                            labelText, description, property,
                            (string)(initialValue ?? ""), _ => true);
                    }
                    if (propertyType == typeof(bool))
                    {
                        return new PropertyEditBox.BoolEdit(
                            labelText, description, property,
                            (bool)(initialValue ?? false), _ => true);
                    }
                    if (propertyType == typeof(Microsoft.Xna.Framework.Vector2))
                    {
                        return new PropertyEditBox.Vector2Edit(
                            labelText, description, property,
                            (Microsoft.Xna.Framework.Vector2)(initialValue ?? new Microsoft.Xna.Framework.Vector2()), _ => true);
                    }
                    if (propertyType == typeof(TimeSpan))
                    {
                        return new PropertyEditBox.TimeSpanEdit(
                            labelText, description, property,
                            (TimeSpan)(initialValue ?? TimeSpan.Zero), _ => true);
                    }
                    if (propertyType.IsEnum)
                    {
                        return new PropertyEditBox.EnumEdit(
                            labelText, description, property,
                            (Enum)(initialValue ?? Enum.ToObject(propertyType, 0)), _ => true, propertyType);
                    }
                    break;
                case EditType.ConstrainedNumeric:
                    if (property is null)
                    {
                        return new PropertyEditBox.ErrorEdit(property,
                            "The constrained numeric edit type can only be used on properties, not parameters.");
                    }
                    if (propertyType == typeof(float))
                    {
                        List<EditorFloatConstraintAttribute> constraintAttributes =
                            property.GetCustomAttributes(typeof(EditorFloatConstraintAttribute))
                                .Cast<EditorFloatConstraintAttribute>().ToList();
                        if (constraintAttributes.Count == 0)
                        {
                            return new PropertyEditBox.ErrorEdit(property,
                                $"{property.Name} property is missing EditorFloatConstraint attribute");
                        }
                        EditorFloatConstraintAttribute constraintAttribute = constraintAttributes[0];

                        return new PropertyEditBox.ConstrainedFloatEdit(
                            labelText, description, property,
                            (float)(initialValue ?? 0), _ => true,
                            constraintAttribute.Maximum, constraintAttribute.Minimum);
                    }
                    if (propertyType == typeof(int))
                    {
                        List<EditorIntConstraintAttribute> constraintAttributes =
                            property.GetCustomAttributes(typeof(EditorIntConstraintAttribute))
                                .Cast<EditorIntConstraintAttribute>().ToList();
                        if (constraintAttributes.Count == 0)
                        {
                            return new PropertyEditBox.ErrorEdit(property,
                                $"{property.Name} property is missing EditorIntConstraint attribute");
                        }
                        EditorIntConstraintAttribute constraintAttribute = constraintAttributes[0];

                        return new PropertyEditBox.ConstrainedIntEdit(
                            labelText, description, property,
                            (int)(initialValue ?? 0), _ => true,
                            constraintAttribute.Maximum, constraintAttribute.Minimum);
                    }
                    break;
                case EditType.RoomCoordinate:
                    if (propertyType == typeof(Microsoft.Xna.Framework.Vector2))
                    {
                        PropertyEditBox.RoomCoordinateEdit editBox = new(
                            labelText, description, property,
                            (Microsoft.Xna.Framework.Vector2)(initialValue ?? new Microsoft.Xna.Framework.Vector2()), _ => true,
                            OpenRoom);

                        editBox.CoordinateSelectButtonClick += (_, _) => StartPositionSelection(editBox);

                        return editBox;
                    }
                    break;
                case EditType.EntityTexture:
                    if (propertyType == typeof(string))
                    {
                        PropertyEditBox.EntityTextureEdit editBox = new(
                            labelText, description, property,
                            (string?)initialValue, _ => true);

                        editBox.TextureSelectButtonClick += (_, _) =>
                        {
                            ToolWindows.TextureSelector selector = new(EntityTextureFolderPath);
                            if ((selector.ShowDialog() ?? false) && selector.SelectedTextureName is not null)
                            {
                                editBox.Value = selector.SelectedTextureName;
                            }
                        };

                        return editBox;
                    }
                    break;
                case EditType.EntityLink:
                    if (propertyType == typeof(string))
                    {
                        PropertyEditBox.EntityLinkEdit editBox = new(
                            labelText, description, property,
                            (string)(initialValue ?? ""), _ => true,
                            OpenRoom, OpenRoom.Entities);

                        editBox.EntitySelectButtonClick += (_, _) => StartEntitySelection(
                            typeof(PropertyEditBox.EntityLinkEdit).GetProperty("Value")!, editBox);

                        return editBox;
                    }
                    break;
            }

            return new PropertyEditBox.ErrorEdit(property,
                $"A {editType} edit box has not been defined for properties with type {propertyType}");
        }
    }
}

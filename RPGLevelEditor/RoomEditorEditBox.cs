using System.Reflection;
using RPGGame.GameObject.Entity;

namespace RPGLevelEditor
{
    public partial class RoomEditor
    {
        public PropertyEditBox.PropertyEditBox CreatePropertyEditBox(PropertyInfo property,
            EditorModifiableAttribute editorAttribute,
            Entity entity)
        {
            string labelText = $"{editorAttribute.Name} ({property.Name})";

            switch (editorAttribute.EditorEditType)
            {
                case EditType.Default:
                default:
                    if (property.PropertyType == typeof(float))
                    {
                        return new PropertyEditBox.FloatEdit(
                            labelText, editorAttribute.Description, property,
                            (float)property.GetValue(entity)!, _ => true);
                    }
                    if (property.PropertyType == typeof(int))
                    {
                        return new PropertyEditBox.IntEdit(
                            labelText, editorAttribute.Description, property,
                            (int)property.GetValue(entity)!, _ => true);
                    }
                    if (property.PropertyType == typeof(string))
                    {
                        return new PropertyEditBox.StringEdit(
                            labelText, editorAttribute.Description, property,
                            (string)property.GetValue(entity)!, _ => true);
                    }
                    if (property.PropertyType == typeof(Microsoft.Xna.Framework.Vector2))
                    {
                        return new PropertyEditBox.Vector2Edit(
                            labelText, editorAttribute.Description, property,
                            (Microsoft.Xna.Framework.Vector2)property.GetValue(entity)!, _ => true);
                    }
                    break;
                case EditType.ConstrainedNumeric:
                    if (property.PropertyType == typeof(float))
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
                            labelText, editorAttribute.Description, property,
                            (float)property.GetValue(entity)!, _ => true,
                            constraintAttribute.Maximum, constraintAttribute.Minimum);
                    }
                    if (property.PropertyType == typeof(int))
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
                            labelText, editorAttribute.Description, property,
                            (int)property.GetValue(entity)!, _ => true,
                            constraintAttribute.Maximum, constraintAttribute.Minimum);
                    }
                    break;
                case EditType.RoomCoordinate:
                    if (property.PropertyType == typeof(Microsoft.Xna.Framework.Vector2))
                    {
                        // TODO: Subscribe to coordinate picker button event

                        return new PropertyEditBox.RoomCoordinateEdit(
                            labelText, editorAttribute.Description, property,
                            (Microsoft.Xna.Framework.Vector2)property.GetValue(entity)!, _ => true,
                            OpenRoom);
                    }
                    break;
                case EditType.EntityTexture:
                    if (property.PropertyType == typeof(string))
                    {
                        // TODO: Subscribe to texture browser button event

                        return new PropertyEditBox.EntityTextureEdit(
                            labelText, editorAttribute.Description, property,
                            (string)property.GetValue(entity)!, _ => true);
                    }
                    break;
            }

            return new PropertyEditBox.ErrorEdit(property,
                $"A {editorAttribute.EditorEditType} edit box has not been defined for properties with type {property.PropertyType}");
        }
    }
}

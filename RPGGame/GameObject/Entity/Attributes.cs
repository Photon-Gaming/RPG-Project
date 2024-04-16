using System;

namespace RPGGame.GameObject.Entity
{
    public enum EditType
    {
        Default,
        RoomCoordinate,
        EntityTexture,
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class EditorModifiableAttribute(string name, string description, EditType editorEditType = EditType.Default) : Attribute
    {
        public string Name = name;
        public string Description = description;
        public EditType EditorEditType = editorEditType;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class EditorFloatConstraintAttribute(float? min, float? max) : Attribute
    {
        public float? Minimum = min;
        public float? Maximum = max;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class EditorIntConstraintAttribute(int? min, int? max) : Attribute
    {
        public int? Minimum = min;
        public int? Maximum = max;
    }
}

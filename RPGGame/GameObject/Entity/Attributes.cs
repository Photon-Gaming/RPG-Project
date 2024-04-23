using System;

namespace RPGGame.GameObject.Entity
{
    public enum EditType
    {
        Default,
        RoomCoordinate,
        EntityTexture,
        ConstrainedNumeric,
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class EditorModifiableAttribute(string name, string description, EditType editorEditType = EditType.Default) : Attribute
    {
        public readonly string Name = name;
        public readonly string Description = description;
        public readonly EditType EditorEditType = editorEditType;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class EditorFloatConstraintAttribute(float min, float max) : Attribute
    {
        public readonly float Minimum = min;
        public readonly float Maximum = max;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class EditorIntConstraintAttribute(int min, int max) : Attribute
    {
        public readonly int Minimum = min;
        public readonly int Maximum = max;
    }
}

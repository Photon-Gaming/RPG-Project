using System;

namespace RPGGame.GameObject.Entity
{
    // Editor property edits

    public enum EditType
    {
        Default,
        RoomCoordinate,
        EntityTexture,
        ConstrainedNumeric,
        EntityLink,
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

    // Event->Action system

    [AttributeUsage(AttributeTargets.Method)]
    public class ActionMethodAttribute(string description) : Attribute
    {
        public readonly string Description = description;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ActionMethodParameterAttribute(string name, Type parameterType, EditType editorEditType = EditType.Default) : Attribute
    {
        public readonly string Name = name;
        public readonly Type ParameterType = parameterType;
        public readonly EditType EditorEditType = editorEditType;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FiresEventAttribute(string name, string description) : Attribute
    {
        public readonly string Name = name;
        public readonly string Description = description;
    }
}

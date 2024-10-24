using JetBrains.Annotations;
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

    /// <summary>
    /// Makes an entity class selectable when creating a new entity in the editor.
    /// The use of this attribute is not automatically inherited by derived classes.
    /// </summary>
    /// <remarks>
    /// Applied classes must be public, instantiatable (i.e. not static or abstract),
    /// and inherit either directly or indirectly from <see cref="Entity"/>.
    /// They also must have the default <see cref="Entity.Entity"/> constructor available.
    /// </remarks>
    /// <param name="name">
    /// The friendly name to display in the editor. If different from the class name, both will be displayed.
    /// </param>
    /// <param name="description">
    /// An extended description of the entity class, displayed by the editor to give additional information.
    /// </param>
    /// <param name="categories">
    /// A dot-separated (.) hierarchical list of categories that this entity is in,
    /// which will determine where in the class selection tree view the class will be placed.
    /// For example Cat1.Cat2.Cat3 will render as:<br/>
    /// Cat1<br/>
    /// ├Cat2<br/>
    /// │├Cat3<br/>
    /// ││├Class
    /// </param>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EditorEntityAttribute(string name, string description, string categories) : Attribute
    {
        public readonly string Name = name;
        public readonly string Description = description;
        public readonly string Categories = categories;
    }

    /// <summary>
    /// Makes an entity property editable in the editor properties panel.
    /// </summary>
    /// <remarks>
    /// Applied properties must have a public getter and a non-private (e.g. public, internal, protected) setter.
    /// Should be combined with <see cref="Newtonsoft.Json.JsonPropertyAttribute"/>.
    /// </remarks>
    /// <param name="name">
    /// The friendly name to display in the editor. If different from the property name, both will be displayed.
    /// </param>
    /// <param name="description">
    /// An extended description of the property, displayed by the editor to give additional information.
    /// </param>
    /// <param name="editorEditType">
    /// An optional special edit type to use for the property.
    /// Will affect how the property is edited by the user in the editor.
    /// </param>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditorModifiableAttribute(string name, string description, EditType editorEditType = EditType.Default) : Attribute
    {
        public readonly string Name = name;
        public readonly string Description = description;
        public readonly EditType EditorEditType = editorEditType;
    }

    /// <summary>
    /// Used to set the constraints for <see cref="float"/> properties using the
    /// <see cref="EditType.ConstrainedNumeric"/> edit type.
    /// </summary>
    /// <remarks>
    /// Must be combined with <see cref="EditorModifiableAttribute"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditorFloatConstraintAttribute(float min, float max) : Attribute
    {
        public readonly float Minimum = min;
        public readonly float Maximum = max;
    }

    /// <summary>
    /// Used to set the constraints for <see cref="int"/> properties using the
    /// <see cref="EditType.ConstrainedNumeric"/> edit type.
    /// </summary>
    /// <remarks>
    /// Must be combined with <see cref="EditorModifiableAttribute"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class EditorIntConstraintAttribute(int min, int max) : Attribute
    {
        public readonly int Minimum = min;
        public readonly int Maximum = max;
    }

    // Event->Action system

    /// <summary>
    /// Marks a method as an action method so that it is displayed in the list of available action methods in the editor.
    /// </summary>
    /// <remarks>
    /// Applied methods must conform to the signature of the <see cref="ActionMethod"/> delegate.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public class ActionMethodAttribute(string description) : Attribute
    {
        public readonly string Description = description;
    }

    /// <summary>
    /// Specifies each parameter taken by the action method so that they can be set from the editor.
    /// A separate instance of this attribute should be used for each required parameter.
    /// The use of this attribute does not automatically verify that the parameter will actually be given,
    /// nor that it will necessarily be of the correct type;
    /// such validation must be done in the method body.
    /// </summary>
    /// <remarks>
    /// Must be combined with <see cref="ActionMethodAttribute"/>.
    /// </remarks>
    /// <param name="name">
    /// The key in the parameter dictionary that should be used for the parameter.
    /// </param>
    /// <param name="description">
    /// A description of the parameter to show in the editor.
    /// </param>
    /// <param name="parameterType">
    /// The <see cref="Type"/> that the value of the parameter should be.
    /// </param>
    /// <param name="editorEditType">
    /// An optional special edit type to use for the parameter.
    /// Will affect how the parameter is edited by the user in the editor.
    /// </param>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ActionMethodParameterAttribute(string name, string description, Type parameterType, EditType editorEditType = EditType.Default) : Attribute
    {
        public readonly string Name = name;
        public readonly string Description = description;
        public readonly Type ParameterType = parameterType;
        public readonly EditType EditorEditType = editorEditType;
    }

    /// <summary>
    /// Specifies that an entity class can, at any point, fire an event with the given name.
    /// A separate instance of this attribute should be used for each possible event.
    /// This attribute is automatically inherited by derived classes, so should not be repeated.
    /// </summary>
    /// <param name="name">
    /// The exact name of the event that can be fired.
    /// </param>
    /// <param name="description">
    /// A description of the event to show in the editor.
    /// </param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FiresEventAttribute(string name, string description) : Attribute
    {
        public readonly string Name = name;
        public readonly string Description = description;
    }
}

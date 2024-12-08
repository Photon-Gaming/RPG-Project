# Entities

Entities are a critical component of the RPG engine. Not only are they used to represent all visible actors and non-tile decorations, they are also used to represent most of the game logic and interactions between actors. Entities are stored in rooms, and are loaded and unloaded as the player moves between different rooms. All entities are instances of classes derived from the `Entity` base class.

## Properties

Entity instances can be customised from the editor by changing the values of their class properties. When an entity class derives from another entity class, it inherits all of the derived class's properties, and can also define its own additional ones that will not be included in the derived class, but will be included in any further derivative classes.

### Programming New Properties

New properties can be added to an entity class the same as you would implement any other C# property. In order for a property to appear in the editor, however, it must have a public getter, a non-private (e.g. public, internal, protected) setter, and be decorated with the `JsonProperty` and `EditorModifiable` attributes.

The `EditorModifiable` attribute requires two string parameters: the display name of the property and a description for the editor to show. An optional third parameter can also be given to specify a special edit type, which will be presented differently in the editor.

Example property definitions:

```csharp
[JsonProperty]
[EditorModifiable("Name", "The unique name of this entity that other entities in this room will refer to it by")]
public string Name { get; protected set; } = name;

[JsonProperty]
[EditorModifiable("Position", "The location of the entity within the room", EditType.RoomCoordinate)]
public Vector2 Position { get; protected set; } = position;

[JsonProperty]
[EditorModifiable("Size", "The size of the entity relative to the tile grid")]
public Vector2 Size { get; protected set; } = size;

[JsonProperty]
[EditorModifiable("Render Texture", "The optional name of the texture that the game will draw for this entity", EditType.EntityTexture)]
public string? Texture { get; protected set; } = texture;

[JsonProperty]
[EditorModifiable("Enabled", "Whether or not this entity will be rendered and run its Tick function every frame")]
public bool Enabled { get; protected set; } = true;
```

#### Edit Types

| Edit Type | Compatible Data Types | Purpose and Effect |
|-----------|-----------------------|--------------------|
| `Default` | `float`, `int`, `string`, `bool`, `Vector2` | No special effect - value is edited directly |
| `RoomCoordinate` | `Vector2` | Used for coordinates within the current room - editor will provide coordinate selection via the mouse and verify the coordinate is inside the room boundaries |
| `EntityTexture` | `string` | Used to assign the name of an entity texture resource - the editor will provide a pop-up window previewing all available textures |
| `ConstrainedNumeric` | `float`, `int` | Constrains an integer or floating point value between a given minimum and maximum value - the editor will enforce these and provide a draggable slider to set the value. **Must be combined with the `EditorFloatConstraint` / `EditorIntConstraint` attributes to provide the minimum and maximum values** |
| `EntityLink` | `string` | Used to assign the name of an entity within the current room - the editor will provide entity selection via the mouse and verify the name belongs to an existing entity |

**Note:** Edit types specified on `List` properties are applied to the items inside the list, not the list itself.

## Entity Event->Action System

The Event->Action system is the core for creating game logic and interactions in the RPG engine. Every entity can link to, and be linked to by, other entities in the current room. Every entity has a name stored as a string, which is all that is needed to reference it within the system.

### Events

An event is simply a string that is used to group linked entity action methods by when they should be run. Every entity is capable of running ("firing") events from within its code using the `FireEvent` method. Only one parameter is required - the name of the event to fire. When an event is fired, all action methods linked to that event will be executed on the linked entity along with their stored parameters, in the order that they are defined in the list of links.

An example of firing events:

```csharp
public virtual bool Move(Vector2 targetPos, bool relative)
{
    ...

    FireEvent("OnMove");

    ...
}
```

#### Programming New Events

Technically, there is no requirement for you to do anything to start using a new event name as the parameter to the `FireEvent` method. This is ill-advised, however, as it will prevent the event from showing in the room editor. Therefore, you should always declare new events whenever you create them.

To do this, simply add the `FiresEvent` attribute to the entity's class - it will also be inherited by any derivative entity classes. The attribute takes two parameters: the name of the event, and a description to be displayed by the editor.

For example:

```csharp
...
[FiresEvent("OnMove", "Fired when the entity's position changes")]
[FiresEvent("OnResize", "Fired when the entity's size changes")]
public class Entity(string name, Vector2 position, Vector2 size, string? texture)
{
    ...
}
```

### Actions

Actions are special methods that are called in response to an event being fired. Every entity class is capable of containing its own action methods. They are referenced using the name of the method as a string.

#### Programming New Action Methods

Action methods must belong to an instance (i.e. they must not be static), have no return value (i.e. be void), and take exactly two parameters, one of type `Entity` and one of type `Dictionary<string, object?>`. These *should* be named `sender` and `parameters` respectively.

Action methods should be decorated with the `ActionMethod` attribute, which takes a description of the method as a parameter to be displayed by the editor. If you want entries to be included in the Dictionary parameter, you should also include an `ActionMethodParameter` attribute for each parameter you want. This attribute takes three parameters, a name, description, and the data type of the parameter. There is an optional fourth parameter for the edit type (see the section on Properties above).

Action methods are not executed on disabled entities by default. To override this on a per-method basis, pass `true` as an optional second parameter to the `ActionMethod` attribute.

An example action method definition:

```csharp
[ActionMethod("Moves the entity to an absolute position in the current room. Class-specific movement logic applies")]
[ActionMethodParameter("TargetPosition", "The absolute room coordinates to move the entity to", typeof(Vector2), EditType.RoomCoordinate)]
protected void SetPosition(Entity sender, Dictionary<string, object?> parameters)
{
    ...
}
```

## Engine Event Methods

Every entity has `Init`, `Destroy`, `Tick`, and `AfterTick` methods which are automatically called by the engine. Each entity class can implement its own logic for these events by overriding the corresponding `InitLogic`, `DestroyLogic`, `TickLogic`, and `AfterTickLogic` methods respectively. The purpose of each method is as follows:

| Method | Purpose |
|--------|---------|
| `Init` | Called every time an entity goes from being disabled to being enabled, including when the entity is first loaded, if it is enabled by default. |
| `Destroy` | Called every time an entity goes from being enabled to being disabled, including when the entity is unloaded, if it was enabled at the time. |
| `Tick` | Called every frame while the entity is loaded. `TickLogic` is only called if the entity is currently enabled, however. |
| `AfterTick` | Called every frame while the entity is loaded and, after *every* entity, including the current one, has run its corresponding `Tick` method for the frame. `AfterTickLogic` is only called if the entity is currently enabled, however. |

## Programming New Entity Classes

All entity classes must inherit either directly from the `Entity` class, or from another class that does. There must be an accessible, three parameter constructor defined with the first parameter being of type `string`, and the second and third being of type `Vector2`. They should be named `name`, `position`, and `size` respectively.

Entity classes must be instantiable (i.e. not be `static` or `abstract`). They can optionally be generic, but if they are, they must have only a single type parameter.

For an entity class to be selectable in the editor, it must be decorated with the `EditorEntity` attribute. The attribute takes three parameters, a name, a description, and a list of categories separated by dots (`.`). The list of categories can contain zero, one, or more categories. Categories are hierarchical, which means that, in the list `Cat1.Cat2.Cat3`, `Cat3` would appear nested inside `Cat2`, which would itself appear nested inside `Cat1`.

Unlike the `FiresEvent` attribute, the `EditorEntity` attribute is *not* automatically inherited by derived classes. It must be specified individually on each class you wish to appear in the editor.

An example of an entity class definition:

```csharp
[EditorEntity("EntitySpawner", "Creates copies of a template entity", "Tool.Spawning")]
public class EntitySpawner(string name, Vector2 position, Vector2 size) : Entity(name, position, size)
{
    ...
}
```

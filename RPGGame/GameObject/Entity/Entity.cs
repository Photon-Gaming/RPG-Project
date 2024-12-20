﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject.Entity
{
    public delegate void ActionMethod(Entity sender, Dictionary<string, object?> parameters);

    public readonly struct EventActionLink(string targetEntityName, string targetAction, Dictionary<string, object?> parameters)
    {
        public readonly string TargetEntityName = targetEntityName;
        public readonly string TargetAction = targetAction;

        [JsonConverter(typeof(JsonActionParametersConverter))]
        public readonly Dictionary<string, object?> Parameters = new(parameters);
    }

    public readonly struct ActionMethodInfo(ActionMethod? method, bool executableWhenDisabled)
    {
        public readonly ActionMethod? Method = method;
        public readonly bool ExecutableWhenDisabled = executableWhenDisabled;

        public static readonly ActionMethodInfo InvalidActionMethod = new(null, false);
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    [EditorEntity("Entity", "The base class for all entities. Has no special behaviour of its own, but can be rendered, moved, and scaled" +
        " - as well as be linked to and from in the Event->Action system.", "")]
    [FiresEvent("OnMove", "Fired when the entity's position changes")]
    [FiresEvent("OnResize", "Fired when the entity's size changes")]
    public class Entity(string name, Vector2 position, Vector2 size)
    {
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
        public string? Texture { get; protected set; } = null;

        [JsonProperty]
        [EditorModifiable("Enabled", "Whether or not this entity will be rendered and run its Tick function every frame")]
        public bool Enabled { get; protected set; } = true;

        /// <summary>
        /// Dictionary of event names to all the actions fired by that event.
        /// </summary>
        [JsonProperty]
        public Dictionary<string, List<EventActionLink>> EventActionLinks = new();

        public Room? CurrentRoom { get; set; } = null;

        // Entity origin is the bottom middle, tile origin is the top left
        public Vector2 TopLeft => new(Position.X - (Size.X / 2), Position.Y - Size.Y);
        public Vector2 BottomRight => new(TopLeft.X + Size.X, TopLeft.Y + Size.Y);

        private static Type[] actionMethodTypes = new[] { typeof(Entity), typeof(Dictionary<string, object?>) };

        protected static readonly ILogger logger = RPGGame.loggerFactory.CreateLogger("Entity");

        /// <summary>
        /// Called every time the entity is loaded or enabled, for example when the player enters its room.
        /// To implement custom init logic, override the <see cref="InitLogic"/> method.
        /// </summary>
        public void Init()
        {
            try
            {
                InitLogic();
            }
            catch (Exception exc)
            {
                logger.LogCritical(exc, "Uncaught error in InitLogic function for Entity \"{Name}\" at ({PosX}, {PosY})",
                    Name, Position.X, Position.Y);
            }
        }

        protected virtual void InitLogic() { }

        /// <summary>
        /// Called every time the entity is unloaded or disabled, for example when the player leaves its room.
        /// To implement custom destroy logic, override the <see cref="DestroyLogic"/> method.
        /// </summary>
        public void Destroy()
        {
            try
            {
                DestroyLogic();
            }
            catch (Exception exc)
            {
                logger.LogCritical(exc, "Uncaught error in DestroyLogic function for Entity \"{Name}\" at ({PosX}, {PosY})",
                    Name, Position.X, Position.Y);
            }
        }

        protected virtual void DestroyLogic() { }

        /// <summary>
        /// Called every frame while the entity is loaded.
        /// Logic will only run if entity is <see cref="Enabled"/>.
        /// To implement custom tick logic, override the <see cref="TickLogic"/> method.
        /// </summary>
        public void Tick(GameTime gameTime)
        {
            if (Enabled)
            {
                try
                {
                    TickLogic(gameTime);
                }
                catch (Exception exc)
                {
                    logger.LogCritical(exc, "Uncaught error in TickLogic function for Entity \"{Name}\" at ({PosX}, {PosY})",
                        Name, Position.X, Position.Y);
                }
            }
        }

        protected virtual void TickLogic(GameTime gameTime) { }

        /// <summary>
        /// Called every frame while the entity is loaded,
        /// after all entities including this one have run their <see cref="Tick"/> method.
        /// Logic will only run if entity is <see cref="Enabled"/>.
        /// To implement custom after-tick logic, override the <see cref="AfterTickLogic"/> method.
        /// </summary>
        public void AfterTick(GameTime gameTime)
        {
            if (Enabled)
            {
                try
                {
                    AfterTickLogic(gameTime);
                }
                catch (Exception exc)
                {
                    logger.LogCritical(exc, "Uncaught error in AfterTickLogic function for Entity \"{Name}\" at ({PosX}, {PosY})",
                        Name, Position.X, Position.Y);
                }
            }
        }

        protected virtual void AfterTickLogic(GameTime gameTime) { }

        public virtual bool Move(Vector2 targetPos, bool relative, bool force = false)
        {
            if (relative)
            {
                targetPos += Position;
            }

            Vector2 originalPosition = Position;
            Position = targetPos;
            if (!force && IsOutOfBounds())
            {
                Position = originalPosition;
                return false;
            }

            FireEvent("OnMove");
            return true;
        }

        public virtual bool Resize(Vector2 targetSize, bool relative)
        {
            if (relative)
            {
                targetSize += Size;
            }

            if (targetSize.X <= 0 || targetSize.Y <= 0)
            {
                return false;
            }

            Vector2 originalSize = Size;
            Size = targetSize;
            if (IsOutOfBounds())
            {
                Size = originalSize;
                return false;
            }

            FireEvent("OnResize");
            return true;
        }

        /// <summary>
        /// Create a new instance of the entity class based on this entity.
        /// </summary>
        /// <remarks>
        /// Does not produce a 1:1 copy of the original entity.
        /// Only properties present in the Entity's JSON serialization will be copied.
        /// </remarks>
        public Entity Clone(string newName)
        {
            // De-serializing and re-serializing the entity as JSON creates a copy of the entity with only the editable parameters transferred.
            Entity clone = JsonConvert.DeserializeObject<Entity>(
                JsonConvert.SerializeObject(this, RPGContentLoader.SerializerSettings), RPGContentLoader.SerializerSettings)!;
            clone.Name = newName;
            return clone;
        }

        public bool Collides(Entity other)
        {
            return !ReferenceEquals(this, other)  // Entity should not collide with itself
                && TopLeft.X < other.BottomRight.X
                && TopLeft.Y < other.BottomRight.Y
                && BottomRight.X > other.TopLeft.X
                && BottomRight.Y > other.TopLeft.Y;
        }

        public bool Collides(Vector2 position)
        {
            return position.X > TopLeft.X
                && position.Y > TopLeft.Y
                && position.X < BottomRight.X
                && position.Y < BottomRight.Y;
        }

        public bool IsOutOfBounds()
        {
            // Room.IsOutOfBounds could be used for this, but we can reduce the number of comparisons needed
            // by only checking TopLeft against the lower bound and BottomRight against the upper bound.
            return CurrentRoom is null
                || TopLeft.X < 0
                || TopLeft.Y < 0
                || BottomRight.X >= CurrentRoom.TileMap.GetLength(0)
                || BottomRight.Y >= CurrentRoom.TileMap.GetLength(1);
        }

        public void Enable()
        {
            if (Enabled)
            {
                return;
            }
            Enabled = true;
            Init();
        }

        public void Disable()
        {
            if (!Enabled)
            {
                return;
            }
            Enabled = false;
            Destroy();
        }

        // Action Methods

        [ActionMethod("Enables the entity, starting its Tick method and making it visible if it has a texture assigned", true)]
        protected void Enable(Entity sender, Dictionary<string, object?> parameters)
        {
            Enable();
        }

        [ActionMethod("Disables the entity, stopping its Tick method and making it invisible")]
        protected void Disable(Entity sender, Dictionary<string, object?> parameters)
        {
            Disable();
        }

        [ActionMethod("Moves the entity to an absolute position in the current room. Class-specific movement logic applies")]
        [ActionMethodParameter("TargetPosition", "The absolute room coordinates to move the entity to", typeof(Vector2), EditType.RoomCoordinate)]
        protected void SetPosition(Entity sender, Dictionary<string, object?> parameters)
        {
            if (!parameters.TryGetValue("TargetPosition", out object? positionObj) || positionObj is not Vector2 position)
            {
                logger.LogError("TargetPosition parameter for SetPosition action was not given or was of incorrect type" +
                    " (fired by \"{Source}\" to \"{Target}\")",
                    sender.Name, Name);
                return;
            }
            Move(position, false);
        }

        [ActionMethod("Moves the entity to a position in the current room relative to its current position. Class-specific movement logic applies")]
        [ActionMethodParameter("MovementVector", "The amount to change the X and Y coordinates by", typeof(Vector2))]
        protected void Move(Entity sender, Dictionary<string, object?> parameters)
        {
            if (!parameters.TryGetValue("MovementVector", out object? positionObj) || positionObj is not Vector2 position)
            {
                logger.LogError("MovementVector parameter for Move action was not given or was of incorrect type" +
                    " (fired by \"{Source}\" to \"{Target}\")",
                    sender.Name, Name);
                return;
            }
            Move(position, true);
        }

        [ActionMethod("Sets the entity size to an absolute number of units. Class-specific resize logic applies")]
        [ActionMethodParameter("TargetSize", "The absolute size to resize the entity to", typeof(Vector2), EditType.RoomCoordinate)]
        protected void SetSize(Entity sender, Dictionary<string, object?> parameters)
        {
            if (!parameters.TryGetValue("TargetSize", out object? sizeObj) || sizeObj is not Vector2 size)
            {
                logger.LogError("TargetSize parameter for SetSize action was not given or was of incorrect type" +
                    " (fired by \"{Source}\" to \"{Target}\")",
                    sender.Name, Name);
                return;
            }
            Resize(size, false);
        }

        [ActionMethod("Sets the entity size to a number of units relative to the current size. Class-specific resize logic applies")]
        [ActionMethodParameter("ResizeVector", "The number of units in each dimension to affect the size by", typeof(Vector2), EditType.RoomCoordinate)]
        protected void Resize(Entity sender, Dictionary<string, object?> parameters)
        {
            if (!parameters.TryGetValue("ResizeVector", out object? sizeObj) || sizeObj is not Vector2 size)
            {
                logger.LogError("ResizeVector parameter for Resize action was not given or was of incorrect type" +
                    " (fired by \"{Source}\" to \"{Target}\")",
                    sender.Name, Name);
                return;
            }
            Resize(size, true);
        }

        [ActionMethod("Sets the entity size to a number of units relative to the current size with multipliers. Class-specific resize logic applies")]
        [ActionMethodParameter("ScaleVector", "The multiplier in each dimension to affect the size by", typeof(Vector2), EditType.RoomCoordinate)]
        protected void Scale(Entity sender, Dictionary<string, object?> parameters)
        {
            if (!parameters.TryGetValue("ScaleVector", out object? sizeObj) || sizeObj is not Vector2 scale)
            {
                logger.LogError("ScaleVector parameter for Scale action was not given or was of incorrect type" +
                    " (fired by \"{Source}\" to \"{Target}\")",
                    sender.Name, Name);
                return;
            }
            Resize(Size * scale, false);
        }

        [ActionMethod("Sets the render texture of the entity")]
        [ActionMethodParameter("TextureName", "The name of the new texture to use. Must be part of the built game content", typeof(string), EditType.EntityTexture)]
        protected void ChangeTexture(Entity sender, Dictionary<string, object?> parameters)
        {
            if (!parameters.TryGetValue("TextureName", out object? textureObj) || textureObj is not string texture)
            {
                logger.LogError("TextureName parameter for ChangeTexture action was not given or was of incorrect type" +
                    " (fired by \"{Source}\" to \"{Target}\")",
                    sender.Name, Name);
                return;
            }
            Texture = texture == "" ? null : texture;
        }

        // Event->Action System

        protected void FireEvent(string eventName)
        {
            logger.LogTrace("Event \"{Event}\" fired by \"{Source}\"", eventName, Name);

            if (CurrentRoom is null
                || !EventActionLinks.TryGetValue(eventName, out List<EventActionLink>? links))
            {
                return;
            }

            if (!CurrentRoom.CurrentlyTickingEntities)
            {
                logger.LogWarning(
                    "Ignoring event \"{Event}\" fired by \"{Source}\" as the engine is not currently ticking entities",
                    eventName, Name);
                return;
            }

            foreach (EventActionLink link in links)
            {
                if (!CurrentRoom.LoadedNamedEntities.TryGetValue(link.TargetEntityName, out Entity? targetEntity))
                {
                    logger.LogError("Entity \"{Target}\" linked from \"{Source}\" for {Event}->{Action} could not be found",
                        link.TargetEntityName, Name, eventName, link.TargetAction);
                    continue;
                }

                logger.LogTrace("Running action method on \"{Target}\" linked from \"{Source}\" for {Event}->{Action}",
                    link.TargetEntityName, Name, eventName, link.TargetAction);
                targetEntity.RunActionMethod(link.TargetAction, this, link.Parameters);
            }
        }

        public void RunActionMethod(string methodName, Entity sender, Dictionary<string, object?> parameters)
        {
            ActionMethodInfo methodInfo = GetActionMethod(methodName);
            if (Enabled || methodInfo.ExecutableWhenDisabled)
            {
                methodInfo.Method?.Invoke(sender, parameters);
            }
        }

        private Dictionary<string, ActionMethodInfo> actionMethods = new();
        private ActionMethodInfo GetActionMethod(string methodName)
        {
            // Action methods are cached so non-performant reflection doesn't have to be used each time.
            if (actionMethods.TryGetValue(methodName, out ActionMethodInfo outMethod))
            {
                return outMethod;
            }

            MethodInfo? methodInfo = GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                actionMethodTypes);
            if (methodInfo is null)
            {
                logger.LogError("Action method with name \"{Name}\" could not be found on Entity \"{Entity}\" of type {Type}",
                    methodName, Name, GetType().ToString());
                return ActionMethodInfo.InvalidActionMethod;
            }

            ActionMethod? actionMethod = (ActionMethod?)Delegate.CreateDelegate(typeof(ActionMethod), this, methodInfo, false);
            if (actionMethod is null)
            {
                logger.LogError("Unexpected error getting action method with name \"{Name}\" on Entity \"{Entity}\" of type {Type}",
                    methodName, Name, GetType().ToString());
                return ActionMethodInfo.InvalidActionMethod;
            }

            ActionMethodInfo actionMethodInfo = new(actionMethod,
                methodInfo.GetCustomAttribute<ActionMethodAttribute>()?.ExecutableWhenDisabled ?? false);

            actionMethods[methodName] = actionMethodInfo;
            return actionMethodInfo;
        }
    }
}

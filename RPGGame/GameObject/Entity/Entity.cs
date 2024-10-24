using System;
using System.Collections.Generic;
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

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    [EditorEntity("Entity", "The base class for all entities. Has no special behaviour of its own, but can be rendered, moved, and scaled" +
        " - as well as be linked to and from in the Event->Action system.", "Basic")]
    [FiresEvent("OnInit", "Fired when the entity is loaded, before it runs its initialisation logic")]
    [FiresEvent("OnLoad", "Fired when the entity is loaded, after it runs its initialisation logic")]
    [FiresEvent("OnUnload", "Fired when the entity is loaded, before it runs its destroy logic")]
    [FiresEvent("OnDestroy", "Fired when the entity is loaded, after it runs its destroy logic")]
    [FiresEvent("OnMove", "Fired when the entity's position changes")]
    public class Entity(string name, Vector2 position, Vector2 size, string? texture) : ICloneable
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
        public string? Texture { get; protected set; } = texture;

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

        private static readonly ILogger logger = RPGGame.loggerFactory.CreateLogger("Entity");

        /// <summary>
        /// Called every time the entity is loaded or enabled, for example when the player enters its room.
        /// To implement custom init logic, override the <see cref="InitLogic"/> method.
        /// </summary>
        public void Init()
        {
            FireEvent("OnInit");
            try
            {
                InitLogic();
            }
            catch (Exception exc)
            {
                logger.LogCritical(exc, "Uncaught error in InitLogic function for Entity \"{Name}\" at ({PosX}, {PosY})",
                    Name, Position.X, Position.Y);
            }
            FireEvent("OnLoad");
        }

        protected virtual void InitLogic() { }

        /// <summary>
        /// Called every time the entity is unloaded or disabled, for example when the player leaves its room.
        /// To implement custom destroy logic, override the <see cref="DestroyLogic"/> method.
        /// </summary>
        public void Destroy()
        {
            FireEvent("OnUnload");
            try
            {
                DestroyLogic();
            }
            catch (Exception exc)
            {
                logger.LogCritical(exc, "Uncaught error in DestroyLogic function for Entity \"{Name}\" at ({PosX}, {PosY})",
                    Name, Position.X, Position.Y);
            }
            FireEvent("OnDestroy");
        }

        protected virtual void DestroyLogic() { }

        /// <summary>
        /// Called every frame while the entity is loaded and enabled.
        /// </summary>
        public virtual void Tick(GameTime gameTime) { }

        public virtual bool Move(Vector2 targetPos, bool relative)
        {
            if (relative)
            {
                targetPos += Position;
            }

            Vector2 originalPosition = Position;
            Position = targetPos;
            if (IsOutOfBounds())
            {
                Position = originalPosition;
                return false;
            }

            FireEvent("OnMove");
            return true;
        }

        public virtual object Clone()
        {
            return new Entity(Name, Position, Size, Texture);
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

        [ActionMethod("Enables the entity, starting its Tick method and making it visible if it has a texture assigned")]
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

            foreach (EventActionLink link in links)
            {
                if (!CurrentRoom.LoadedNamedEntities.TryGetValue(link.TargetEntityName, out Entity? targetEntity))
                {
                    logger.LogError("Entity \"{Target}\" linked from \"{Source}\" for {Event}->{Action} could not be found",
                        link.TargetEntityName, Name, eventName, link.TargetAction);
                    continue;
                }

                if (!targetEntity.Enabled)
                {
                    logger.LogDebug("Entity \"{Target}\" linked from \"{Source}\" for {Event}->{Action} is disabled. Action method will not run",
                        link.TargetEntityName, Name, eventName, link.TargetAction);
                    continue;
                }

                logger.LogTrace("Running action method on \"{Target}\" linked from \"{Source}\" for {Event}->{Action}",
                    link.TargetEntityName, Name, eventName, link.TargetAction);
                targetEntity.GetActionMethod(link.TargetAction)?.Invoke(this, link.Parameters);
            }
        }

        private Dictionary<string, ActionMethod> actionMethods = new();
        public ActionMethod? GetActionMethod(string methodName)
        {
            // Action methods are cached so non-performant reflection doesn't have to be used each time.
            if (actionMethods.TryGetValue(methodName, out ActionMethod? outMethod))
            {
                return outMethod;
            }

            ActionMethod? actionMethod = (ActionMethod?)Delegate.CreateDelegate(typeof(ActionMethod), this, methodName, false, false);
            if (actionMethod is null)
            {
                logger.LogError("Action method with name \"{Name}\" could not be found on Entity \"{Entity}\" of type {Type}",
                    methodName, Name, GetType().ToString());
                return null;
            }
            actionMethods[methodName] = actionMethod;
            return actionMethod;
        }
    }
}

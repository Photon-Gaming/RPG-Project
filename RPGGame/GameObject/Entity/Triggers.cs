using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject.Entity
{
    [EditorEntity("TriggerGroup", "Allows multiple triggers to be linked together to act as one trigger", "Tool.Triggers")]
    [FiresEvent("OnTriggerAny", "Fired when any of the linked triggers' trigger conditions are met")]
    [FiresEvent("OnTriggerGroup", "Fired when any of the linked triggers' trigger conditions are met following at least one frame where none of the conditions were met")]
    public class TriggerGroup(string name, Vector2 position, Vector2 size, string? texture) : Entity(name, position, size, texture)
    {
        [JsonProperty]
        [EditorModifiable("Linked Triggers", "A list of all the trigger entities that are part of this trigger group", EditType.EntityLink)]
        public List<string> LinkedTriggers { get; set; } = new();

        public bool AnyTriggerConditionsMetLastFrame { get; protected set; } = false;

        protected bool anyTriggerConditionsMetThisFrame = false;

        protected TriggerBase[] linkedTriggerEntities = Array.Empty<TriggerBase>();

        protected override void InitLogic()
        {
            base.InitLogic();

            // Convert list of names to list of entities
            linkedTriggerEntities = LinkedTriggers.Select(t => CurrentRoom?.LoadedNamedEntities.GetValueOrDefault(t))
                .OfType<TriggerBase>().ToArray();

            if (linkedTriggerEntities.Length != LinkedTriggers.Count)
            {
                logger.LogWarning("TriggerGroup \"{Name}\" had {Count} linked entities which either could not be found or were not a type of trigger.",
                    Name, LinkedTriggers.Count - linkedTriggerEntities.Length);
            }
        }

        protected override void TickLogic(GameTime gameTime)
        {
            base.TickLogic(gameTime);

            anyTriggerConditionsMetThisFrame = false;

            foreach (TriggerBase trigger in linkedTriggerEntities)
            {
                if (trigger.TriggerConditionMet())
                {
                    if (!trigger.TriggerConditionMetLastFrame)
                    {
                        FireEvent("OnTriggerAny");
                        if (!AnyTriggerConditionsMetLastFrame)
                        {
                            FireEvent("OnTriggerGroup");
                        }
                    }
                    anyTriggerConditionsMetThisFrame = true;
                }
            }
        }

        public override void AfterTick(GameTime gameTime)
        {
            base.AfterTick(gameTime);

            AnyTriggerConditionsMetLastFrame = anyTriggerConditionsMetThisFrame;
        }
    }

    [FiresEvent("OnTrigger", "Fired when the collision condition of the trigger is met.")]
    public abstract class TriggerBase(string name, Vector2 position, Vector2 size, string? texture) : Entity(name, position, size, texture)
    {
        [JsonProperty]
        [EditorModifiable("Collision Mode", "The type of detection the trigger will use to detect collision.")]
        public CollisionMode TriggerCollisionMode { get; set; } = CollisionMode.BoundingBox;

        public bool TargetCurrentlyInside { get; protected set; }

        public bool TriggerConditionMetLastFrame { get; protected set; } = false;

        protected bool triggerConditionMetThisFrame = false;

        protected override void TickLogic(GameTime gameTime)
        {
            base.TickLogic(gameTime);

            TargetCurrentlyInside = IsTargetInside();

            if (TriggerConditionMet())
            {
                if (!TriggerConditionMetLastFrame)
                {
                    FireEvent("OnTrigger");
                }
                triggerConditionMetThisFrame = true;
            }
            else
            {
                triggerConditionMetThisFrame = false;
            }
        }

        public override void AfterTick(GameTime gameTime)
        {
            base.AfterTick(gameTime);

            TriggerConditionMetLastFrame = triggerConditionMetThisFrame;
        }

        public abstract bool IsTargetInside();

        public abstract bool TriggerConditionMet();
    };

    public abstract class PlayerTriggerBase(string name, Vector2 position, Vector2 size, string? texture) : TriggerBase(name, position, size, texture)
    {
        public override bool IsTargetInside()
        {
            Player? currentPlayer = CurrentRoom?.ContainingWorld?.CurrentPlayer;
            return currentPlayer is not null && TriggerCollisionMode switch
            {
                CollisionMode.BoundingBox => Collides(currentPlayer),
                CollisionMode.Origin => Collides(currentPlayer.Position),
                _ => throw new InvalidOperationException($"{TriggerCollisionMode} is not a valid collision mode for player triggers.")
            };
        }
    }

    [EditorEntity("PlayerEnterTrigger", "A trigger entity that fires when the player walks into the trigger's bounding box.", "Tool.Triggers.Player")]
    public class PlayerEnterTrigger(string name, Vector2 position, Vector2 size, string? texture) : PlayerTriggerBase(name, position, size, texture)
    {
        public override bool TriggerConditionMet()
        {
            return TargetCurrentlyInside;
        }
    }

    [EditorEntity("PlayerExitTrigger", "A trigger entity that fires when the player walks out of the trigger's bounding box.", "Tool.Triggers.Player")]
    public class PlayerExitTrigger(string name, Vector2 position, Vector2 size, string? texture) : PlayerTriggerBase(name, position, size, texture)
    {
        public override bool TriggerConditionMet()
        {
            return !TargetCurrentlyInside;
        }
    }
}

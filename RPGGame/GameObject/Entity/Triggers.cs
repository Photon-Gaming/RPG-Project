using System;
using Microsoft.Xna.Framework;

namespace RPGGame.GameObject.Entity
{
    [FiresEvent("OnTrigger", "Fired when the collision condition of the trigger is met.")]
    public abstract class TriggerBase(string name, Vector2 position, Vector2 size, string? texture) : Entity(name, position, size, texture)
    {
        [EditorModifiable("Collision Mode", "The type of detection the trigger will use to detect collision.")]
        public CollisionMode TriggerCollisionMode { get; set; } = CollisionMode.BoundingBox;

        protected bool targetWasInsideLastFrame;
        protected bool targetCurrentlyInside;

        protected override void InitLogic()
        {
            base.InitLogic();

            targetWasInsideLastFrame = IsTargetInside();
        }

        public override void Tick(GameTime gameTime)
        {
            base.Tick(gameTime);

            targetCurrentlyInside = IsTargetInside();

            if (TriggerConditionMet())
            {
                FireEvent("OnTrigger");
            }

            targetWasInsideLastFrame = targetCurrentlyInside;
        }

        protected abstract bool IsTargetInside();

        protected abstract bool TriggerConditionMet();
    };

    public abstract class PlayerTriggerBase(string name, Vector2 position, Vector2 size, string? texture) : TriggerBase(name, position, size, texture)
    {
        protected override bool IsTargetInside()
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

    [EditorEntity("PlayerEnterTrigger", "A trigger entity that fires when the player walks into the trigger's bounding box.", "Triggers.Player")]
    public class PlayerEnterTrigger(string name, Vector2 position, Vector2 size, string? texture) : PlayerTriggerBase(name, position, size, texture)
    {
        protected override bool TriggerConditionMet()
        {
            return targetCurrentlyInside && !targetWasInsideLastFrame;
        }
    }

    [EditorEntity("PlayerExitTrigger", "A trigger entity that fires when the player walks out of the trigger's bounding box.", "Triggers.Player")]
    public class PlayerExitTrigger(string name, Vector2 position, Vector2 size, string? texture) : PlayerTriggerBase(name, position, size, texture)
    {
        protected override bool TriggerConditionMet()
        {
            return !targetCurrentlyInside && targetWasInsideLastFrame;
        }
    }
}

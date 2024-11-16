using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject.Entity
{
    [FiresEvent("TimerElapsed", "Fired when the specified duration of the timer lapses")]
    public abstract class TimerBase<TTime, TDuration>(string name, Vector2 position, Vector2 size, string? texture) : Entity(name, position, size, texture) where TTime : IComparable<TTime>
    {
        [JsonProperty]
        [EditorModifiable("Repeat Timer?", "Whether or not the timer should only fire once after being enabled, or whether it should continue to fire every time the specified duration lapses.")]
        public bool Repeat { get; set; }

        [JsonProperty]
        [EditorModifiable("Timer Duration", "The amount of time before the timer lapses")]
        public abstract TDuration TimerDuration { get; set; }

        public abstract TTime NextFireTime { get; protected set; }

        public abstract TTime GetNextFireTime();
        public abstract TTime GetCurrentTime();

        protected override void InitLogic()
        {
            base.InitLogic();

            NextFireTime = GetNextFireTime();
        }

        public override void Tick(GameTime gameTime)
        {
            base.Tick(gameTime);

            // CompareTo is used as IComparable does not implement comparison operators
            if (GetCurrentTime().CompareTo(GetNextFireTime()) >= 0)
            {
                // The next fire time is now or has elapsed
                FireEvent("TimerElapsed");

                if (!Repeat)
                {
                    // Prevent timer firing again unless it is explicitly re-enabled
                    Enabled = false;
                }
            }
        }
    }
}

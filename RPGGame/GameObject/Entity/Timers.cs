﻿using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace RPGGame.GameObject.Entity
{
    [FiresEvent("TimerElapsed", "Fired when the specified duration of the timer lapses")]
    public abstract class TimerBase<TTime, TDuration>(string name, Vector2 position, Vector2 size) : Entity(name, position, size)
        where TTime : struct, IComparable<TTime>
        where TDuration : struct
    {
        [JsonProperty]
        [EditorModifiable("Repeat Timer?", "Whether or not the timer should only fire once after being enabled, or whether it should continue to fire every time the specified duration lapses.")]
        public bool Repeat { get; set; }

        [JsonProperty]
        [EditorModifiable("Timer Duration", "The amount of time before the timer lapses")]
        public TDuration TimerDuration { get; set; }

        public TTime NextFireTime { get; protected set; }

        public abstract TTime GetCurrentTime();
        public abstract TTime GetNextFireTime();

        protected override void InitLogic()
        {
            base.InitLogic();

            NextFireTime = GetNextFireTime();
        }

        protected override void TickLogic(GameTime gameTime)
        {
            base.TickLogic(gameTime);

            // CompareTo is used as IComparable does not implement comparison operators
            if (GetCurrentTime().CompareTo(NextFireTime) >= 0)
            {
                // The next fire time is now or has elapsed
                FireEvent("TimerElapsed");

                if (Repeat)
                {
                    NextFireTime = GetNextFireTime();
                }
                else
                {
                    // Prevent timer firing again unless it is explicitly re-enabled
                    Enabled = false;
                }
            }
        }
    }

    [EditorEntity("FrameTimer", "Fires an event after a specified number of frames are processed", "Tool.Timing.Periodic")]
    public class FrameTimer(string name, Vector2 position, Vector2 size) : TimerBase<ulong, ulong>(name, position, size)
    {
        private ulong framesElapsed = 0;

        public override ulong GetCurrentTime()
        {
            return framesElapsed;
        }

        public override ulong GetNextFireTime()
        {
            return framesElapsed + TimerDuration;
        }

        protected override void TickLogic(GameTime gameTime)
        {
            base.TickLogic(gameTime);

            framesElapsed++;
        }
    }

    [EditorEntity("ClockTimer", "Fires an event after a specified amount of time elapses", "Tool.Timing.Periodic")]
    public class ClockTimer(string name, Vector2 position, Vector2 size) : TimerBase<DateTime, TimeSpan>(name, position, size)
    {
        public override DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }

        public override DateTime GetNextFireTime()
        {
            return GetCurrentTime() + TimerDuration;
        }
    }
}

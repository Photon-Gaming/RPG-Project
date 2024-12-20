﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace RPGGame.GameObject.Entity
{
    public class Player : Entity
    {

        public const string PlayerEntityName = "Player";
        public const string PlayerTexture = "player";

        // TODO: Make configurable
        private const Keys keyUp = Keys.W;
        private const Keys keyLeft = Keys.A;
        private const Keys keyDown = Keys.S;
        private const Keys keyRight = Keys.D;

        [JsonProperty]
        [EditorModifiable("Enable Input?", "Whether or not the user can control the player.")]
        public bool InputEnabled { get; set; } = true;

        [JsonProperty]
        [EditorModifiable("Speed", "The number of units the player moves per second of having the corresponding input key held down.")]
        public float Speed { get; set; } = 2.5f;

        public Input PlayerInput { get; }

        public Player(Input playerInput) : base(PlayerEntityName, new Vector2(0.5f, 1), Vector2.One)
        {
            PlayerInput = playerInput;

            Texture = PlayerTexture;
        }

        protected override void TickLogic(GameTime gameTime)
        {
            base.TickLogic(gameTime);

            if (InputEnabled)
            {
                ProcessInputMovement(gameTime);
            }
        }

        private void ProcessInputMovement(GameTime gameTime)
        {
            Vector2 movementVector = new(0, 0);

            if (PlayerInput.GetKeyboardKeyDown(keyUp))
            {
                movementVector.Y--;
            }
            if (PlayerInput.GetKeyboardKeyDown(keyDown))
            {
                movementVector.Y++;
            }
            if (PlayerInput.GetKeyboardKeyDown(keyLeft))
            {
                movementVector.X--;
            }
            if (PlayerInput.GetKeyboardKeyDown(keyRight))
            {
                movementVector.X++;
            }

            if (movementVector == Vector2.Zero)
            {
                // movementVector.Normalize will return NaN values if used on zero vector
                return;
            }

            movementVector.Normalize();

            Move(movementVector * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds, true);
        }

        // Action Methods

        [ActionMethod("Enables player input, allowing the user to move the player")]
        protected void EnableInput(Entity sender, Dictionary<string, object?> parameters)
        {
            InputEnabled = true;
        }

        [ActionMethod("Disables player input, stopping the user from moving the player")]
        protected void DisableInput(Entity sender, Dictionary<string, object?> parameters)
        {
            InputEnabled = false;
        }
    };
}

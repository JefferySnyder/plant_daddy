using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Project1
{
    class Ghost
    {
        public Ghost(Vector2 position, float opacity)
        {
            pos = position;
            this.opacity = opacity;
        }
        public Vector2 pos;
        public float opacity;
    }
    public enum Facing
    {
        Down = 0, Left = 1, Up = 2, Right = 3
    }

    class Player
    {
        private Facing characterDir;

        AnimatedTexture playerIdle;
        AnimatedTexture playerWalk;
        AnimatedTexture playerDash;
        List<Ghost> ghosts;
        AnimatedTexture playerSwing;
        private const float depth = 0.5f;

        private const float GroundDragFactor = 0.8f;
        private const float MoveAcceleration = 3000f;
        private const float MaxMoveSpeed = 1750f;
        private float Xmovement;
        private float Ymovement;
        private Vector2 velocity;
        private Vector2 characterPos;
        private const int frameRows = 4;
        private const int frames = 4;
        private const int framesPerSec = 8;

        float cooldowntime = 1;
        KeyboardState currentKeyState;
        KeyboardState previousKeyState;
        bool isSwinging;

        public void Initialize()
        {
            ghosts = [];
            characterDir = Facing.Down;
            playerIdle = new(Vector2.Zero, depth);
            playerWalk = new(Vector2.Zero, depth);
            playerDash = new(Vector2.Zero, depth);
            playerSwing = new(Vector2.Zero, depth);
        }
        public void Load(ContentManager content)
        {
            playerIdle.Load(content, "Player_Idles", frames, framesPerSec, frameRows);
            playerWalk.Load(content, "Player_Walks", 8, framesPerSec * 2, frameRows);
            playerDash.Load(content, "Player_Dashes", 1, 1, frameRows * 2);
            playerSwing.Load(content, "Player_Swings", 7, 14, frameRows);
            characterPos = new Vector2(Game1.gameWidth / 2, Game1.gameHeight / 2);
        }
        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            previousKeyState = currentKeyState;
            var kstate = Keyboard.GetState();
            currentKeyState = kstate;

            GetInput(kstate, elapsed);

            ApplyPhysics(elapsed);

            CheckCollisions();

            if (velocity == Vector2.Zero)
                playerIdle.UpdateFrame(elapsed, characterPos);
            else if (velocity.X < -220 || velocity.Y < -220 || velocity.X > 220 || velocity.Y > 220)
            {
                ghosts.Add(new Ghost(characterPos, 0.5f));
            }
            else
            {
                playerWalk.UpdateFrame(elapsed, characterPos);
                if (ghosts != null && ghosts.Count > 0 && ghosts[0].opacity <= 0f)
                    ghosts.Clear();
            }

            if (isSwinging)
            {
                playerSwing.UpdateFrame(elapsed, characterPos);
            }

            Xmovement = 0f;
            Ymovement = 0f;

        }
        private void GetInput(KeyboardState kstate, float elapsed)
        {
            //Xmovement = 0f; Ymovement = 0f;

            //if (kstate.IsKeyDown(Keys.Space))
            if (kstate.IsKeyDown(Keys.Up))
            {
                Ymovement = -1f;
                characterDir = Facing.Up;
            }
            if (kstate.IsKeyDown(Keys.Down))
            {
                Ymovement = 1f;
                characterDir = Facing.Down;
            }
            if (kstate.IsKeyDown(Keys.Left))
            {
                Xmovement = -1f;
                characterDir = Facing.Left;
            }
            if (kstate.IsKeyDown(Keys.Right))
            {
                Xmovement = 1f;
                characterDir = Facing.Right;
            }

            if (currentKeyState.IsKeyDown(Keys.LeftControl) && !previousKeyState.IsKeyDown(Keys.LeftControl))
            {
                Xmovement *= 20;
                Ymovement *= 20;
            }

            cooldowntime += elapsed;

            if (cooldowntime > 0.5)
                isSwinging = false;
            if (cooldowntime >= 0.5 && kstate.IsKeyDown(Keys.Space))
            {
                isSwinging = true;
                cooldowntime = 0;
            }
        }
        private void ApplyPhysics(float elapsed)
        {
            velocity.X += Xmovement * MoveAcceleration * elapsed;
            velocity.Y += Ymovement * MoveAcceleration * elapsed;

            velocity.X *= GroundDragFactor;
            velocity.Y *= GroundDragFactor;

            //velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);
            //velocity.Y = MathHelper.Clamp(velocity.Y, -MaxMoveSpeed, MaxMoveSpeed);

            var previousPosition = characterPos;

            characterPos += velocity * elapsed;
            characterPos = new Vector2((float)Math.Round(characterPos.X), (float)Math.Round(characterPos.Y));

            if (characterPos.X == previousPosition.X)
                velocity.X = 0;
            if (characterPos.Y == previousPosition.Y)
                velocity.Y = 0;
        }
        private void CheckCollisions()
        {
            if (characterPos.X < 0) characterPos.X = Game1.gameWidth;
            if (characterPos.X > Game1.gameWidth) characterPos.X = 0;

            if (characterPos.Y < 0) characterPos.Y = Game1.gameHeight;
            if (characterPos.Y > Game1.gameHeight) characterPos.Y = 0;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isSwinging)
            {
                Vector2 updatedCharacterPos = characterPos + new Vector2(-16, -24);
                playerSwing.DrawFrame(spriteBatch, updatedCharacterPos, characterDir);
                if (velocity.X < -220 || velocity.Y < -220 || velocity.X > 220 || velocity.Y > 220)
                {
                    foreach (var ghost in ghosts)
                    {
                        if (characterDir == Facing.Left || characterDir == Facing.Right)
                            playerDash.DrawFrame(spriteBatch, ghost.pos + new Vector2(0, -5), characterDir + 4, Color.White * ghost.opacity);
                        else
                            playerDash.DrawFrame(spriteBatch, ghost.pos, characterDir + 4, Color.White * ghost.opacity);
                        ghost.opacity -= 0.075f;
                    }
                }
            }
            else
            {
                if (velocity == Vector2.Zero)
                    playerIdle.DrawFrame(spriteBatch, characterPos, characterDir);
                else if (velocity.X < -220 || velocity.Y < -220 || velocity.X > 220 || velocity.Y > 220)
                {
                    playerDash.DrawFrame(spriteBatch, characterPos, characterDir);
                    foreach (var ghost in ghosts)
                    {
                        playerDash.DrawFrame(spriteBatch, ghost.pos, characterDir + 4, Color.White * ghost.opacity);
                        ghost.opacity -= 0.075f;
                    }
                }
                else
                {
                    playerWalk.DrawFrame(spriteBatch, characterPos, characterDir);
                }
            }
        }
    }
}

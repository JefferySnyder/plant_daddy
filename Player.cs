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

        public AnimatedTexture playerIdle;
        public AnimatedTexture playerWalk;
        public AnimatedTexture playerDash;
        List<Ghost> ghosts;
        public AnimatedTexture playerSwing;
        private Vector2 playerSwingOffset = new Vector2(16,24);
        private const float depth = 0.5f;

        private const int DashThreshold = 125;
        private const int DashSpeed = 20;
        private const float GroundDragFactor = 0.48f;
        private const float MoveAcceleration = 8000f;
        private const float MaxMoveSpeed = 1750f;
        private float Xmovement;
        private float Ymovement;
        private Vector2 velocity;
        public Vector2 characterPos;
        private const int frameRows = 4;
        private const int frames = 4;
        private const int framesPerSec = 4;

        float cooldowntime = 1;
        KeyboardState currentKeyState;
        KeyboardState previousKeyState;
        public bool isSwinging;
        Rectangle initialSwingCollision;

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
            // Have to set custom rect size after load
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

            if (velocity.X < -DashThreshold || velocity.Y < -DashThreshold || velocity.X > DashThreshold || velocity.Y > DashThreshold)
            {
                ghosts.Add(new Ghost(characterPos, 0.5f));
            }
            else
            {
                if (ghosts != null && ghosts.Count > 0 && ghosts[0].opacity <= 0f)
                    ghosts.Clear();
            }

            playerIdle.UpdateFrame(elapsed, characterPos);
            playerWalk.UpdateFrame(elapsed, characterPos);
            playerSwing.UpdateFrame(elapsed, characterPos - playerSwingOffset);

            Xmovement = 0f;
            Ymovement = 0f;

            //Debug.WriteLine(characterPos.ToString());

        }
        private void GetInput(KeyboardState kstate, float elapsed)
        {
            //Xmovement = 0f; Ymovement = 0f;

            //if (kstate.IsKeyDown(Keys.Space))
            if (kstate.IsKeyDown(Keys.Up) || kstate.IsKeyDown(Keys.W))
            {
                Ymovement = -1f;
                characterDir = Facing.Up;
            }
            if (kstate.IsKeyDown(Keys.Down) || kstate.IsKeyDown(Keys.S))
            {
                Ymovement = 1f;
                characterDir = Facing.Down;
            }
            if (kstate.IsKeyDown(Keys.Left) || kstate.IsKeyDown(Keys.A))
            {
                Xmovement = -1f;
                characterDir = Facing.Left;
            }
            if (kstate.IsKeyDown(Keys.Right) || kstate.IsKeyDown(Keys.D))
            {
                Xmovement = 1f;
                characterDir = Facing.Right;
            }

            if (currentKeyState.IsKeyDown(Keys.Space) && !previousKeyState.IsKeyDown(Keys.Space))
            {
                Xmovement *= DashSpeed;
                Ymovement *= DashSpeed;
            }

            cooldowntime += elapsed;

            if (cooldowntime > 0.5)
            {
                isSwinging = false;
            }
            if (cooldowntime >= 0.5 && kstate.IsKeyDown(Keys.J))
            {
                isSwinging = true;
                playerSwing.Reset();
                cooldowntime = 0;
                initialSwingCollision = getSwingCollision();
                //characterPos += new Vector2(-16, -24);
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

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isSwinging)
            {
                playerSwing.DrawFrame(spriteBatch, characterDir);
                if (velocity.X < -DashThreshold || velocity.Y < -DashThreshold || velocity.X > DashThreshold || velocity.Y > DashThreshold)
                {
                    foreach (var ghost in ghosts)
                    {
                        if (characterDir == Facing.Left || characterDir == Facing.Right)
                            playerDash.DrawFrame(spriteBatch, ghost.pos + new Vector2(20,18), characterDir + 4, Color.White * ghost.opacity);
                        else
                            playerDash.DrawFrame(spriteBatch, ghost.pos + new Vector2(16,18), characterDir + 4, Color.White * ghost.opacity);
                        ghost.opacity -= 0.075f;
                    }
                }
            }
            else
            {
                if (velocity == Vector2.Zero)
                    playerIdle.DrawFrame(spriteBatch, characterPos, characterDir);
                else if (velocity.X < -DashThreshold || velocity.Y < -DashThreshold || velocity.X > DashThreshold || velocity.Y > DashThreshold)
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
                    playerWalk.DrawFrame(spriteBatch, characterDir);
                }
            }
        }
        public Rectangle getCollision()
        {
            return new Rectangle((int)characterPos.X, (int)characterPos.Y, 16, 16);
        }
        public Rectangle getSwingCollision()
        {
            // to get character center
            Vector2 centered = playerIdle.Position + new Vector2(8, 4);
            if (characterDir == Facing.Down)
                centered += new Vector2(0, 16);
            if (characterDir == Facing.Left)
                centered -= new Vector2(16, 0);
            if (characterDir == Facing.Up)
                centered -= new Vector2(0, 16);
            if (characterDir == Facing.Right)
                centered += new Vector2(16, 0);

            int gridX = ((int)centered.X / 16) * 16;
            int gridY = ((int)centered.Y / 16) * 16;
            return new Rectangle(gridX, gridY, 16, 16);
        }
    }
}

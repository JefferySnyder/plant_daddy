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
        public Vector2 pos;
        public Facing dir;
        public float opacity;
        public Ghost(Vector2 position, float opacity, Facing direction)
        {
            pos = position;
            this.opacity = opacity;
            dir = direction;
        }
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

        public int inventory = 0;
        public int points = 10;
        float cooldowntime = 1;
        KeyboardState currentKeyState;
        KeyboardState previousKeyState;
        MouseState currentMouseState;
        MouseState previousMouseState;
        public bool IsSwinging;
        Rectangle initialSwingCollision;
        public bool AlreadyBrokeSomething;
        public bool AlreadyPlacedSomething;
        public bool IsHoldingWater;
        public List<AnimatedTexture> CarriedItems = new List<AnimatedTexture>();

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
        float addGhostCooldown = 0;
        Vector2 preDashPos; 
        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            GetInput(elapsed);

            ApplyPhysics(elapsed);

            addGhostCooldown += elapsed;
            if (velocity.X < -DashThreshold || velocity.Y < -DashThreshold || velocity.X > DashThreshold || velocity.Y > DashThreshold)
            {
                if (Math.Abs(characterPos.X - preDashPos.X) >= 12 || Math.Abs(characterPos.Y - preDashPos.Y) >= 12)
                {
                    ghosts.Add(new Ghost(characterPos, 0.5f, characterDir + 4));
                    preDashPos = characterPos;
                }
            }
            else
            {
                preDashPos = characterPos;
            }
            int end = ghosts.Count;
            for (int i = 0; i < end; i++)
            {
                var ghost = ghosts[i];
                if (ghost.opacity <= 0)
                {
                    ghosts.Remove(ghost);
                    end--;
                }
            }

            playerIdle.UpdateFrame(elapsed, characterPos);
            playerWalk.UpdateFrame(elapsed, characterPos);
            playerSwing.UpdateFrame(elapsed, characterPos - playerSwingOffset);

            Xmovement = 0f;
            Ymovement = 0f;

            //Debug.WriteLine(characterPos.ToString());

        }
        private void GetInput(float elapsed)
        {
            previousKeyState = currentKeyState;
            currentKeyState = Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            Xmovement = 0f; Ymovement = 0f;

            //if (kstate.IsKeyDown(Keys.Space))
            if (currentKeyState.IsKeyDown(Keys.Up) || currentKeyState.IsKeyDown(Keys.W))
            {
                Ymovement += -1f;
                characterDir = Facing.Up;
            }
            if (currentKeyState.IsKeyDown(Keys.Down) || currentKeyState.IsKeyDown(Keys.S))
            {
                Ymovement += 1f;
                characterDir = Facing.Down;
            }
            if (currentKeyState.IsKeyDown(Keys.Left) || currentKeyState.IsKeyDown(Keys.A))
            {
                Xmovement += -1f;
                characterDir = Facing.Left;
            }
            if (currentKeyState.IsKeyDown(Keys.Right) || currentKeyState.IsKeyDown(Keys.D))
            {
                Xmovement += 1f;
                characterDir = Facing.Right;
            }

            if (Math.Abs(Xmovement) + Math.Abs(Ymovement) == 2f)
            {
                Xmovement -= Xmovement * 0.25f;
                Ymovement -= Ymovement * 0.25f;
            }
            //if (Math.Abs(Xmovement) + Math.Abs(Ymovement) == 2f)
            //    (Xmovement, Ymovement) = Vector2.Normalize(new Vector2(Xmovement, Ymovement));
            //Debug.WriteLine(Math.Abs(Xmovement) + Math.Abs(Ymovement));

            if (currentKeyState.IsKeyDown(Keys.Space) && !previousKeyState.IsKeyDown(Keys.Space))
            {
                Xmovement *= DashSpeed;
                Ymovement *= DashSpeed;
            }

            cooldowntime += elapsed;

            if (cooldowntime > 0.5)
            {
                IsSwinging = false;
                playerSwing.Pause();
            }
            // Swing = J
            if (cooldowntime >= 0.5 && (currentKeyState.IsKeyDown(Keys.J) || currentMouseState.LeftButton == ButtonState.Pressed))
            {
                IsSwinging = true;
                AlreadyBrokeSomething = false;
                playerSwing.Reset();
                playerSwing.Play();
                cooldowntime = 0;
                initialSwingCollision = getSwingCollision();
            }
            if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton != ButtonState.Pressed)
            {
                AlreadyPlacedSomething = false;
            }

            if (currentMouseState.ScrollWheelValue != previousMouseState.ScrollWheelValue)
            {
                inventory = -1 * (currentMouseState.ScrollWheelValue / 120 % 4);
                if (inventory < 0)
                    inventory += 4;

                //Debug.WriteLine("Inventory: " + inventory);
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
            if (IsSwinging)
            {
                playerSwing.DrawFrame(spriteBatch, characterDir);
                if (velocity.X < -DashThreshold || velocity.Y < -DashThreshold || velocity.X > DashThreshold || velocity.Y > DashThreshold)
                {
                    foreach (var ghost in ghosts)
                    {
                        playerDash.DrawFrame(spriteBatch, ghost.pos + new Vector2(0, -4), characterDir + 4, Color.White * ghost.opacity);
                        //if (characterDir == Facing.Left || characterDir == Facing.Right)
                        //    playerDash.DrawFrame(spriteBatch, ghost.pos + new Vector2(0,-4), characterDir + 4, Color.White * ghost.opacity);
                        //else
                        //    playerDash.DrawFrame(spriteBatch, ghost.pos + new Vector2(16, 18), characterDir + 4, Color.White * ghost.opacity);
                        ghost.opacity -= 0.075f;
                    }
                }
            }
            else
            {
                if (velocity == Vector2.Zero)
                    playerIdle.DrawFrame(spriteBatch, characterPos, characterDir);
                else if (velocity.X < -DashThreshold || velocity.Y < -DashThreshold || velocity.X > DashThreshold || velocity.Y > DashThreshold)
                    playerDash.DrawFrame(spriteBatch, characterPos, characterDir);
                else
                    playerWalk.DrawFrame(spriteBatch, characterDir);
                foreach (var ghost in ghosts)
                {
                    playerDash.DrawFrame(spriteBatch, ghost.pos, ghost.dir, Color.White * ghost.opacity);
                    ghost.opacity -= 0.015f;
                }
            }
        }
        public Rectangle getCollision()
        {
            int gridX = (int)(characterPos.X + 8) / 16 * 16;
            int gridY = (int)(characterPos.Y + 8) / 16 * 16;
            return new Rectangle(gridX, gridY, 16, 16);
        }
        public Rectangle getPlacementCollision()
        {
            Rectangle playerArea = new Rectangle((int)characterPos.X - 16, (int)characterPos.Y - 16, 48, 48);
            if (playerArea.Intersects(getMouseCollision()))
                return getMouseCollision();
            return getCollision();
        }
        //public Rectangle getSwingCollision()
        //{
        //    // to get character center
        //    //Vector2 centered = playerIdle.Position + new Vector2(8, 4);
        //    Rectangle playerPos = playerIdle.Rect();
        //    Point centered = playerPos.Center;
        //    Point mousePos = currentMouseState.Position;
        //    Vector2 floatedMousePos = new(mousePos.X, mousePos.Y);

        //    var screen = new Vector2(Game1.screenWidth, Game1.screenHeight);
        //    var game = new Vector2(Game1.gameWidth, Game1.gameHeight);
        //    var res = screen / game;
        //    floatedMousePos.X /= res.X;
        //    floatedMousePos.Y /= res.Y;

        //    if (floatedMousePos.Y > playerPos.Top) // Up
        //        centered += new Point(0, 16);
        //    if (floatedMousePos.Y < playerPos.Bottom) // Down
        //        centered -= new Point(0, 16);
        //    if (floatedMousePos.X < playerPos.Left) // Left
        //        centered -= new Point(16, 0);
        //    if (floatedMousePos.X > playerPos.Right) // Right
        //        centered += new Point(16, 0);

        //    int gridX = centered.X / 16 * 16;
        //    int gridY = centered.Y / 16 * 16;
        //    return new Rectangle(gridX, gridY, 16, 16);
        //}
        public Rectangle getSwingCollision()
        {
            Rectangle playerArea = new Rectangle((int)characterPos.X - 16, (int)characterPos.Y - 16, 48, 48);
            if (playerArea.Intersects(getMouseCollision()))
                return getMouseCollision();
            return playerArea;
        }
        public Rectangle getMouseCollision()
        {
            Point mousePos = currentMouseState.Position;
            Vector2 floatedMousePos = new(mousePos.X, mousePos.Y);

            var screen = new Vector2(Game1.screenWidth, Game1.screenHeight);
            var game = new Vector2(Game1.gameWidth, Game1.gameHeight);
            var res = screen / game;
            floatedMousePos.X /= res.X;
            floatedMousePos.Y /= res.Y;

            int gridX = (int)floatedMousePos.X / 16 * 16;
            int gridY = (int)floatedMousePos.Y / 16 * 16;
            return new Rectangle(gridX, gridY, 16, 16);
        }
        public Vector2 GetVelocity()
        {
            return velocity;
        }
    }
}

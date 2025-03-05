using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Project1
{
    public class Game1 : Game
    {
        bool showDebugBox = true;

        Player player;
        Lettuce lettuce;
        RepeatedItem soilPot;
        RepeatedItem hydroPot;
        RepeatedItem waterCan;

        private TileMap.TileMap map;

        KeyboardState currentKeyState;
        KeyboardState previousKeyState;
        MouseState currentMouseState;
        MouseState previousMouseState;

        bool isPouring;

        SpriteFont font1;
        Vector2 fontPos;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public static GraphicsDevice graphicsDevice;

        // To sizeup view
        private RenderTarget2D renderTarget;
        public const int gameWidth = 400;
        public const int gameHeight = 225;
        public const int screenWidth = 1440;
        public const int screenHeight = 810;
        private Rectangle upScaledResolution = new (0, 0, screenWidth, screenHeight);

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = screenWidth,
                PreferredBackBufferHeight = screenHeight,
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }


        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            player = new();
            player.Initialize();
            map = new TileMap.TileMap();
            lettuce = new Lettuce();
            lettuce.Initialize();
            soilPot = new RepeatedItem();
            soilPot.Initialize();
            hydroPot = new RepeatedItem();
            hydroPot.Initialize();
            waterCan = new RepeatedItem();
            waterCan.Initialize();

            renderTarget = new RenderTarget2D(GraphicsDevice, gameWidth, gameHeight);
            graphicsDevice = GraphicsDevice;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            player.Load(Content);
            map.Load(Content, "ground-tiles");
            lettuce.Load(Content, "Lettuce_Growth");
            soilPot.Load(Content, "Soil_Pot");
            for (int i = 1; i < 5; i++)
            {
                AnimatedTexture tempAT = new AnimatedTexture(Vector2.Zero, 0.5f);
                tempAT.LoadWithoutContent(soilPot.texture, 2, 0, 2);
                tempAT.Position = new Vector2(gameWidth / 4 / 16 * 16, gameHeight / i / 2 / 16 * 16);
                soilPot.items.Add(tempAT);
            }
            hydroPot.Load(Content, "Hydro_Pot");

            soilPot.Load(Content, "Water_Can");
            waterCan.Load(Content, "Water_Can");
            AnimatedTexture waterCanAT = new AnimatedTexture(Vector2.Zero, 0.5f);
            waterCanAT.LoadWithoutContent(waterCan.texture, 2, 0, 3);
            waterCanAT.NextFrame();
            waterCanAT.Position = new Vector2(gameWidth / 2 / 16 * 16, gameHeight / 2 / 16 * 16);
            waterCan.items.Add(waterCanAT);
            //waterCan.Position = new Vector2(gameWidth / 2, gameHeight / 2);

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            font1 = Content.Load<SpriteFont>("DefaultFont");
            fontPos = new Vector2(5, 5);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            previousKeyState = currentKeyState;
            currentKeyState = Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            // TODO: Add your update logic here
            player.Update(gameTime);

            lettuce.Update(gameTime, soilPot);
            lettuce.Update(gameTime, hydroPot);

            ItemBreakingConditions(lettuce.lettuce, true);
            //ItemBreakingConditions(soilPot);
            //ItemBreakingConditions(hydroPot);

            ItemCarryLogic(gameTime, soilPot.items);
            ItemCarryLogic(gameTime, waterCan.items);

            if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton != ButtonState.Pressed)
            {
                bool occupied = false;
                foreach (var lettuce in lettuce.lettuce.items)
                {
                    if (lettuce.Rect().Location == player.getPlacementCollision().Location)
                        occupied = true;
                }
                bool valid = false;
                foreach (var pot in soilPot.items)
                {
                    if (pot.Rect().Location == player.getPlacementCollision().Location)
                    {
                        valid = true;
                    }
                }
                if (waterCan.items[0].IsBeingCarried)
                {
                    foreach (var pot in soilPot.items)
                    {
                        if (pot.Rect().Location == player.getPlacementCollision().Location)
                        {
                            pot.NextFrame();
                            isPouring = true;
                        }
                    }
                }
                else if (!occupied && valid && !player.AlreadyPlacedSomething && player.points > 0)
                {
                    AnimatedTexture tempAT = new AnimatedTexture(Vector2.Zero, 0.5f);
                    tempAT.LoadWithoutContent(lettuce.lettuce.texture, 5, 5, 2);
                    tempAT.Position = new Vector2(player.getPlacementCollision().Location.X,
                                                  player.getPlacementCollision().Location.Y);
                    lettuce.lettuce.items.Add(tempAT);
                    player.points--;
                }
            }
            isPouring = false;

            base.Update(gameTime);
        }
        private void ItemCarryLogic(GameTime gameTime, List<AnimatedTexture> itemList)
        {
            int end = itemList.Count;
            for (int i = 0; i < end; i++)
            {
                var item = itemList[i];
                if (item.IsHighlighted && player.playerSwing.GetFrame() == 4 && !player.AlreadyBrokeSomething && !player.IsCarryingItem)
                {
                    //soilPot.items.RemoveAt(i);
                    player.IsCarryingItem = true;
                    item.IsBeingCarried = true;
                    player.AlreadyBrokeSomething = true;
                    //end--;
                }
                if (item.IsBeingCarried)
                {
                    //var playerAnimation = player.GetVelocity() == Vector2.Zero ? player.playerIdle : player.playerWalk;
                    bool IsWalking = player.GetVelocity() == Vector2.Zero ? false : true;
                    int yOffset = 0;
                    if (player.IsSwinging)
                    {
                        if (player.playerSwing.GetFrame() == 0) yOffset = 1;
                        if (player.playerSwing.GetFrame() == 1) yOffset = -4;
                        if (player.playerSwing.GetFrame() == 2) yOffset = -4;
                        if (player.playerSwing.GetFrame() == 3) yOffset = -3;
                        if (player.playerSwing.GetFrame() == 4) yOffset = -8;
                        if (player.playerSwing.GetFrame() == 5) yOffset = -8;
                        if (player.playerSwing.GetFrame() == 6) yOffset = -3;
                    }
                    else
                    {
                        if (!IsWalking && player.playerIdle.GetFrame() == 1) yOffset = 1;
                        if (!IsWalking && player.playerIdle.GetFrame() == 2) yOffset = -1;
                        if (!IsWalking && player.playerIdle.GetFrame() == 3) yOffset = -1;
                        if (IsWalking && player.playerWalk.GetFrame() == 1) yOffset = 1;
                        if (IsWalking && player.playerWalk.GetFrame() == 3) yOffset = -1;
                        if (IsWalking && player.playerWalk.GetFrame() == 5) yOffset = 1;
                        if (IsWalking && player.playerWalk.GetFrame() == 7) yOffset = -1;
                    }
                    item.UpdateFrame((float)gameTime.ElapsedGameTime.TotalSeconds, player.characterPos + new Vector2(0, -16 + yOffset ));

                    bool occupied = false;
                    foreach (var pot in soilPot.items)
                    {
                        if (pot.Rect().Location == player.getPlacementCollision().Location)
                        {
                            occupied = true;
                        }
                    }
                    if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton != ButtonState.Pressed && !occupied)
                    {
                        player.IsCarryingItem = false;
                        item.IsBeingCarried = false;
                        item.UpdateFrame((float)gameTime.ElapsedGameTime.TotalSeconds, new Vector2(player.getPlacementCollision().Location.X, player.getPlacementCollision().Location.Y));
                        player.AlreadyPlacedSomething = true;
                    }
                }
            }
        }
        private void ItemBreakingConditions(RepeatedItem repeatedItem, bool getPoints = false)
        {
            int end = repeatedItem.items.Count;
            for (int i = 0; i < end; i++)
            {
                var item = repeatedItem.items[i];
                //if (player.isSwinging && player.getSwingCollision().Intersects(item.Rect()) && player.playerSwing.GetFrame() == 4)
                if (item.IsHighlighted && player.playerSwing.GetFrame() == 4 && !player.AlreadyBrokeSomething)
                {
                    repeatedItem.items.RemoveAt(i);
                    player.AlreadyBrokeSomething = true;
                    end--;
                    if (getPoints)
                    {
                        player.points += item.GetFrame() + 1;
                        Debug.WriteLine("Player points: " + player.points);
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            Color color = Color.Transparent;
            if (showDebugBox)
                color = Color.DarkSlateGray;
            Texture2D _texture = new Texture2D(GraphicsDevice, 1, 1);
            _texture.SetData(new Color[] { color });

            map.Draw(_spriteBatch);
            //foreach (var i in lettuce.items)
            //{
            //    _spriteBatch.Draw(_texture, i.Rect(), Color.White);
            //}
            //if (player.IsSwinging)
            _spriteBatch.Draw(_texture, player.getSwingCollision(), Color.White);
            _spriteBatch.Draw(_texture, player.getPlacementCollision(), Color.Black);
            soilPot.Draw(_spriteBatch, player.getSwingCollision(), lettuce.lettuce);
            hydroPot.Draw(_spriteBatch, player.getSwingCollision(), lettuce.lettuce);
            lettuce.lettuce.Draw(_spriteBatch, player.getSwingCollision());
            waterCan.Draw(_spriteBatch, player.getSwingCollision());
            player.Draw(_spriteBatch);

            string output = player.points.ToString();
            // Find the center of the string
            Vector2 FontOrigin = font1.MeasureString(output) / 2;
            // Draw the string
            _spriteBatch.DrawString(font1, output, fontPos, Color.LightGreen,
                0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);

            _spriteBatch.End();

            // Upscale resolution via target Rectangle
            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(renderTarget, upScaledResolution, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

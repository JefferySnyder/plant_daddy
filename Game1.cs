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
        bool showDebugBox = false;

        Player player;
        RepeatedItem lettuce;
        RepeatedItem soilPot;
        RepeatedItem hydroPot;

        private TileMap.TileMap map;

        KeyboardState currentKeyState;
        KeyboardState previousKeyState;
        MouseState currentMouseState;
        MouseState previousMouseState;

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
            lettuce = new RepeatedItem();
            lettuce.Initialize();
            soilPot = new RepeatedItem();
            soilPot.Initialize();
            hydroPot = new RepeatedItem();
            hydroPot.Initialize();

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
            hydroPot.Load(Content, "Hydro_Pot");
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

            int end = lettuce.items.Count;
            for (int i = 0; i < end; i++)
            {
                var item = lettuce.items[i];
                foreach (var pot in soilPot.items)
                {
                    if (pot.Rect().Location == item.Rect().Location && pot.GetFrame() == 1)
                    {
                        item.growthCountdown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (item.growthCountdown <= 0)
                        {
                            item.NextFrame();
                            item.growthCountdown = 1f;
                            pot.SetFrame(0);
                        }
                    }
                }
                foreach (var pot in hydroPot.items)
                {
                    if (pot.Rect().Location == item.Rect().Location && pot.GetFrame() == 1)
                    {
                        item.growthCountdown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (item.growthCountdown <= 0)
                        {
                            item.NextFrame();
                            item.growthCountdown = 1f;

                            if (item.GetFrame() == 4)
                                pot.SetFrame(0);
                        }
                    }
                }
                //lettuce.items[i].UpdateFrame((float)gameTime.ElapsedGameTime.TotalSeconds, lettuce.items[i].Position);
            }

            ItemBreakingConditions(lettuce, true);
            ItemBreakingConditions(soilPot);
            ItemBreakingConditions(hydroPot);

            // Place lettuce = K
            if (currentKeyState.IsKeyDown(Keys.K) && !previousKeyState.IsKeyDown(Keys.K) ||
                currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton != ButtonState.Pressed) 
            {
                bool occupied = false;
                switch (player.inventory)
                {
                    case 0:
                        foreach (var lettuce in lettuce.items)
                        {
                            if (lettuce.Rect().Location == player.getSwingCollision().Location)
                                occupied = true;
                        }
                        bool valid = false;
                        foreach (var pot in soilPot.items)
                        {
                            if (pot.Rect().Location == player.getSwingCollision().Location)
                            {
                                valid = true;
                            }
                        }
                        foreach (var pot in hydroPot.items)
                        {
                            if (pot.Rect().Location == player.getSwingCollision().Location)
                            {
                                valid = true;
                            }
                        }
                        if (!occupied && valid)
                        {
                            AnimatedTexture tempAT = new AnimatedTexture(Vector2.Zero, 0.5f);
                            tempAT.LoadWithoutContent(lettuce.texture, 5, 5, 2);
                            tempAT.Position = new Vector2(player.getSwingCollision().Location.X,
                                                          player.getSwingCollision().Location.Y);
                            lettuce.items.Add(tempAT);
                        }
                        break;
                    case 1:
                        foreach (var item in soilPot.items)
                        {
                            if (item.Rect().Location == player.getSwingCollision().Location)
                                occupied = true;
                        }
                        if (!occupied)
                        {
                            AnimatedTexture tempAT = new AnimatedTexture(Vector2.Zero, 0.5f);
                            tempAT.LoadWithoutContent(soilPot.texture, 2, 0, 2);
                            tempAT.Position = new Vector2(player.getSwingCollision().Location.X,
                                                          player.getSwingCollision().Location.Y);
                            soilPot.items.Add(tempAT);
                        }
                        break;
                    case 2:
                        foreach (var item in hydroPot.items)
                        {
                            if (item.Rect().Location == player.getSwingCollision().Location)
                                occupied = true;
                        }
                        foreach (var item in soilPot.items)
                        {
                            if (item.Rect().Location == player.getSwingCollision().Location)
                                occupied = true;
                        }
                        if (!occupied)
                        {
                            AnimatedTexture tempAT = new AnimatedTexture(Vector2.Zero, 0.5f);
                            tempAT.LoadWithoutContent(hydroPot.texture, 2, 0, 2);
                            tempAT.Position = new Vector2(player.getSwingCollision().Location.X,
                                                          player.getSwingCollision().Location.Y);
                            hydroPot.items.Add(tempAT);
                        }
                        break;
                    case 3:
                        foreach (var pot in soilPot.items)
                        {
                            if (pot.Rect().Location == player.getSwingCollision().Location)
                            {
                                pot.NextFrame();
                            }
                        }
                        foreach (var pot in hydroPot.items)
                        {
                            if (pot.Rect().Location == player.getSwingCollision().Location)
                            {
                                pot.NextFrame();
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            base.Update(gameTime);
        }
        private void ItemBreakingConditions(RepeatedItem repeatedItem, bool getPoints = false)
        {
            int end = repeatedItem.items.Count;
            for (int i = 0; i < end; i++)
            {
                var item = repeatedItem.items[i];
                //if (player.isSwinging && player.getSwingCollision().Intersects(item.Rect()) && player.playerSwing.GetFrame() == 4)
                if (item.IsHighlighted && player.playerSwing.GetFrame() == 4 && !player.AlreadyBrokeSomthing)
                {
                    repeatedItem.items.RemoveAt(i);
                    player.AlreadyBrokeSomthing = true;
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
            if (player.isSwinging)
                _spriteBatch.Draw(_texture, player.getSwingCollision(), Color.White);
            soilPot.Draw(_spriteBatch, player.getSwingCollision(), lettuce);
            hydroPot.Draw(_spriteBatch, player.getSwingCollision(), lettuce);
            lettuce.Draw(_spriteBatch, player.getSwingCollision());
            player.Draw(_spriteBatch);
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

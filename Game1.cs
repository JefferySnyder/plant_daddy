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
        RepeatedItem lettuce;
        RepeatedItem pot;

        private TileMap.TileMap map;

        KeyboardState currentKeyState;
        KeyboardState previousKeyState;

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
            pot = new RepeatedItem();
            pot.Initialize();

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
            pot.Load(Content, "Soil_Pot");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            previousKeyState = currentKeyState;
            var kstate = Keyboard.GetState();
            currentKeyState = kstate;

            // TODO: Add your update logic here
            player.Update(gameTime);

            int end = lettuce.items.Count;
            for (int i = 0; i < end; i++)
            {
                lettuce.items[i].UpdateFrame((float)gameTime.ElapsedGameTime.TotalSeconds, lettuce.items[i].Position);
                if (player.isSwinging && player.getSwingCollision().Intersects(lettuce.items[i].Rect()) && player.playerSwing.GetFrame() == 4)
                {
                    lettuce.items.RemoveAt(i);
                    end--;
                }
            }

            if (kstate.IsKeyDown(Keys.K) && previousKeyState.IsKeyDown(Keys.K))
            {
                bool occupied = false;
                foreach (var lettuce in lettuce.items)
                {
                    if (lettuce.Rect().Location == player.getSwingCollision().Location)
                        occupied = true;
                }
                bool valid = false;
                foreach (var pot in pot.items)
                {
                    if (pot.Rect().Location == player.getSwingCollision().Location)
                    {
                        valid = true;
                    }
                }
                if (!occupied && valid)
                {
                    AnimatedTexture tempAT = new AnimatedTexture(Vector2.Zero, 0.5f);
                    tempAT.LoadWithoutContent(lettuce.texture, 5, 5, 1);
                    tempAT.Position = new Vector2(player.getSwingCollision().Location.X,
                                                  player.getSwingCollision().Location.Y);
                    lettuce.items.Add(tempAT);
                }
            }
            if (kstate.IsKeyDown(Keys.L) && previousKeyState.IsKeyDown(Keys.L))
            {
                bool occupied = false;
                foreach (var item in pot.items)
                {
                    if (item.Rect().Location == player.getSwingCollision().Location)
                        occupied = true;
                }
                if (!occupied)
                {
                    AnimatedTexture tempAT = new AnimatedTexture(Vector2.Zero, 0.5f);
                    tempAT.LoadWithoutContent(pot.texture, 2, 0, 1);
                    tempAT.Position = new Vector2(player.getSwingCollision().Location.X,
                                                  player.getSwingCollision().Location.Y);
                    pot.items.Add(tempAT);
                }
            }
            // change pot animation
            if (kstate.IsKeyDown(Keys.OemSemicolon) && previousKeyState.IsKeyDown(Keys.OemSemicolon))
            {
                foreach (var pot in pot.items)
                {
                    if (pot.Rect().Location == player.getSwingCollision().Location)
                    {
                        pot.NextFrame();
                    }
                }
            }


            base.Update(gameTime);
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
            pot.Draw(_spriteBatch);
            lettuce.Draw(_spriteBatch);
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

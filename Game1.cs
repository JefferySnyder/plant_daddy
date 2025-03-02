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

        private TileMap.TileMap map;

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
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            player.Update(gameTime);

            int end = lettuce.items.Count;
            for (int i = 0; i < end; i++)
            {
                if (player.isSwinging && player.getSwingCollision().Intersects(lettuce.items[i].Rect()) && player.playerSwing.GetFrame() == 4)
                {
                    lettuce.items.RemoveAt(i);
                    end--;
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.K))
            {
                AnimatedTexture tempAT = new AnimatedTexture(Vector2.Zero, 0.5f);
                tempAT.LoadWithoutContent(5, 0, 1);
                tempAT.Texture = lettuce.texture;
                tempAT.Position = new Vector2(player.getSwingCollision().Location.X,
                                              player.getSwingCollision().Location.Y);
                lettuce.items.Add(tempAT);
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
            foreach (var i in lettuce.items)
            {
                _spriteBatch.Draw(_texture, i.Rect(), Color.White);
            }
            if (player.isSwinging)
                _spriteBatch.Draw(_texture, player.getSwingCollision(), Color.White);
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

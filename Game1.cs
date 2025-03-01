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
        AnimatedTexture lettuce;
        Vector2 lettucePos;

        Player player;

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
            lettuce = new(Vector2.Zero, 0.5f);
            lettucePos = new(gameWidth / 4, gameHeight / 4);

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
            lettuce.Load(Content, "Lettuce_Growth", 5, 0, 1);
            lettuce.SetFrame(4);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            player.Update(gameTime);

            lettuce.UpdateFrame((float)gameTime.ElapsedGameTime.TotalSeconds, lettucePos);

            //var lettuceRectangle = new Rectangle((int)lettuce.Position.X, (int)lettuce.Position.Y, 16, 16);
            //if (player.getSwingCollision().Intersects(new Rectangle((int)lettuce.Position.X, (int)lettuce.Position.Y, 16, 16))) ;
            if (player.isSwinging && player.getSwingCollision().Intersects(lettuce.Rect()))
                lettuce.IsAlive = false;
            if (Keyboard.GetState().IsKeyDown(Keys.R))
                lettuce.IsAlive = true;

            //Debug.WriteLine("player: " + player.getSwingCollision());
            //Debug.WriteLine("lettuce:" + new Rectangle((int)lettuce.Position.X, (int)lettuce.Position.Y, 16, 16));

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            map.Draw(_spriteBatch);
            player.Draw(_spriteBatch);
            if (lettuce.IsAlive)
            {
                lettuce.DrawFrame(_spriteBatch);
            }
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Project1
{
    public class Game1 : Game
    {
        bool showDebugBox = false;

        Player player;
        RepeatedItem lettuce;
        RepeatedItem soilPot;
        RepeatedItem trayPot;
        RepeatedItem waterCan;
        bool validToPlace = true;

        List<RepeatedItem> repeatedItems;

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
            lettuce = new RepeatedItem();
            lettuce.Initialize();
            soilPot = new RepeatedItem();
            soilPot.Initialize();
            trayPot = new RepeatedItem();
            trayPot.Initialize();
            waterCan = new RepeatedItem();
            waterCan.Initialize();

            repeatedItems = new List<RepeatedItem> { lettuce, soilPot, trayPot, waterCan };

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
            lettuce.Load(Content, "Lettuce_Growth", 5, 0, 2);
            soilPot.Load(Content, "Soil_Pot", 2, 0, 4);
            for (int i = 1; i < 5; i++)
            {
                var pos = new Vector2(gameWidth / 4 / 16 * 16, gameHeight / i / 2 / 16 * 16);
                soilPot.AddNewItem(pos);
            }
            soilPot.items[1].AtlasRow = 1;
            trayPot.Load(Content, "Tray_Pot", 7, 0, 2);
            for (int i = 1; i < 5; i++)
            {
                var pos = new Vector2(gameWidth / 4 / 16 * 16, 128 + 16 * i);
                trayPot.AddNewItem(pos);
            }
            waterCan.Load(Content, "Water_Can", 2, 0, 3);
            var canPos = new Vector2(gameWidth / 2 / 16 * 16, gameHeight / 2 / 16 * 16);
            waterCan.AddNewItem(canPos);

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            font1 = Content.Load<SpriteFont>("DefaultFont");
            fontPos = new Vector2(5, 5);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            previousKeyState = currentKeyState;
            currentKeyState = Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            // TODO: Add your update logic here
            player.Update(gameTime);

            LettuceHighlightingLogic(lettuce);
            if (player.IsHoldingWater)
                WaterCanViewHighlightingLogic();
            else
                HighlightingLogic((float)gameTime.TotalGameTime.TotalSeconds);

            PotCarryLogic(soilPot, elapsed);
            PotCarryLogic(trayPot, elapsed);
            WaterCanCarryLogic(waterCan, elapsed);

            TrayConnectLogic(trayPot);

            LettucePlacementLogic(lettuce, soilPot);
            LettucePlacementLogic(lettuce, trayPot);
            LettuceGrowthLogic(lettuce, elapsed);
            LettuceBreakingLogic(lettuce);

        }
        private void TrayConnectLogic(RepeatedItem trays)
        {
            foreach (var item in trays.items)
            {
                var below = trayPot.items.FirstOrDefault(x => x.Position - new Vector2(0, 16) == item.Position);
                var above = trayPot.items.FirstOrDefault(x => x.Position + new Vector2(0, 16) == item.Position);
                var left = trayPot.items.FirstOrDefault(x => x.Position + new Vector2(16, 0) == item.Position);
                var right = trayPot.items.FirstOrDefault(x => x.Position - new Vector2(16, 0) == item.Position);

                if (below != null && above != null && new List<int> { 0,1,2,3 }.Any(x => x == below.GetFrame()) && new List<int> { 0,1,2,3 }.Any(x => x == above.GetFrame()) && new List<int> { 0,1,2,3 }.Any(x => x == item.GetFrame()))
                    item.SetFrame(3);
                else if (below != null && new List<int> { 0,1,2,3 }.Any(x => x == below.GetFrame()) && new List<int> { 0,1,2,3 }.Any(x => x == item.GetFrame()))
                    item.SetFrame(1);
                else if (above != null && new List<int> { 0,1,2,3 }.Any(x => x == above.GetFrame()) && new List<int> { 0,1,2,3 }.Any(x => x == item.GetFrame()))
                    item.SetFrame(2);
                else if (left != null && right != null && new List<int> { 0,4,5,6 }.Any(x => x == left.GetFrame()) && new List<int> { 0,4,5,6 }.Any(x => x == right.GetFrame()) && new List<int> { 0,4,5,6 }.Any(x => x == item.GetFrame()))
                    item.SetFrame(6);
                else if (left != null && new List<int> { 0,4,5,6 }.Any(x => x == left.GetFrame()) && new List<int> { 0,4,5,6 }.Any(x => x == item.GetFrame()))
                    item.SetFrame(4);
                else if (right != null && new List<int> { 0,4,5,6 }.Any(x => x == right.GetFrame()) && new List<int> { 0,4,5,6 }.Any(x => x == item.GetFrame()))
                    item.SetFrame(5);
                else
                    item.SetFrame(0);
            }
        }
        private void LettuceBreakingLogic(RepeatedItem lettuce)
        {
            if (player.playerSwing.GetFrame() == 4 && !player.AlreadyBrokeSomething)
            {
                int end = lettuce.items.Count;
                for (int i = 0; i < end; i++) 
                {
                    var item = lettuce.items[i];
                    if (item.IsHighlighted)
                    {
                        lettuce.items.RemoveAt(i);
                        end--;
                        player.points += item.GetFrame() + 1;
                        player.AlreadyBrokeSomething = true;
                    }
                }
            }
        }
        private void WaterCanCarryLogic(RepeatedItem items, float elapsed)
        {
            foreach (AnimatedTexture item in items.items) {
                if (item.IsHighlighted && player.playerSwing.GetFrame() == 4 && !player.AlreadyBrokeSomething && player.CarriedItems.Count == 0 && !lettuce.items.Any(x => x.IsHighlighted))
                {
                    player.CarriedItems.Add(item);
                    item.IsBeingCarried = true;
                    player.AlreadyBrokeSomething = true;
                    if (item.Texture == waterCan.texture)
                        player.IsHoldingWater = true;
                }
                if (item.IsBeingCarried)
                {
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
                    item.UpdateFrame(elapsed, player.characterPos + new Vector2(0, -16 + yOffset ));

                    AnimatedTexture occupyingItem = null;
                    string assetName = "";
                    validToPlace = true;
                    foreach (var repeatedItem in repeatedItems)
                    {
                        if (repeatedItem.asset == "Lettuce_Growth")
                            continue;
                        foreach (var rItem in repeatedItem.items)
                        {
                            if (rItem.IsHighlighted)
                            {
                                assetName = repeatedItem.asset;
                                occupyingItem = rItem;
                                validToPlace = false;
                            }
                            else if (rItem.Rect().Intersects(player.getPlacementCollision()) && repeatedItem.asset != "Water_Can")
                            {
                                validToPlace = false;
                            }
                        }
                    }
                    if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton != ButtonState.Pressed)
                    {
                        if (player.IsHoldingWater)
                        {
                            if (occupyingItem == null && validToPlace)
                            {
                                player.CarriedItems.Remove(item);
                                item.IsBeingCarried = false;
                                item.UpdateFrame(elapsed, new Vector2(player.getPlacementCollision().Location.X, player.getPlacementCollision().Location.Y));
                                player.AlreadyPlacedSomething = true;
                                player.IsHoldingWater = false;
                            }
                            else
                            {
                                if (assetName == "Soil_Pot" && occupyingItem.GetFrame() == 0)
                                {
                                    occupyingItem.NextFrame();
                                    player.AlreadyPlacedSomething = true;
                                }
                                else if (assetName == "Lettuce_Growth")
                                {
                                    foreach (var pot in soilPot.items)
                                    {
                                        if (pot.Rect().Location == occupyingItem.Rect().Location && pot.GetFrame() == 0)
                                        {
                                            pot.NextFrame();
                                            player.AlreadyPlacedSomething = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void PotCarryLogic(RepeatedItem items, float elapsed)
        {
            foreach (AnimatedTexture item in items.items) {
                if (item.IsHighlighted && player.playerSwing.GetFrame() == 4 && !player.AlreadyBrokeSomething && player.CarriedItems.Count == 0 && !lettuce.items.Any(x => x.IsHighlighted))
                {
                    player.CarriedItems.Add(item);
                    item.IsBeingCarried = true;
                    player.AlreadyBrokeSomething = true;
                }
                if (item.IsBeingCarried)
                {
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
                    item.UpdateFrame(elapsed, player.characterPos + new Vector2(0, -16 + yOffset ));

                    AnimatedTexture occupyingItem = null;
                    validToPlace = true;
                    foreach (var repeatedItem in repeatedItems)
                    {
                        if (repeatedItem.asset == "Lettuce_Growth")
                            continue;
                        foreach (var rItem in repeatedItem.items)
                        {
                            if (rItem.IsHighlighted)
                            {
                                occupyingItem = rItem;
                                validToPlace = false;
                            }
                            else if (rItem.Rect().Intersects(player.getPlacementCollision()) && repeatedItem.asset != items.asset)
                            {
                                validToPlace = false;
                            }
                        }
                    }
                    if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton != ButtonState.Pressed)
                    {
                        if (occupyingItem == null && validToPlace)
                        {
                            player.CarriedItems.Remove(item);
                            item.IsBeingCarried = false;
                            item.UpdateFrame(elapsed, new Vector2(player.getPlacementCollision().Location.X, player.getPlacementCollision().Location.Y));
                            player.AlreadyPlacedSomething = true;
                        }
                    }
                }
            }
        }
        private void LettucePlacementLogic(RepeatedItem objects, RepeatedItem pots)
        {
            if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton != ButtonState.Pressed)
            {
                foreach (var pot in pots.items)
                {
                    if (pot.IsHighlighted && player.points > 0 && !player.AlreadyPlacedSomething && !player.IsHoldingWater)
                    {
                        var pos = new Vector2(pot.Position.X, pot.Position.Y);
                        objects.AddNewItem(pos);
                        player.points--;
                    }
                }
            }
        }
        private void LettuceHighlightingLogic(RepeatedItem objects)
        {
            AnimatedTexture closest = null;
            foreach (var item in objects.items)
            {
                item.IsHighlighted = false;
                if (item.Rect().Intersects(player.getSwingCollision()))
                {
                    if (closest == null)
                    {
                        closest = item;
                    }
                    else
                    {
                        var result = player.getSwingCollision().Center - closest.Rect().Center;
                        var result2 = player.getSwingCollision().Center - item.Rect().Center;

                        if (Math.Abs(result2.X) + Math.Abs(result2.Y) < Math.Abs(result.X) + Math.Abs(result.Y))
                        {
                            closest.IsHighlighted = false;
                            closest = item;
                        }
                    }
                    closest.IsHighlighted = true;
                }
            }
        }
        //private void PotHighlightingLogic(RepeatedItem objects)
        private void HighlightingLogic(float elapsed)
        {
            AnimatedTexture closest = null;
            foreach (var repeatedItem in repeatedItems)
            {
                if (repeatedItem.asset == "Lettuce_Growth")
                    continue;
                foreach (var item in repeatedItem.items)
                {
                    item.IsHighlighted = false;                                                 // Only highlights if
                    if (item.Rect().Intersects(player.getSwingCollision()) &&                   // in swing radius
                        !lettuce.items.Any(x => x.Rect().Location == item.Rect().Location) &&   // no lettuce is on it
                        //!item.IsBeingCarried)
                        !((item.Texture != soilPot.texture && item.Texture != trayPot.texture) &&// is not pot or tray
                            lettuce.items.Any(x => x.IsHighlighted)) &&                         // and no lettuce is highlighted
                        !player.CarriedItems.Any(x => x.Texture != item.Texture) &&             // its the same texture as held item
                        !player.CarriedItems.Contains(item))                                    // its not being held
                    {
                        if (closest == null)
                        {
                            closest = item;
                        }
                        else
                        {
                            var result = player.getSwingCollision().Center - closest.Rect().Center;
                            var result2 = player.getSwingCollision().Center - item.Rect().Center;
                            if (Math.Abs(result2.X) + Math.Abs(result2.Y) < Math.Abs(result.X) + Math.Abs(result.Y))
                            {
                                closest.IsHighlighted = false;
                                closest = item;
                            }
                        }
                        closest.IsHighlighted = true;
                    }
                }
            }
        }
        private void WaterCanViewHighlightingLogic()
        {
            AnimatedTexture closest = null;
            foreach (var repeatedItem in repeatedItems)
            {
                if (repeatedItem.asset == "Lettuce_Growth")
                    continue;
                foreach (var item in repeatedItem.items)
                {
                    item.IsHighlighted = false;                                                                         // Only highlight if
                    if (item.Rect().Intersects(player.getSwingCollision()) &&                                           // in swing radius
                        soilPot.items.Any(x => x.Rect().Location == item.Rect().Location && x.GetFrame() != 1) &&       // is unwatered pot
                        !lettuce.items.Any(x => x.Rect().Location == item.Rect().Location && x.GetFrame() == 4) &&      // lettuce is not fully grown
                        !player.CarriedItems.Contains(item))                                                            // is not being held
                    {
                        if (closest == null)
                            closest = item;
                        else
                        {
                            var result = player.getSwingCollision().Center - closest.Rect().Center;
                            var result2 = player.getSwingCollision().Center - item.Rect().Center;
                            if (Math.Abs(result2.X) + Math.Abs(result2.Y) < Math.Abs(result.X) + Math.Abs(result.Y))
                            {
                                closest.IsHighlighted = false;
                                closest = item;
                            }
                        }
                        closest.IsHighlighted = true;
                    }
                }
            }
        }

        private void LettuceGrowthLogic(RepeatedItem lettuce, float elapsed)
        {
            foreach (var item in lettuce.items)
            {
                foreach (var pot in soilPot.items)
                {
                    if (pot.Rect().Location == item.Rect().Location && pot.GetFrame() == 1)
                    {
                        item.growthCountdown -= elapsed;
                        if (item.growthCountdown <= 0)
                        {
                            item.NextFrame();
                            item.growthCountdown = 1f;
                            if (pot.AtlasRow == 1)
                            {
                                if (item.GetFrame() == 4)
                                    pot.SetFrame(0);
                            }
                            else
                            {
                                pot.SetFrame(0);
                            }
                        }
                    }
                }
                foreach (var tray in trayPot.items)
                {
                    if (tray.Rect().Location == item.Rect().Location && IsConnectedToWater(tray, new List<Vector2>()))
                    {
                        item.growthCountdown -= elapsed;
                        if (item.growthCountdown <= 0)
                        {
                            item.NextFrame();
                            item.growthCountdown = 1f;
                        }
                    }
                }
            }
        }
        private bool IsConnectedToWater(AnimatedTexture tray, List<Vector2> marked)
        {
            if (tray == null) return false;
            if (marked.Contains(tray.Position)) return false;
            marked.Add(tray.Position);
            if (tray.Texture == waterCan.texture) return true;

            var below = trayPot.items.FirstOrDefault(x => x.Position - new Vector2(0, 16) == tray.Position);
            var above = trayPot.items.FirstOrDefault(x => x.Position + new Vector2(0, 16) == tray.Position);
            var left = trayPot.items.FirstOrDefault(x => x.Position + new Vector2(16, 0) == tray.Position);
            var right = trayPot.items.FirstOrDefault(x => x.Position - new Vector2(16, 0) == tray.Position);

            if (below == null) below = waterCan.items.FirstOrDefault(x => x.Position - new Vector2(0, 16) == tray.Position);
            if (above == null) above = waterCan.items.FirstOrDefault(x => x.Position + new Vector2(0, 16) == tray.Position);
            if (left == null) left = waterCan.items.FirstOrDefault(x => x.Position + new Vector2(16, 0) == tray.Position);
            if (right == null) right = waterCan.items.FirstOrDefault(x => x.Position - new Vector2(16, 0) == tray.Position);

            //if (tray.GetFrame() == 0 && (above != null || below != null || left != null || right != null))
            //{
            //    if (above.Texture == waterCan.texture || below.Texture == waterCan.texture || left.Texture == waterCan.texture || right.Texture == waterCan.texture)
            //        return true;
            //}
            //if (tray.GetFrame() == 1)
            //{
            //    return IsConnectedToWater(above);
            //}
            //if (tray.GetFrame() == 2)
            //{
            //    return IsConnectedToWater(below);
            //}
            //if (tray.GetFrame() == 4)
            //{
            //    return IsConnectedToWater(right);
            //}
            //if (tray.GetFrame() == 5)
            //{
            //    return IsConnectedToWater(left);
            //}

            if (new List<int> { 1,2,3 }.Any(x => x == tray.GetFrame()))
            {
                return IsConnectedToWater(above, marked) || IsConnectedToWater(below, marked);
            }
            if (new List<int> { 4,5,6 }.Any(x => x == tray.GetFrame()))
            {
                return IsConnectedToWater(left, marked) || IsConnectedToWater(right, marked);
            }
            if (tray.GetFrame() == 0)
            {
                return IsConnectedToWater(above, marked) || IsConnectedToWater(below, marked) || 
                       IsConnectedToWater(left, marked) || IsConnectedToWater(right, marked);
            }
            return false;
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
            Point location = player.getPlacementCollision().Location;
            if (player.CarriedItems.Count != 0 && validToPlace)
                player.CarriedItems[0].DrawFrame(_spriteBatch, new Vector2(location.X, location.Y), Facing.Down, Color.White * 0.3f);
            soilPot.Draw(_spriteBatch);
            trayPot.Draw(_spriteBatch);
            lettuce.Draw(_spriteBatch);
            waterCan.Draw(_spriteBatch);
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

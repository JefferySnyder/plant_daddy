using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1
{
    class RepeatedItem
    {
        public Texture2D texture;
        public List<AnimatedTexture> items;

        private int frameCount, framesPerSec, frameRows;
        public string asset;

        public void Initialize()
        {
            items = new List<AnimatedTexture>();
        }
        public void Load(ContentManager content, string asset, int frameCount, int framesPerSec, int frameRows)
        {
            texture = content.Load<Texture2D>(asset);
            this.asset = asset;
            this.frameCount = frameCount;
            this.framesPerSec = framesPerSec;
            this.frameRows = frameRows;
        }
        public void AddNewItem(Vector2 pos)
        {
            AnimatedTexture tempAT = new AnimatedTexture(Vector2.Zero, 0.5f);
            tempAT.LoadWithoutContent(texture, frameCount, framesPerSec, frameRows);
            tempAT.Position = pos;
            items.Add(tempAT);
        }
        public void AddNewItem(Vector2 pos, bool isTray)
        {
            AnimatedTexture tempAT = new AnimatedTexture(Vector2.Zero, 0.5f);
            tempAT.LoadWithoutContent(texture, frameCount, framesPerSec, frameRows);
            tempAT.Position = pos;
            items.Add(tempAT);
        }
        public void HighlightingLogic(Player player)
        {
            AnimatedTexture closest = null;
            foreach (var item in items)
            {
                item.IsHighlighted = false;
                if (item.Rect().Intersects(player.getSwingCollision()) &&
                    !item.IsBeingCarried)
                {
                    if (closest == null)
                    {
                        closest = item;
                    }
                    else
                    {
                        var result = player.getSwingCollision().Center - closest.Rect().Center;
                        var result2 = player.getSwingCollision().Center - item.Rect().Center;
                        Debug.WriteLine("result: " + result.ToString());
                        Debug.WriteLine("result2: " + result2.ToString());
                        if (Math.Abs(result2.X + result2.Y) < Math.Abs(result.X + result.Y))
                        {
                            closest.IsHighlighted = false;
                            closest = item;
                        }
                    }
                    closest.IsHighlighted = true;
                }
            }
        }
        public void HighlightingLogic(RepeatedItem sharedSpace, Player player)
        {
            AnimatedTexture closest = null;
            foreach (var item in items)
            {
                item.IsHighlighted = false;
                if (item.Rect().Intersects(player.getSwingCollision()) &&
                    !sharedSpace.items.Any(x => x.Rect().Location == item.Rect().Location) &&
                    //!sharedSpace.items.Any(x => x.IsHighlighted) &&
                    !item.IsBeingCarried)
                {
                    if (closest == null)
                    {
                        closest = item;
                    }
                    else
                    {
                        var result = player.getSwingCollision().Center - closest.Rect().Center;
                        var result2 = player.getSwingCollision().Center - item.Rect().Center;
                        Debug.WriteLine("result2: " + Math.Abs(result2.X + result2.Y));
                        Debug.WriteLine("result: " + Math.Abs(result.X + result.Y));
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
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var i in items)
            {
                //i.DrawFrame(spriteBatch);
                int modifier = i.IsHighlighted ? 1 : 0;
                i.DrawFrame(spriteBatch, Facing.Down + modifier);
            }
        }
        public void Draw(SpriteBatch spriteBatch, Rectangle playerSwingCollision)
        {
            foreach (var i in items)
            {
                //if (playerSwingCollision.Location == i.Rect().Location && !i.IsBeingCarried)
                //if (playerSwingCollision.Intersects(i.Rect()) && !i.IsBeingCarried)
                int modifier = i.IsHighlighted ? 1 : 0;
                i.DrawFrame(spriteBatch, Facing.Down + modifier);
            }
        }
        //public void Draw(SpriteBatch spriteBatch, Rectangle playerSwingCollision, RepeatedItem maybeOnTop)
        //{
        //    foreach (var i in items)
        //    {
        //        bool itemOnTop = false;
        //        foreach (var m in maybeOnTop.items)
        //        {
        //            if (i.Rect().Location == m.Rect().Location)
        //                itemOnTop = true;
        //        }
        //        int modifier = i.IsHighlighted ? 1 : 0;
        //        i.DrawFrame(spriteBatch, Facing.Down + modifier);
        //        //if (playerSwingCollision.Location == i.Rect().Location && !itemOnTop && !i.IsBeingCarried)
        //        //{
        //        //    i.DrawFrame(spriteBatch, i.Position, Facing.Down + 1);
        //        //    i.IsHighlighted = true;
        //        //}
        //        //else
        //        //{
        //        //    i.DrawFrame(spriteBatch, i.Position);
        //        //    i.IsHighlighted = false;
        //        //}
        //    }
        //}
    }
}

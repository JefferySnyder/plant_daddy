using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1
{
    class RepeatedItem
    {
        public Texture2D texture;
        public List<AnimatedTexture> items;

        public void Initialize()
        {
            items = new List<AnimatedTexture>();
        }
        public void Load(ContentManager content, string asset)
        {
            texture = content.Load<Texture2D>(asset);
        }
        public void HighlightingLogic(Lettuce lettuce, Player player)
        {
            foreach (var pot in items)
            {
                pot.IsHighlighted = false;
                if (pot.Rect().Intersects(player.getSwingCollision()) &&
                    !lettuce.lettuce.items.Any(x => x.Rect().Location == pot.Rect().Location) &&
                    !pot.IsBeingCarried)
                {
                    pot.IsHighlighted = true;
                    break;
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

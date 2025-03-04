using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;

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
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var i in items)
            {
                i.DrawFrame(spriteBatch);
            }
        }
        public void Draw(SpriteBatch spriteBatch, Rectangle playerSwingCollision)
        {
            foreach (var i in items)
            {
                if (playerSwingCollision.Location == i.Rect().Location)
                {
                    i.DrawFrame(spriteBatch, Facing.Down + 1);
                    i.IsHighlighted = true;
                }
                else
                {
                    i.DrawFrame(spriteBatch);
                    i.IsHighlighted = false;
                }
            }
        }
        public void Draw(SpriteBatch spriteBatch, Rectangle playerSwingCollision, RepeatedItem maybeOnTop)
        {
            foreach (var i in items)
            {
                bool itemOnTop = false;
                foreach (var m in maybeOnTop.items)
                {
                    if (i.Rect().Location == m.Rect().Location)
                        itemOnTop = true;
                }
                if (playerSwingCollision.Location == i.Rect().Location && !itemOnTop)
                {
                    i.DrawFrame(spriteBatch, Facing.Down + 1);
                    i.IsHighlighted = true;
                }
                else
                {
                    i.DrawFrame(spriteBatch);
                    i.IsHighlighted = false;
                }
            }
        }
    }
}

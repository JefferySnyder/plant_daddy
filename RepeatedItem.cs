using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

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
        public void Update(GameTime gameTime)
        {
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var i in items)
            {
                i.DrawFrame(spriteBatch);
            }
        }
    }
}

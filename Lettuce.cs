using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1
{
    class Lettuce
    {
        RepeatedItem lettuce;
        public void Initialize()
        {
            lettuce = new RepeatedItem();
            lettuce.Initialize();
        }
        public void Load(ContentManager Content, string asset)
        {
            lettuce.Load(Content, asset);
        }
        public void Update(GameTime gameTime, Pot pot)
        {
        }
        public void Draw()
        {
        }
    }
}

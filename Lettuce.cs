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
        public RepeatedItem lettuce;
        public void Initialize()
        {
            lettuce = new RepeatedItem();
            lettuce.Initialize();
        }
        public void Load(ContentManager Content, string asset)
        {
            lettuce.Load(Content, asset);
        }
        public void Update(GameTime gameTime, RepeatedItem pot)
        {
            int end = lettuce.items.Count;
            for (int i = 0; i < end; i++)
            {
                var item = lettuce.items[i];
                foreach (var p in pot.items)
                {
                    if (p.Rect().Location == item.Rect().Location && p.GetFrame() == 1)
                    {
                        item.growthCountdown -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (item.growthCountdown <= 0)
                        {
                            item.NextFrame();
                            item.growthCountdown = 1f;
                            if (p.AtlasRow == 1)
                            {
                                if (item.GetFrame() == 4)
                                    p.SetFrame(0);
                            }
                            else
                            {
                                p.SetFrame(0);
                            }
                        }
                    }
                }
            }
        }
        public void Draw()
        {
        }
    }
}

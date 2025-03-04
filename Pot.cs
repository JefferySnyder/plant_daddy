using Microsoft.Xna.Framework.Content;

namespace Project1
{
    class Pot
    {
        public RepeatedItem pot;

        public void Initialize()
        {
            pot = new RepeatedItem();
            pot.Initialize();
        }
        public void Load(ContentManager Content, string asset)
        {
            pot.Load(Content, asset);
        }
        public void Update()
        {
        }
        public void Draw()
        {
        }
        public virtual void GrowLettuce(AnimatedTexture plant, AnimatedTexture potItem)
        {
            //item.NextFrame();
            //item.growthCountdown = 1f;
            //pot.SetFrame(0);
            plant.NextFrame();
            plant.growthCountdown = 1f;

            if (plant.GetFrame() == 4)
                potItem.SetFrame(0);
        }
    }
}

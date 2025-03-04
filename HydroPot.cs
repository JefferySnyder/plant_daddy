using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1
{
    internal class HydroPot : Pot
    {
        public override void GrowLettuce(AnimatedTexture plant, AnimatedTexture potItem)
        {
            plant.NextFrame();
            plant.growthCountdown = 1f;

            if (plant.GetFrame() == 4)
                potItem.SetFrame(0);
        }
    }
}

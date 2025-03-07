using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Project1
{
    public class Sprite
    {
        public Texture2D Texture;
        public Vector2 Position;
        public bool IsHighlighted = false;
        public bool IsAlive = true;
        public bool IsBeingCarried = false;


        public Sprite(Texture2D texture, Vector2 position)
        {
            Texture = texture;
            Position = position;
        }
        public Sprite() { }
    }
}

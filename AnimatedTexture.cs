using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using static Project1.Game1;

namespace Project1
{
    public class AnimatedTexture : Sprite
    {
        private int frameCount;
        private int frameRows;
        //private Texture2D myTexture;
        private float timePerFrame;
        private int frame;
        private float totalElapsed;
        private bool isPaused;

        public float Depth;
        public Vector2 Origin;

        public AnimatedTexture(Vector2 origin, float depth)
        {
            this.Origin = origin;
            this.Depth = depth;
        }

        public void Load(ContentManager content, string asset, int frameCount, int framesPerSec, int frameRows)
        {
            this.frameCount = frameCount;
            this.frameRows = frameRows;
            Texture = content.Load<Texture2D>(asset);
            timePerFrame = (float)1 / framesPerSec;
            frame = 0;
            totalElapsed = 0;
            isPaused = false;
        }

        public void UpdateFrame(float elapsed, Vector2 pos)
        {
            if (isPaused) return;
            totalElapsed += elapsed;
            if (totalElapsed > timePerFrame)
            {
                frame++;
                // Keep the Frame between 0 and the total frames, minus one.
                frame %= frameCount;
                totalElapsed -= timePerFrame;
            }
            Position = pos;
        }

        public void DrawFrame(SpriteBatch batch, Vector2 screenPos, Facing dir)
        {
            DrawFrame(batch, screenPos, dir, Color.White);
        }
        public void DrawFrame(SpriteBatch batch, Vector2 screenPos, Facing dir, Color color)
        {
            DrawFrame(batch, frame, screenPos, dir, color);
        }

        // SpriteEffects.FlipHorizontally
        public void DrawFrame(SpriteBatch batch, int frame, Vector2 screenPos, Facing dir, Color color)
        {
            int FrameWidth = Texture.Width / frameCount;
            int FrameHeight = Texture.Height / frameRows;
            Rectangle sourcerect = new (FrameWidth * frame, FrameHeight * (int)dir,
                FrameWidth, FrameHeight);
            batch.Draw(Texture, screenPos, sourcerect, color,
                0, Origin, 1, SpriteEffects.None, Depth);
        }

        public bool IsPaused
        {
            get { return isPaused; }
        }

        public void Reset()
        {
            frame = 0;
            totalElapsed = 0f;
        }

        public void Stop()
        {
            Pause();
            Reset();
        }

        public void Play()
        {
            isPaused = false;
        }

        public void Pause()
        {
            isPaused = true;
        }
    }
}

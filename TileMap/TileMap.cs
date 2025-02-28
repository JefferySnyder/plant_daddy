using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace Project1.TileMap
{
    internal class TileMap
    {
        Texture2D Texture;
        private Dictionary<Vector2, int> tilemap;
        private List<Rectangle> textureStore;

        public void Load(ContentManager content, string asset)
        {
            tilemap = LoadMap("../../../TileMap/map.csv");
            textureStore = new()
            {
                // Dirt
                new Rectangle(0, 0, 16, 16),   // 1
                new Rectangle(16, 0, 16, 16),  // 2
                new Rectangle(32, 0, 16, 16),  // 3
                new Rectangle(0, 16, 16, 16),  // 4
                new Rectangle(16, 16, 16, 16), // 5
                new Rectangle(32, 16, 16, 16), // 6
                new Rectangle(0, 32, 16, 16),  // 7
                new Rectangle(16, 32, 16, 16), // 8
                new Rectangle(32, 32, 16, 16), // 9

                // Gravel
                new Rectangle(0, 48, 16, 16),  // 10
                new Rectangle(16, 48, 16, 16), // 11
                new Rectangle(32, 48, 16, 16), // 12
                new Rectangle(0, 64, 16, 16),  // 13
                new Rectangle(16, 64, 16, 16), // 14
                new Rectangle(32, 64, 16, 16), // 15
                new Rectangle(0, 80, 16, 16),  // 16
                new Rectangle(16, 80, 16, 16), // 17
                new Rectangle(32, 80, 16, 16), // 18
            };
            Texture = content.Load<Texture2D>(asset);
        }
        private Dictionary<Vector2, int> LoadMap(string filepath)
        {
            Dictionary<Vector2, int> result = new();

            StreamReader reader = new(filepath);

            int y = 0;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(',');

                for (int x = 0; x < items.Length; x++)
                {
                    if (int.TryParse(items[x], out int value))
                    {
                        if (value > 0)
                        {
                            result[new Vector2(x, y)] = value;
                        }
                    }
                }
                y++;
            }
            return result;
        }
        public void Draw(SpriteBatch batch)
        {
            foreach (var item in tilemap)
            {
                Rectangle dest = new((int)item.Key.X * 16, (int)item.Key.Y * 16, 16, 16);
                Rectangle src = textureStore[item.Value - 1];

                batch.Draw(Texture, dest, src, Color.White);
            }

        }
    }
}

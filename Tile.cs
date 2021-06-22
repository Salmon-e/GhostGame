using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using SalmonGame.Render;
using MonoGame.Extended.Tiled;
using Microsoft.Xna.Framework.Graphics;
using SalmonGame.Objects;
namespace GhostGame
{
    public class Tile
    {
        public Texture2D texture;
        public string name;
        public Vector2 position { 
            get
            {
                return mapPosition * size;
            }
        }
        public bool horizontalFlip = false;
        public bool verticalFlip = false;
        public Vector2 mapPosition;
        public float size;
        public Tile(string name, Vector2 position)
        {
            mapPosition = position;
            if(name != Air)
                texture = Content.Tiles[name];
            this.name = name;
        }
        public void Render(Renderer r)
        {
            if (name == Air) return;
            SpriteEffects flip = (horizontalFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (verticalFlip ? SpriteEffects.FlipVertically : SpriteEffects.None);

            r.sb.Draw(texture, new Rectangle(position.ToPoint()-new Point((int)size/2), new Point((int)size)), null, Color.White, 0, Vector2.Zero, flip, 0);
        }

        public static string Stone = "stone";
        public static string Air = "air";
    }
}

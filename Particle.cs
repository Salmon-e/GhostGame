using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using SalmonGame.Objects;
using SalmonGame.Render;
namespace GhostGame
{
    public class Particle : Entity
    {
        public Vector2 size;
        public Color color;
        public Vector2 gravity;
        public int lifetime;
        RenderMethod renderMethod = new RenderMethod
        {
            render = (obj, r) =>
            {
                if (obj is Particle)
                {
                    Particle p = (Particle)obj;
                    r.sb.Draw(Content.Particle, new Rectangle(p.position.ToPoint(), p.size.ToPoint()), p.color);
                }
            },
            useSpriteBatch = true
        };
        public Particle(Vector2 position, Vector2 velocity, Color color)
        {
            renderArgs = new RenderArgs
            {
                renderMethod = renderMethod,
                renderSize = size.ToPoint()
            };
            this.position = position;
            this.velocity = velocity;
            this.color = color;
            size = new Vector2(3, 3);
            lifetime = 100;
            Health = 1;
        }
        public override void Update(GameTime time)
        {
            
            if(lifetime == 0)
            {
                Destroy();
            }            
            velocity.Y += 0.1f;
            if (onGround || onCeiling || onLeftWall || onRightWall)
            {                
                velocity.X = 0;
                velocity.Y = 0;
                doCollisions = false;
                lifetime--;
            }
            base.Update(time);
        }
    }
}

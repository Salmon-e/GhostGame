using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SalmonGame.Render;
namespace GhostGame
{
    public class Slime : Entity, IAnimated
    {
        public Animation Animation { get; set; }
        Player target;
        public Slime()
        {            
            Health = 10;
            doCollisions = true;
            hitboxSize = new Vector2(30, 18);
            renderArgs = new RenderArgs
            {
                renderSize = hitboxSize.ToPoint(),
                renderMethod = RenderMethod.animatedSprite                
            };
            Animation = Content.SlimeIdle.Copy();
            bleedColor = Color.LightGreen;
        }
        float coolDown = 3;
        public override void Update(GameTime time)
        {
            base.Update(time);
            Random r = new Random();            
            Animation.Update(time);
            velocity.Y += 0.3f;
            coolDown -= (float)time.ElapsedGameTime.TotalSeconds;
            if (onGround) velocity.X = 0;
            if (target is null)
            {
                target = (Player)manager.objects.Find(o => o is Player);
            }
            
            else if (coolDown <= 0)
            {
                if (onGround)
                {
                    velocity.Y = -5 - (float) r.NextDouble();                   
                }
                velocity.X = target.position.X > position.X ? 2 : -2;
                renderArgs.spriteEffects = velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                coolDown = 4*(float)r.NextDouble();
            }
        }
        public override void Damage(float amount, Entity attacker)
        {
            coolDown = 0.1f;
            base.Damage(amount, attacker);            
        }
    }
}

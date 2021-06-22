using System;
using System.Collections.Generic;
using System.Text;
using SalmonGame.Render;
using SalmonGame;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using SalmonGame.Objects;
using SalmonGame.Collisions;
namespace GhostGame
{
    public class Player : Entity, IAnimated
    {
        Animation idle = Content.PlayerIdle.Copy();
        Animation walk = Content.PlayerWalk.Copy();
        Animation ball = Content.PlayerBall.Copy();
        Animation jump = Content.PlayerJump.Copy();
        Animation startRoll = Content.PlayerStartRoll.Copy();
        Vector2 jumpVelocity;
        PlayerController controller;
        public bool IsBall = false;
        public int immunity;
        public float maxHealth = 10;
        public Player()
        {
            doCollisions = true;
            
            Animation = idle;
            renderArgs = new RenderArgs
            {
                renderSize = new Point(18, 33),
                renderMethod = RenderMethod.animatedSprite,
                
            };
            hitboxSize = new Vector2(24, 33);
            Health = 10;
            bleedColor = Color.Black;
        }
        public Player(Vector2 pos) : this()
        {
            position = pos;
        }
        public override void Add()
        {
            controller = new PlayerController(this);
            controller.actions[PlayerAction.Jump].action = () =>
            {
                if (onGround && !IsBall)
                {
                    jumpVelocity.Y = -10;
                    velocity.Y = 0;
                }
                if(!onGround && !IsBall)
                {
                    bufferedJumpTime = 10;
                }
            };
            controller.actions[PlayerAction.EndJump].action = () =>
            {
                jumpVelocity = Vector2.Zero;
            };
            controller.actions[PlayerAction.StartRoll].action = () =>
            {
                StartRoll();
            };
            controller.actions[PlayerAction.EndRoll].action = () =>
            {
                EndRoll();
            };
            controller.actions[PlayerAction.Left].action = () =>
            {
                 velocity.X -= IsBall ? 0.5f : 2;
            };
            controller.actions[PlayerAction.Right].action = () =>
            {
                velocity.X += IsBall ? 0.5f : 2;
            };
            Game1.instance.player = this;
            base.Add();
        }
        public override void HitWall()
        {
            if (IsBall)
            {
                if (onGround || onCeiling)
                {
                    velocity.Y *= -0.5f;
                    if(Math.Abs(velocity.Y) > 0.1f)
                    {
                        onGround = false;
                        onCeiling = false;
                    }
                }
                if (onLeftWall || onRightWall)
                    velocity.X *= -0.5f;
            }
            
            else
            {
                base.HitWall();
            }
        }
        public Animation Animation { get; set; }

        int bufferedJumpTime = 0;

        private void DetermineAnimation()
        {
            if (IsBall)
            {
                if (Animation != startRoll || startRoll.Ended)
                {
                    Animation = ball;
                    startRoll.Reset();
                }
            }
            else if (onGround && Math.Abs(velocity.X) > 0.1)
            {
                Animation = walk;
            }
            else if(!onGround)
            {
                Animation = jump;
            }
            else
            {                
                Animation = idle;
            }
            if(Animation != jump)
            {
                jump.Reset();
            }            
        }
        public void StartRoll()
        {
            if (!IsBall && !onGround)
            {
                Animation = startRoll;
            }
            if (!onGround)
                IsBall = true;
        }
        public void EndRoll()
        {
            if (IsBall)
            {
                IsBall = false;
                position.Y -= 4.5f;
                hitboxSize = new Vector2(24, 33);
                if (world.tileMap.GetIntersectedTiles(aabb).Count != 0)
                {
                    IsBall = true;
                    hitboxSize = new Vector2(24);
                    position.Y += 4.5f;
                }
            }            
        }
        public override void Damage(float amount, Entity attacker)
        {
            float direction = Math.Sign(position.X - attacker.position.X);
            Vector2 knockBack = new Vector2(5 * direction, -5f);
            velocity = knockBack;
            immunity = 30;
            base.Damage(amount, attacker);
            invincible = true;
        }
        public override void Update(GameTime time)
        {            
            base.Update(time);
            if(Health < maxHealth)
            {
                Health += 0.001f;
            }
            if (immunity != 0)
            {
                immunity--;
                invincible = true;
                if(!onGround)
                {
                    controller.focused = false;
                }
                else
                {
                    controller.focused = true;
                }
            }
            else
            {
                controller.focused = true;
                invincible = false;
            }
            
            velocity -= jumpVelocity;            
            controller.Update();
            DetermineAnimation();
            Animation.Update(time);
            if (bufferedJumpTime != 0)
            {
                bufferedJumpTime--;
            }
            if (onGround && bufferedJumpTime != 0)
            {
                jumpVelocity.Y = -10;
                bufferedJumpTime = 0;
            }

            renderArgs.renderSize = new Point(Animation.frameSize.X*3, Animation.frameSize.Y *3);
            
            foreach(GameObject o in manager.objects)
            {
                if(o is Entity && o != this)
                {
                    Entity e = (Entity)o;
                    if (!aabb.Overlap(e.aabb).Equals(AABB.Empty))
                    {
                        if (IsBall)
                        {
                            Vector2 bounce = Vector2.Normalize(position - e.position) * 7;
                            
                            velocity = bounce;
                            e.Damage(1, this);
                            e.invincible = true;
                        }
                        else if(immunity == 0)
                        {                           
                            Damage(1, e);
                        }
                    }
                    else
                    {
                        if (e.invincible)
                        {
                            e.invincible = false;
                        }
                    }
                }
            }
            
            if(velocity.X < 0)
            {
                renderArgs.spriteEffects = SpriteEffects.FlipHorizontally;
            }
            else
            {
                renderArgs.spriteEffects = SpriteEffects.None;
            }
            if (IsBall)
            {
                hitboxSize = new Vector2(24);                 
            }
            else
            {
                hitboxSize = new Vector2(24, 33);
            }
            
            if (onGround)
            {
                EndRoll();
            }                 
            
            if(controller.focused || onGround)
            {
                velocity.X *= IsBall ? 0.9f : 0.7f;
            }
            if (jumpVelocity.Y >= 0) velocity.Y += 0.3f;
            else jumpVelocity.Y += 0.3f;
            velocity += jumpVelocity;
            Game1.instance.renderer.camera.position += (position - Game1.instance.renderer.camera.position)/5;
        }
        
    }
}

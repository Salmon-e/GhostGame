using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using SalmonGame.Render;
using SalmonGame.Objects;
namespace GhostGame
{
    public class Torch : Entity, ILight, IAnimated
    {
        public Vector2 lightPosition { get; set; }
        public float intensity { get; set; }
        public Animation Animation { get; set; }

        public override void Add()
        {
            lightPosition = position;

            Game1.instance.renderer.lights.Add(this);
            Game1.instance.renderer.UpdateLights();
            base.Add();
        }
        public override void Destroy()
        {
            Game1.instance.renderer.lights.Remove(this);
            Game1.instance.renderer.UpdateLights();
            base.Destroy();
        }
        public Torch()
        {
            lightPosition = position;
            intensity = 200;
            renderArgs = new RenderArgs
            {
                renderMethod = RenderMethod.animatedSprite,
                renderSize = new Point(24, 33)                
            };
            hitboxSize = new Vector2(24, 33);
            doCollisions = false;
            Animation = Content.Torch;
            Health = 1;
        }
        public override void Update(GameTime time)
        {
            Animation.Update(time);
            base.Update(time);
        }
    }
}

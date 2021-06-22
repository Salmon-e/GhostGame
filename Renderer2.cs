using System;
using System.Collections.Generic;
using System.Text;
using SalmonGame.Render;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SalmonGame.Objects;
using System.Linq;
namespace GhostGame
{
    public class Renderer2 : Renderer
    {
        TileMap map;
        public Effect lighting;
        public Vector2 screenSize;
        Effect blur;
        public bool doLights = true;
        public float ambient = 0.6f;
        public RenderTarget2D gameplayTarget;
        public RenderTarget2D screenTarget;
        public List<ILight> lights = new List<ILight>();
        public ObjectRenderGroup entityGroup;       
        public Renderer2(SpriteBatch sb, TileMap map) : base(sb)
        {
            this.map = map;
            Game1.instance.Window.ClientSizeChanged += (o, e) =>
            {
                gameplayTarget.Dispose();
                screenTarget.Dispose();                
                gameplayTarget = new RenderTarget2D(
                graphicsDevice,
                graphicsDevice.PresentationParameters.BackBufferWidth,
                graphicsDevice.PresentationParameters.BackBufferHeight);
                screenTarget = new RenderTarget2D(
                graphicsDevice,
                graphicsDevice.PresentationParameters.BackBufferWidth,
                graphicsDevice.PresentationParameters.BackBufferHeight);                
            };
            lighting = Content.LightingEffect;
            entityGroup = new ObjectRenderGroup
            {
                filter = o => o.renderArgs.renderMethod.useSpriteBatch,
                render = (group, renderer) =>
                {
                    renderer.sb.Begin(transformMatrix: renderer.camera.view, samplerState: SamplerState.PointClamp);
                    foreach (GameObject obj in group.objects)
                    {
                        obj.renderArgs.renderMethod.render(obj, renderer);
                    }
                    renderer.sb.End();
                }
            };
            gameplayTarget = new RenderTarget2D(
                graphicsDevice,
                graphicsDevice.PresentationParameters.BackBufferWidth,
                graphicsDevice.PresentationParameters.BackBufferHeight);
            screenTarget = new RenderTarget2D(
               graphicsDevice,
               graphicsDevice.PresentationParameters.BackBufferWidth,
               graphicsDevice.PresentationParameters.BackBufferHeight);            
            objectRenderGroups.Clear();
            blur = Game1.instance.Content.Load<Effect>("blur");
            blur.Parameters["resolution"].SetValue(map.tileTexture.Bounds.Size.ToVector2());
            RenderTarget2D blurTexture = new RenderTarget2D(graphicsDevice, map.tileTexture.Width, map.tileTexture.Height, true, SurfaceFormat.Alpha8, DepthFormat.Depth16);
            graphicsDevice.SetRenderTarget(blurTexture);
            sb.Begin(effect: blur);
            sb.Draw(map.tileTexture, new Rectangle(Point.Zero, blurTexture.Bounds.Size), new Rectangle(Point.Zero, map.tileTexture.Bounds.Size), Color.White);
            sb.End();
            graphicsDevice.SetRenderTarget(null);
            lighting.Parameters["blurTileTexture"].SetValue(blurTexture);
            lighting.Parameters["tileTexture"].SetValue(map.tileTexture);
            lighting.Parameters["tileSize"].SetValue(map.TileSize);
            lighting.Parameters["tileTextureSize"].SetValue(map.tileTexture.Bounds.Size.ToVector2());
            lighting.Parameters["tileOrigin"].SetValue(map.tileOrigin);            
        }
        public override void Render()
        {
            screenSize = Game1.instance.Window.ClientBounds.Size.ToVector2();
            graphicsDevice.SetRenderTarget(gameplayTarget);
            graphicsDevice.Clear(Color.White * 0.3f);
            sb.Begin(transformMatrix: camera.view, samplerState: SamplerState.PointClamp);
            foreach(Tile t in map.GetOnScreenTiles(3))
            {
                t.Render(this);
            }
            sb.End();
            entityGroup.render(entityGroup, this);

            graphicsDevice.SetRenderTarget(null);            
            
            lighting.Parameters["screenSize"].SetValue(Game1.instance.Window.ClientBounds.Size.ToVector2());
            lighting.Parameters["zoom"].SetValue(camera.zoom);
            lighting.Parameters["cameraPos"].SetValue(camera.position);
            lighting.Parameters["ambient"].SetValue(ambient);

            graphicsDevice.SetRenderTarget(screenTarget);            
            

            sb.Begin(effect: doLights ? lighting : null, samplerState: SamplerState.PointClamp);
            sb.Draw(gameplayTarget, new Rectangle(Point.Zero, screenTarget.Bounds.Size), Color.White);            
            sb.End();
            
            if(Game1.instance.player != null && Game1.instance.player.Health > 0)
            {
                sb.Begin(samplerState: SamplerState.PointWrap);
                int healthSize = 21;
                Player player = Game1.instance.player;
                for (int i = 0; i < Math.Floor(player.maxHealth); i++)
                {
                    sb.Draw(Content.PlayerHealth, new Rectangle(new Point(healthSize, healthSize) + new Point(i * healthSize, 0), new Point(healthSize)), Color.Black);
                }
                for (int i = 0; i < Math.Floor(player.Health); i++)
                {
                    sb.Draw(Content.PlayerHealth, new Rectangle(new Point(healthSize, healthSize) + new Point(i * healthSize, 0), new Point(healthSize)), Color.White);
                }                
                sb.End();
            }
            graphicsDevice.SetRenderTarget(null);
            sb.Begin(samplerState: SamplerState.PointClamp);
            sb.Draw(screenTarget, new Rectangle(Point.Zero, Game1.instance.Window.ClientBounds.Size), Color.White);
            sb.End();
        }
        public void UpdateLights()
        {
            List<Vector3> lightList = new List<Vector3>();
            foreach (ILight light in lights)
            {
                if (lightList.Count >= 50)
                    break;
                if (Vector2.Distance(light.lightPosition, camera.position) > light.intensity + (screenSize/2).Length()/camera.zoom)
                    continue;
                lightList.Add(new Vector3(light.lightPosition, light.intensity));
            }
            lighting.Parameters["lightCount"].SetValue(lightList.Count);
            lighting.Parameters["lights"].SetValue(lightList.ToArray());
            
        }
        public void UpdateTileTextures()
        {
            map.tileTexture = map.CreateTileTexture();
            blur = Game1.instance.Content.Load<Effect>("blur");
            blur.Parameters["resolution"].SetValue(map.tileTexture.Bounds.Size.ToVector2());
            RenderTarget2D blurTexture = new RenderTarget2D(graphicsDevice, map.tileTexture.Width, map.tileTexture.Height, true, SurfaceFormat.Alpha8, DepthFormat.Depth16);
            graphicsDevice.SetRenderTarget(blurTexture);
            sb.Begin(effect: blur);
            sb.Draw(map.tileTexture, new Rectangle(Point.Zero, blurTexture.Bounds.Size), new Rectangle(Point.Zero, map.tileTexture.Bounds.Size), Color.White);
            sb.End();
            graphicsDevice.SetRenderTarget(null);
            lighting.Parameters["blurTileTexture"].SetValue(blurTexture);
            lighting.Parameters["tileTexture"].SetValue(map.tileTexture);
            lighting.Parameters["tileSize"].SetValue(map.TileSize);
            lighting.Parameters["tileTextureSize"].SetValue(map.tileTexture.Bounds.Size.ToVector2());
            lighting.Parameters["tileOrigin"].SetValue(map.tileOrigin);
        }
    }
}

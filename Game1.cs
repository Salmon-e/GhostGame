using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Content;
using SalmonGame;
using ImGuiNET;

using System;
namespace GhostGame
{
    public class Game1 : SGame
    {

        public World world;
        public static Game1 instance;
        public new Renderer2 renderer;
        public Player player;
        public bool InEditor = false;
        Editor editor;
        ImGuiRenderer guiRenderer;
        public Game1()
        {
            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 16);
        }

        protected override void Initialize()
        {
            
            instance = this;
            GhostGame.Content.Init();

            world = World.FromFile(this, "Content/tileMap.tmap");
            World.instance = world;
            base.Initialize();

            renderer = new Renderer2(new SpriteBatch(GraphicsDevice), world.tileMap);
            renderer.camera.position = world.tileMap.tileOrigin * world.tileMap.TileSize;
            
            renderer.camera.zoom = 2f;
            guiRenderer = new ImGuiRenderer(this);
            guiRenderer.RebuildFontAtlas();
            editor = new Editor();
            world.SpawnAll();

        }

        protected override void LoadContent()
        {
            World.instance.tileMap.tileTexture = world.tileMap.CreateTileTexture();

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if(gameTime.TotalGameTime.Milliseconds % 30 == 0)
                renderer.UpdateLights();
            
            if (InEditor)
            {
                editor.Update(gameTime, this);
            }
            if(!InEditor)
                base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White * 0.1f);
            renderer.Render();
            guiRenderer.BeforeLayout(gameTime);            
            if(ImGui.Button("Kill player"))
            {
                player.Kill();
            }
           
            if (ImGui.Button("Respawn player") && !objectManager.objects.Contains(player))
            {
                player = new Player(renderer.camera.position);
                player.Add();
            }
            if (ImGui.Button("Relight"))
            {
                renderer.UpdateTileTextures();
            }
            if(ImGui.Button("Save map"))
            {
                world.Save("Content/tileMap.tmap");
            }
            if(ImGui.Button("Toggle lights"))
            {
                renderer.doLights = !renderer.doLights;
            }
            if(ImGui.Button("Toggle editor"))
            {
                InEditor = !InEditor;
                if (InEditor)
                    editor.Open();
                else
                {
                    editor.UpdateSpawners();
                }
            }
            if (ImGui.Button("DEATH"))
            {
                world.DestroyEntities();
            }
            if (ImGui.Button("Life"))
            {
                world.SpawnAll();
            }
            ImGui.SliderFloat("Zoom", ref renderer.camera.zoom, 0.2f, 2);
            if (InEditor)
                editor.Render();
            guiRenderer.AfterLayout();
        }
    }
}

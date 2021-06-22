using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using SalmonGame.Render;
using Microsoft.Xna.Framework;
namespace GhostGame
{
    public static class Content
    {
        public static Texture2D BlackParticleTexture;
        public static Animation PlayerIdle;
        public static Animation PlayerWalk;
        public static Animation PlayerBall;
        public static Animation PlayerJump;
        public static Animation PlayerStartRoll;
        public static Animation SlimeIdle;
        public static Animation Torch;
        public static Effect LightingEffect;
        public static Texture2D PlayerHealth;
        public static Texture2D Particle;
        public static Dictionary<string, Texture2D> Tiles = new Dictionary<string, Texture2D>();
        static T Load<T>(string name)
        {
            return Game1.instance.Content.Load<T>(name);
        }   
        public static void Init()
        {
            BlackParticleTexture = Load<Texture2D>("black_particle");
            Particle = new Texture2D(Game1.instance.GraphicsDevice, 1, 1);
            Particle.SetData(new Color[] { Color.White });
            PlayerIdle = new Animation(Load<Texture2D>("player_idle"), new Point(6, 11), 0, 1, 0.5f, true);
            PlayerWalk = new Animation(Load<Texture2D>("player_walk"), new Point(8, 11), 0, 3, 0.1f, true);
            PlayerBall = new Animation(Load<Texture2D>("player_ball"), new Point(8, 8), 0, 2, 0.05f, true);
            PlayerJump = new Animation(Load<Texture2D>("player_jump"), new Point(8, 11), 0, 1, 0.1f, false);
            PlayerStartRoll = new Animation(Load<Texture2D>("player_start_roll"), new Point(8, 11), 0, 3, 0.07f, false);
            SlimeIdle = new Animation(Load<Texture2D>("slime_idle"), new Point(10, 6), 0, 1, 0.5f, true);
            Torch = new Animation(Load<Texture2D>("torch"), new Point(8, 11), 0, 4, 0.3f, true);
            PlayerHealth = Load<Texture2D>("health");
            LightingEffect = Load<Effect>("lighting");
            LoadTile(Tile.Stone);
        }
        static void LoadTile(string name)
        {
            Tiles.Add(name, Load<Texture2D>(name));
        }
    }
}

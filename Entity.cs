using System;
using System.Collections.Generic;
using System.Text;
using SalmonGame.Objects;
using System.Linq;
using Microsoft.Xna.Framework;
using SalmonGame.Collisions;
using SalmonGame;
namespace GhostGame
{
    public abstract class Entity : GameObject
    {
        public World world;
        public float Health;
        public bool invincible;
        public Color bleedColor;
        public bool Alive => Game1.instance.objectManager.objects.Contains(this);
        public virtual void Damage(float amount, Entity attacker)
        {
            if (!invincible)
            {
                Health -= amount;
                AABB overlap = aabb.Overlap(attacker.aabb);
                Random r = new Random();
                for (int i = 0; i < 20; i++)
                {
                    if(overlap.size != Vector2.Zero)
                    {
                        Particle p = new Particle(overlap.position + new Vector2(1-(float)r.NextDouble()*2, 1-(float)r.NextDouble()*2), Vector2.Normalize(attacker.position - position) * new Vector2((float)r.NextDouble(),1-(float) r.NextDouble()) * 2, bleedColor);
                        p.Add();
                    }
                }
            }            
        }
        public override void Add()
        {
            renderArgs.renderTarget = Game1.instance.renderer.gameplayTarget;
            renderArgs.effect = null;
            Game1.instance.renderer.entityGroup.TryAdd(this);
            base.Add();
            world = Game1.instance.world;
        }
        public virtual void HitWall() 
        {
            if (onGround || onCeiling)
                velocity.Y = 0;
            if (onLeftWall || onRightWall)
                velocity.X = 0;
        }
        public virtual void Kill()
        {
            Random r = new Random();
            for(int i = 0; i < 200; i++)
            {
                Vector2 pOffset = new Vector2(1-(float)r.NextDouble()*2, 1-(float)r.NextDouble()*2) * hitboxSize.Length()/2;
                Vector2 v = new Vector2(1-(float)r.NextDouble()*2, 1-(float)r.NextDouble()*2) * 2;
                Particle p = new Particle(position + pOffset, v, bleedColor);
                p.Add();
            }
            Destroy();
        }
        protected bool CorrectMapXCollision(TileMap map)
        {
            if (velocity.X == 0)
                return false;
            List<Tile> tiles = map.GetIntersectedTiles(aabb);            
            if (tiles.Count == 0)
                return false;            
            if(velocity.X > 0)
            {
                float minTile = tiles.Min(tile => tile.position.X);
                position.X = minTile - map.TileSize / 2 - hitboxSize.X / 1.999f;
                onRightWall = true;
            }
            else if (velocity.X < 0)
            {
                float maxTile = tiles.Max(tile => tile.position.X);
                position.X = maxTile + map.TileSize / 2 + hitboxSize.X / 1.999f;
                onLeftWall = true;
            }
            tempVelocity.X = 0;
            return true;
        }
        protected bool CorrectMapYCollision(TileMap map)
        {
            if (velocity.Y == 0)
                return false;
            List<Tile> tiles = map.GetIntersectedTiles(aabb);
            if (tiles.Count == 0)
                return false;            
            if (velocity.Y > 0)
            {
                float minTile = tiles.Min(tile => tile.position.Y);
                position.Y = minTile - map.TileSize / 2 - hitboxSize.Y / 1.999f;
                onGround = true;
            }
            else if (velocity.Y < 0)
            {
                float maxTile = tiles.Max(tile => tile.position.Y);
                position.Y = maxTile + map.TileSize / 2 + hitboxSize.Y / 1.999f;
                onCeiling = true;
            }
            tempVelocity.Y = 0;
            return true;
        }
        public override void Update(GameTime time)
        {
            if (doCollisions)
            {
                bool hitWall = false;
                tempVelocity = velocity;
                if (velocity.X != 0)
                {
                    onLeftWall = false;
                    onRightWall = false;
                }
                if (velocity.Y != 0)
                {
                    onGround = false;
                    onCeiling = false;
                }
                List<GameObject> objects = new List<GameObject>();
                collidingGroups.ForEach(groupID => objects.AddRange(manager.GetGroup(groupID)));
                position.X += velocity.X;
                hitWall |= CorrectMapXCollision(world.tileMap);
                foreach (GameObject obj in objects)
                {
                    if (obj == this) continue;
                    hitWall |= CorrectXCollision(obj);
                }
                position.Y += velocity.Y;
                hitWall |= CorrectMapYCollision(world.tileMap);
                foreach (GameObject obj in objects)
                {
                    if (obj == this) continue;
                    hitWall |= CorrectYCollision(obj);
                }
                if (hitWall) HitWall();                
            }
            else
            {
                position += velocity;
            }
            if (Health <= 0)
            {
                Kill();
            }
        }


    }
}

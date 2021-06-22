using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
namespace GhostGame
{
    public class EntitySpawner
    {
        Type EntityType;
        public Vector2 position;
        public bool LoadFailed;
        public string typeName;
        public Entity GetEntity()
        {
            if (LoadFailed)
            {
                return null;
            }            
            Entity e = (Entity) Activator.CreateInstance(EntityType);
            e.position = position;
            return e;
        }
        public void Spawn()
        {
            Entity e = GetEntity();
            if(e != null)
            {
                e.Add();
            }
        }
        public EntitySpawner(string name, Vector2 position)
        {
            this.position = position;
            typeName = name;
            try
            {
                EntityType = Type.GetType("GhostGame." + name);
            }
            catch
            {
                LoadFailed = true;
                return;
            }
            if (EntityType is null || !EntityType.IsSubclassOf(typeof(Entity)))
            {
                LoadFailed = true;
                return;
            }
            
            LoadFailed = false;
        }
    }
}

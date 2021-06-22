using System;
using System.Collections.Generic;
using System.Text;
using SalmonGame.Objects;
using Microsoft.Xna.Framework;
using System.IO;
namespace GhostGame
{
    public class World
    {
        public ObjectManager objectManager;
        public TileMap tileMap;
        public List<EntitySpawner> spawners = new List<EntitySpawner>();
        public static World instance;
        

        public World(Game1 game)
        {            
           
            objectManager = game.objectManager;            
            
        }
        public static World FromFile(Game1 game, string path)
        {
            World world = new World(game);
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);
            world.tileMap = TileMap.ReadTileMap(reader, 27);
            if(reader.BaseStream.Position != reader.BaseStream.Length && reader.ReadString() == "spawnerhead")
            {
                int spawnerCount = reader.ReadInt32();
                for(int i = 0; i <spawnerCount; i++)
                {
                    string name = reader.ReadString();
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    world.spawners.Add(new EntitySpawner(name, new Vector2(x, y)));
                }
            }
            reader.Close();
            fs.Close();
            return world;
        }
        public void Save(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(fs);
            tileMap.WriteTileMap(writer);
            writer.Write("spawnerhead");
            writer.Write(spawners.Count);
            foreach(EntitySpawner spawner in spawners)
            {
                writer.Write(spawner.typeName);
                writer.Write(spawner.position.X);
                writer.Write(spawner.position.Y);
            }
            writer.Close();
            fs.Close();
        }
        public void DestroyEntities()
        {
            List<Entity> toBeKilled = new List<Entity>();
            foreach(Entity e in Game1.instance.objectManager.objects)
            {
                toBeKilled.Add(e);
            }
            toBeKilled.ForEach(e => e.Destroy());
        }
        public void SpawnAll()
        {
            foreach (EntitySpawner spawner in spawners)
            {
                spawner.Spawn();
            }
        }       
    }
}

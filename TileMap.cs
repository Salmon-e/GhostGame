using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using SalmonGame.Collisions;
using MonoGame.Extended.Tiled;
using SalmonGame.Render;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.IO;
namespace GhostGame
{
    public class TileMap
    {        
        public Dictionary<Vector2, Tile> Tiles = new Dictionary<Vector2, Tile>();
        public float TileSize;
        public Vector2 tileOrigin;
        public Texture2D tileTexture;
        public Texture2D CreateTileTexture()
        {
            float maxX = Tiles.Values.Max(t => t.mapPosition.X);
            float maxY = Tiles.Values.Max(t => t.mapPosition.Y);
            float minX = Tiles.Values.Min(t => t.mapPosition.X);
            float minY = Tiles.Values.Min(t => t.mapPosition.Y);
            tileOrigin = new Vector2(minX, minY);
            Texture2D t = new Texture2D(Game1.instance.GraphicsDevice, (int)(maxX - minX), (int)(maxY - minY), false, SurfaceFormat.Alpha8);
            
            byte[] data = new byte[t.Width * t.Height];
            int i = 0;
            for (int y = 0; y < t.Height; y++)
                for (int x = 0; x < t.Width; x++, i++)
                {
                    data[i] = (byte)(Tiles.ContainsKey(new Vector2(x, y) + tileOrigin) ? 255 : 0);
                }
            t.SetData(data);
            
            return t;
        }
        public void AddTile(Tile tile)
        {           
            if(tile.name == Tile.Air)
            {
                RemoveTile(tile);
                return;
            }
            Tiles.TryAdd(tile.mapPosition, tile);
            tile.size = TileSize;
        }
        public void RemoveTile(Tile tile)
        {
            Tiles.Remove(tile.mapPosition);            
        }
        public Tile GetTile(Vector2 point)
        {
            Vector2 tileCoord = point / TileSize;
            tileCoord.Round();
            return Tiles.GetValueOrDefault(tileCoord, null);
        }
        public List<Tile> GetOnScreenTiles(int padding)
        {
            List<Tile> tiles = new List<Tile>();
            Rectangle screen = Game1.instance.Window.ClientBounds;
            Camera c = Game1.instance.renderer.camera;
            int tileWidth = screen.Width / (int) (TileSize * c.zoom) + padding;
            int tileHeight = screen.Height / (int)(TileSize * c.zoom) + padding;
            Vector2 center = Vector2.Round(c.position / TileSize);

            for(float x = center.X - tileWidth/2; x < center.X + tileWidth/2; x++)
                for (float y = center.Y - tileHeight / 2; y < center.Y + tileWidth / 2; y++)
                {
                    if (Tiles.ContainsKey(new Vector2(x, y)))
                        tiles.Add(Tiles[new Vector2(x, y)]);
                }
            return tiles;
        }
        public List<Tile> GetIntersectedTiles(AABB aabb)
        {
            List<Tile> intersectedTiles = new List<Tile>();

            Vector2 UL = aabb.UL / TileSize;
            Vector2 BR = aabb.BR / TileSize;

            UL.Round();
            BR.Round();

            for(float x = UL.X; x <= BR.X; x++)
                for(float y = UL.Y; y <= BR.Y; y++)
                {
                    if(Tiles.ContainsKey(new Vector2(x, y)))
                    {
                        intersectedTiles.Add(Tiles[new Vector2(x, y)]);
                    }
                }
            return intersectedTiles;
        }
        public static TileMap FromTmxMap(TiledMap map, float tileSize)
        {
            TiledMapTileLayer tileLayer = map.GetLayer<TiledMapTileLayer>("tiles");
            TileMap tileMap = new TileMap();
            tileMap.TileSize = tileSize;
            foreach(TiledMapTile tile in tileLayer.Tiles)
            {
                if (!tile.IsBlank)
                {
                    tileMap.AddTile(new Tile(Tile.Stone, new Vector2(tile.X, tile.Y)));
                }
            }
            return tileMap;
        }
        public static TileMap ReadTileMap(BinaryReader reader, float tileSize)
        {             
            string header = reader.ReadString();
            if(header != "tilemaphead")
            {
                throw new FileLoadException("Invalid file header");
            }
            TileMap map = new TileMap();
            map.TileSize = tileSize;
            int numTiles = reader.ReadInt32();
            for(int i = 0; i < numTiles; i++)
            {
                string name = reader.ReadString();
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                map.AddTile(new Tile(name, new Vector2(x, y)));
            }            
            return map;
        }
        public void WriteTileMap(BinaryWriter writer)
        {           
            writer.Write("tilemaphead");
            writer.Write(Tiles.Count);
            foreach(Tile tile in Tiles.Values)
            {
                writer.Write(tile.name);
                writer.Write(tile.mapPosition.X);
                writer.Write(tile.mapPosition.Y);
            }            
        }
    }
}

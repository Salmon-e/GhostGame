using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using ImGuiNET;
using System.Reflection;
namespace GhostGame
{
    
    public class Editor
    {
        public string tool;
        public string selectedTile;
        public string selectedEntity;
        public int paintRadius;
        public List<EditorAction> actionHistory = new List<EditorAction>();
        int historyIndex = -1;
        MouseState lastMouse = Mouse.GetState();
        public List<Entity> entities = new List<Entity>();
            
        public Vector2 mouseWorld => (Mouse.GetState().Position.ToVector2() - Game1.instance.Window.ClientBounds.Size.ToVector2() / 2) / Game1.instance.renderer.camera.zoom + Game1.instance.renderer.camera.position;
        World world;
        Game1 game;
        public Editor()
        {
            world = World.instance;
            game = Game1.instance;
        }
        public void Open()
        {
            world.DestroyEntities();
            world.SpawnAll();
            game.objectManager.FlushQueue();
            entities.Clear();
            entities.AddRange(game.objectManager.objects.ConvertAll(o => (Entity) o));
        }
        public void UpdateSpawners()
        {
            List<EntitySpawner> newSpawners = new List<EntitySpawner>();
            foreach (Entity e in entities)
            {
                newSpawners.Add(new EntitySpawner(e.GetType().Name, e.position));
            }
            if (newSpawners.Count != 0)
            {
                world.spawners.Clear();
                world.spawners.AddRange(newSpawners);
            }
        }
        public void Render()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Tiles"))
                {
                    if (ImGui.MenuItem("air"))
                        selectedTile = "air";
                    foreach (string key in Content.Tiles.Keys)
                    {
                        if (ImGui.MenuItem(key))
                            selectedTile = key;
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Entities"))
                {
                    Type[] types = Assembly.GetAssembly(typeof(Entity)).GetTypes().Where(t => !t.IsAbstract && t.IsClass && t.IsSubclassOf(typeof(Entity))).ToArray();
                    foreach(Type type in types)
                    {
                        if (ImGui.MenuItem(type.Name))
                        {
                            selectedEntity = type.FullName;
                        }
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Undo", "Ctrl+Z")) Undo();
                    if (ImGui.MenuItem("Redo", "Ctrl+Y")) Redo();
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }
            ImGui.Begin("Editor tools");
            if (ImGui.Button("Tiles"))
            {
                tool = "tiles";
            }
            if (ImGui.Button("Entities"))
            {
                tool = "entities";
            }
            
            ImGui.SliderInt("Paint radius", ref paintRadius, 0, 10);
        }
        bool triedUndo = false;
        bool triedRedo = false;
        public void Update(GameTime gameTime, Game1 game)
        {
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.W)) game.renderer.camera.position.Y -= 10;
            if (state.IsKeyDown(Keys.A)) game.renderer.camera.position.X -= 10;
            if (state.IsKeyDown(Keys.D)) game.renderer.camera.position.X += 10;
            if (state.IsKeyDown(Keys.S)) game.renderer.camera.position.Y += 10;
            if(state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.Z))
            {
                if(!triedUndo)
                    Undo();
                triedUndo = true;
            }
            else
            {
                triedUndo = false;
            }
            if (state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.Y))
            {
                if (!triedRedo)
                    Redo();
                triedRedo = true;
            }
            else
            {
                triedRedo = false;
            }
            if (!ImGui.GetIO().WantCaptureMouse)
            {
                if(Mouse.GetState().LeftButton == ButtonState.Pressed && selectedTile != null && tool == "tiles")
                {
                    ApplyAction(PaintTile());
                }
                if (Mouse.GetState().LeftButton == ButtonState.Pressed && lastMouse.LeftButton == ButtonState.Released && selectedEntity != null && tool == "entities")
                {
                    Entity e = (Entity) Activator.CreateInstance(Type.GetType(selectedEntity));
                    e.position = mouseWorld;
                    Entity hoveredEntity = entities.Find(e => e.aabb.Contains(mouseWorld));
                    if(hoveredEntity != null)
                        ApplyAction(new EditorEntityAction(this, hoveredEntity, null));
                    else
                        ApplyAction(new EditorEntityAction(this, null, e));

                }
                lastMouse = Mouse.GetState();
            }
        }
        EditorTileAction PaintTile()
        {
            List<Tile> before = new List<Tile>();
            List<Tile> after = new List<Tile>();
            TileMap map = Game1.instance.world.tileMap;
            Vector2 tilePos = mouseWorld / Game1.instance.world.tileMap.TileSize;
            tilePos.Round();
            for (int i = -paintRadius; i <= paintRadius; i++)
                for (int j = -paintRadius; j <= paintRadius; j++)
                {
                    Vector2 offset = new Vector2(i, j);
                    if (map.Tiles.ContainsKey(tilePos + offset))
                        before.Add(map.Tiles[tilePos + offset]);                    
                    after.Add(new Tile(selectedTile, tilePos + offset));                    
                }
            return new EditorTileAction(before, after);
        }
        
        public void ApplyAction(EditorAction action)
        {
            if (action.Redundant) return;
            action.Apply();
            historyIndex++;
            actionHistory.Insert(historyIndex, action);
        }
        public void Undo()
        {
            if (historyIndex == -1) return;
            EditorAction action = actionHistory[historyIndex];
            action.Undo();
            historyIndex--;
        }
        public void Redo()
        {
            historyIndex++;
            EditorAction action = actionHistory[historyIndex];
            action.Apply();
        }
        public abstract class EditorAction
        {
            public abstract void Apply();
            public abstract void Undo();
            public virtual bool Redundant { get;}
        }
        public class EditorEntityAction : EditorAction
        {
            Entity remove;
            Entity add;
            Editor editor;

            public EditorEntityAction(Editor editor, Entity remove, Entity add)
            {
                this.remove = remove;
                this.add = add;
                this.editor = editor;
            }
            public override void Apply()
            {
                if (remove != null)
                {
                    editor.entities.Remove(remove);
                    remove.Destroy();
                }
                if (add != null)
                {
                    editor.entities.Add(add);
                    add.Add();
                }
            }
            public override void Undo()
            {
                if (add != null)
                {
                    editor.entities.Remove(add);
                    add.Destroy();
                }
                if (add != null)
                {
                    editor.entities.Add(remove);
                    remove.Add();
                }
            }
        }
        public class EditorTileAction : EditorAction
        {
            public List<Tile> before;
            public List<Tile> after;
            public override bool Redundant
            {
                get
                {
                    bool redundant = true;
                    
                    foreach(Tile tile in after)
                    {
                        Tile beforeTile = before.Find(t => t.mapPosition == tile.mapPosition);

                        if (beforeTile != null)
                        {                            
                            redundant &= beforeTile.name == tile.name;
                        }
                    }
                    return redundant;
                }
            }
            public EditorTileAction(List<Tile> before, List<Tile> after)
            {
                
                foreach(Tile tile in before)
                {
                    if(after.Find(t => t.mapPosition == tile.mapPosition) == null)
                    {
                        after.Add(new Tile(Tile.Air, tile.mapPosition));
                    }
                }
                foreach (Tile tile in after)
                {
                    if (before.Find(t => t.mapPosition == tile.mapPosition) == null)
                    {
                        before.Add(new Tile(Tile.Air, tile.mapPosition));
                    }
                }
                this.before = before;
                this.after = after;
            }
            public override void Apply()
            {
                foreach(Tile tile in before)
                {
                    Game1.instance.world.tileMap.RemoveTile(tile);
                }
                foreach(Tile tile in after)
                {
                    Game1.instance.world.tileMap.AddTile(tile);
                }
            }
            public override void Undo()
            {
                foreach (Tile tile in after)
                {
                    Game1.instance.world.tileMap.RemoveTile(tile);
                }
                foreach (Tile tile in before)
                {
                    Game1.instance.world.tileMap.AddTile(tile);
                }
            }
        }
    }
}

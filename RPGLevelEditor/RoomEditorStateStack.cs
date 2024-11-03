using System.Reflection;
using RPGGame.GameObject.Entity;

namespace RPGLevelEditor
{
    public partial class RoomEditor
    {
        private abstract class StateStackFrame(RoomEditor editorWindow)
        {
            public RoomEditor EditorWindow { get; } = editorWindow;

            public abstract void RestoreState(bool isUndo);
        }

        private class TileEditStackFrame(RoomEditor editorWindow, int x, int y, RPGGame.GameObject.Tile tile) : StateStackFrame(editorWindow)
        {
            public int X { get; } = x;
            public int Y { get; } = y;
            public RPGGame.GameObject.Tile Tile { get; } = tile;

            public override void RestoreState(bool isUndo)
            {
                Stack<StateStackFrame> stack = isUndo ? EditorWindow.redoStack : EditorWindow.undoStack;
                stack.Push(new TileEditStackFrame(EditorWindow, X, Y, EditorWindow.OpenRoom.TileMap[X, Y]));

                // If collision has changed, select collision tool mode, else select tile tool mode
                EditorWindow.toolPanel.SelectedIndex = EditorWindow.OpenRoom.TileMap[X, Y].IsCollision != Tile.IsCollision ? 1 : 0;

                EditorWindow.OpenRoom.TileMap[X, Y] = Tile;
                EditorWindow.UpdateTiling(X, Y);
                EditorWindow.UpdateTileTexture(X, Y);
            }
        }

        private class EntityMoveStackFrame(RoomEditor editorWindow, Entity entity, float x, float y) : StateStackFrame(editorWindow)
        {
            public Entity Entity { get; } = entity;
            public float X { get; } = x;
            public float Y { get; } = y;

            public override void RestoreState(bool isUndo)
            {
                Stack<StateStackFrame> stack = isUndo ? EditorWindow.redoStack : EditorWindow.undoStack;
                stack.Push(new EntityMoveStackFrame(EditorWindow, Entity, Entity.Position.X, Entity.Position.Y));

                EditorWindow.DrawEntity(Entity, true);
                _ = Entity.Move(new Microsoft.Xna.Framework.Vector2(X, Y), false);
                EditorWindow.SelectEntity(Entity);
            }
        }

        private class EntityResizeStackFrame(RoomEditor editorWindow, Entity entity, float x, float y, float w, float h) : StateStackFrame(editorWindow)
        {
            public Entity Entity { get; } = entity;
            public float X { get; } = x;
            public float Y { get; } = y;
            public float Width { get; } = w;
            public float Height { get; } = h;

            public override void RestoreState(bool isUndo)
            {
                Stack<StateStackFrame> stack = isUndo ? EditorWindow.redoStack : EditorWindow.undoStack;
                stack.Push(new EntityResizeStackFrame(EditorWindow, Entity, Entity.Position.X, Entity.Position.Y, Entity.Size.X, Entity.Size.Y));

                EditorWindow.DrawEntity(Entity, true);
                _ = Entity.Move(new Microsoft.Xna.Framework.Vector2(X, Y), false);
                _ = Entity.Resize(new Microsoft.Xna.Framework.Vector2(Width, Height), false);
                EditorWindow.SelectEntity(Entity);
            }
        }

        private class EntityCreateStackFrame(RoomEditor editorWindow, float x, float y) : StateStackFrame(editorWindow)
        {
            public float X { get; } = x;
            public float Y { get; } = y;

            public override void RestoreState(bool isUndo)
            {
                Stack<StateStackFrame> stack = isUndo ? EditorWindow.redoStack : EditorWindow.undoStack;
                stack.Push(this);

                if (isUndo)
                {
                    EditorWindow.SelectEntity(null);
                    EditorWindow.DrawEntity(EditorWindow.OpenRoom.Entities[^1], true);
                    EditorWindow.OpenRoom.Entities.RemoveAt(EditorWindow.OpenRoom.Entities.Count - 1);
                }
                else
                {
                    EditorWindow.CreateEntityAtPosition(X, Y, false);
                }
            }
        }

        private class EntityPropertyEditStackFrame(RoomEditor editorWindow, Entity editedEntity) : StateStackFrame(editorWindow)
        {
            public Entity EditedEntity { get; } = editedEntity;
            public Entity EntityClone { get; } = (Entity)editedEntity.Clone();

            public override void RestoreState(bool isUndo)
            {
                Stack<StateStackFrame> stack = isUndo ? EditorWindow.redoStack : EditorWindow.undoStack;
                stack.Push(new EntityPropertyEditStackFrame(EditorWindow, EditedEntity));

                EditorWindow.SelectEntity(EditedEntity);

                foreach ((PropertyInfo property, _) in GetEditableEntityProperties(EditedEntity))
                {
                    property.SetValue(EditedEntity, property.GetValue(EntityClone));
                }

                EditorWindow.UpdateSelectedEntity();
            }
        }

        private class EntityDeleteStackFrame(RoomEditor editorWindow, Entity deletedEntity) : StateStackFrame(editorWindow)
        {
            public Entity DeletedEntity { get; } = deletedEntity;

            public override void RestoreState(bool isUndo)
            {
                Stack<StateStackFrame> stack = isUndo ? EditorWindow.redoStack : EditorWindow.undoStack;
                stack.Push(this);

                if (isUndo)
                {
                    EditorWindow.OpenRoom.Entities.Add(DeletedEntity);
                    EditorWindow.SelectEntity(DeletedEntity);
                }
                else
                {
                    EditorWindow.SelectEntity(null);
                    EditorWindow.DrawEntity(DeletedEntity, true);
                    _ = EditorWindow.OpenRoom.Entities.Remove(DeletedEntity);
                }
            }
        }
    }
}

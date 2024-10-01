using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace KegBubble
{
    internal sealed class ModEntry : Mod
    {
        private readonly HashSet<StardewValley.Object> _kegList = new();
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
        }
        
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            foreach (var location in Game1.locations)
            {
                foreach (var item in location.Objects.Values.Where(item => item.QualifiedItemId == "(BC)12"))
                {
                    _kegList.Add(item);
                }
            }
        }

        private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
        {
            foreach (var item in e.Added)
            {
                if (item.Value.QualifiedItemId == "(BC)12")
                {
                    _kegList.Add(item.Value);
                }
            }

            foreach (var item in e.Removed)
            {
                if (item.Value.QualifiedItemId == "(BC)12")
                {
                    _kegList.Remove(item.Value);
                }
            }
            Monitor.Log(_kegList.Count.ToString(), LogLevel.Error);
        }

        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            foreach (var item in _kegList.Where(item => item.heldObject.Value == null))
            {
                e.SpriteBatch.Draw(Game1.emoteSpriteSheet,
                    new Vector2(item.TileLocation.X * Game1.tileSize - Game1.viewport.X,
                        (item.TileLocation.Y - 1.5f) * Game1.tileSize - Game1.viewport.Y),
                    new Rectangle(0, 64, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
            }
        }
    }
}
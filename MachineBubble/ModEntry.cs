using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MachineBubble
{
    internal sealed class ModEntry : Mod
    {
        private readonly Dictionary<string, HashSet<StardewValley.Object>> _watchLists = new();
        private readonly HashSet<string> _trackedTypes = new()
        {
            "(BC)12", "(BC)15"
        };
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
        } 
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            // init
            foreach (var type in _trackedTypes)
            {
                _watchLists.Add(type, new HashSet<StardewValley.Object>());
            }
            
            // search for machines
            foreach (var location in Game1.locations)
            {
                foreach (var item in location.Objects.Values.Where(item => _trackedTypes.Contains(item.QualifiedItemId)))
                {
                    _watchLists[item.QualifiedItemId].Add(item);
                }
            }
        }

        private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
        {
            foreach (var item in e.Added)
            {
                if (_trackedTypes.Contains(item.Value.QualifiedItemId))
                {
                    _watchLists[item.Value.QualifiedItemId].Add(item.Value);
                }
            }

            foreach (var item in e.Removed)
            {
                if (_trackedTypes.Contains(item.Value.QualifiedItemId))
                {
                    _watchLists[item.Value.QualifiedItemId].Remove(item.Value);
                }
            }
        }

        private int _renderedCount = 0;
        private const int Cycle = 100;
        private const float Factor = Cycle / (2  * MathF.PI);
        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            foreach (var item in _watchLists.SelectMany(currentWatchList => currentWatchList.Value.Where(item => item.heldObject.Value == null && item.Location.Equals(Game1.currentLocation))))
            {
                e.SpriteBatch.Draw(Game1.emoteSpriteSheet,
                    new Vector2(item.TileLocation.X * Game1.tileSize - Game1.viewport.X,
                        (item.TileLocation.Y - 1.5f) * Game1.tileSize - Game1.viewport.Y + 6.5f * (float)Math.Sin(_renderedCount / Factor)) ,
                    new Rectangle(0, 64, 16, 16), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
            }
            _renderedCount++;
            if (_renderedCount < Cycle) return;
            _renderedCount = 0;
        }
    }
}
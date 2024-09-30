using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace autoWatering
{
    internal sealed class ModEntry : Mod
    {
        private readonly Dictionary<Vector2, HoeDirt> _hoeDirtList = new();
        private readonly HashSet<Vector2> _sprinklerTiles = new();
        private readonly HashSet<IndoorPot> _potList = new();
        private readonly HashSet<GameLocation> _plantableList = new();
        private readonly HashSet<GameLocation> _unplantableIndoors = new();
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.World.TerrainFeatureListChanged += OnTerrainFeatureListChanged;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            CreateLists();
        }

        private void OnTerrainFeatureListChanged(object? sender, TerrainFeatureListChangedEventArgs e)
        {
            if (!_plantableList.Contains(e.Location))
            {
                return;
            }

            foreach (var item in e.Added)
            {
                if (item.Value is HoeDirt hoeDirt)
                {
                    _hoeDirtList.TryAdd(hoeDirt.Tile, hoeDirt);
                }
            }
            foreach (var item in e.Removed)
            {
                if (item.Value is HoeDirt hoeDirt)
                {
                    _hoeDirtList.Remove(hoeDirt.Tile);
                }
            }
        }

        private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
        {
            if (!(_plantableList.Contains(e.Location) || _unplantableIndoors.Contains(e.Location)))
            {
                return;
            }
            //add
            foreach (var item in e.Added)
            {
                if (item.Value.IsSprinkler())
                {
                    foreach (var tile in item.Value.GetSprinklerTiles().Where(tile => !_sprinklerTiles.Contains(tile)))
                    {
                        _sprinklerTiles.Add(tile);
                    }
                }
                if (item.Value is IndoorPot pot)
                {
                    _potList.Add(pot);
                }
            }
            //remove
            foreach (var item in e.Removed)
            {
                if (item.Value.IsSprinkler())
                {
                    foreach (var tile in item.Value.GetSprinklerTiles())
                    {
                        _sprinklerTiles.Remove(tile);
                    }
                }
                if (item.Value is IndoorPot pot)
                {
                    _potList.Remove(pot);
                }
            }
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            var count = 0;
            foreach (var item in _hoeDirtList.Values.TakeWhile(item => !Game1.isRaining && Game1.player._money >= 5))
            {
                if (_sprinklerTiles.Contains(item.Tile) || item.crop == null) continue;
                item.state.Value = HoeDirt.watered;
                Game1.player._money -= 5;
                count++;
            }
            foreach (var item in _potList.TakeWhile(item => Game1.player._money >= 5))
            {
                if (item.hoeDirt.Value.crop == null) continue;
                item.Water();
                Game1.player._money -= 5;
                count++;
            }
            Game1.addHUDMessage(new HUDMessage($"自动浇水服务今日花费： {count}"));
        }

        private void CreateLists()
        {
            // add available locations
            foreach (var location in Game1.locations)
            {
                if (location.IsFarm || location.IsGreenhouse)
                {
                    _plantableList.Add(location);
                }
                else if (!location.IsOutdoors)
                {
                    _unplantableIndoors.Add(location);
                }
            }
            // add hoedirts in farms and greenhouses
            foreach (var location in _plantableList)
            {
                foreach(var item in location.terrainFeatures.Values.OfType<HoeDirt>())
                {
                    _hoeDirtList.Add(item.Tile, item);
                }
                foreach (var item in location.objects.Values)
                {
                    if (item.IsSprinkler())
                    {
                        foreach(var tile in item.GetSprinklerTiles())
                        {
                            _sprinklerTiles.Add(tile);
                        }
                    }
                    else if (item is IndoorPot pot)
                    {
                        _potList.Add(pot);
                    }
                }
            }
            // add indoor pots
            foreach (var item in _unplantableIndoors.SelectMany(location => location.objects.Values))
            {
                if (item is IndoorPot pot)
                {
                    _potList.Add(pot);
                }
            }
        }
    }
}

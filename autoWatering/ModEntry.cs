using System.Transactions;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace autoWatering
{
    internal sealed class ModEntry : Mod
    {
        readonly Dictionary<Vector2, HoeDirt> HoeDirtList = new();
        readonly HashSet<Vector2> SprinklerTiles = new();
        readonly HashSet<IndoorPot> PotList = new();
        readonly HashSet<GameLocation> plantableList = new();
        readonly HashSet<GameLocation> unplantableIndoors = new();
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += OnSaveloaded;
            helper.Events.World.TerrainFeatureListChanged += OnTerrainFeatureListChanged;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnSaveloaded(object? sender, SaveLoadedEventArgs e)
        {
            CreateLists();
        }

        private void OnTerrainFeatureListChanged(object? sender, TerrainFeatureListChangedEventArgs e)
        {
            foreach (var item in e.Added)
            {
                if (item.Value is HoeDirt)
                {
                    
                    break;
                }
            }
        }

        private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
        {
            //UpdateWaterList();
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            int HoeDirtCount = 0;
            int PotCount = 0;
            foreach (var item in HoeDirtList.Values)
            {
                if (item.crop != null)
                {
                    item.state.Value = HoeDirt.watered;
                    HoeDirtCount++;
                }
            }
            foreach (var item in PotList)
            {
                if (item.hoeDirt.Value.crop != null)
                {
                    item.Water();
                    PotCount++;
                }
            }
            Monitor.Log("HoeDirt: " + HoeDirtCount + " " +"Pot: " + PotCount, LogLevel.Error);
        }

        private void CreateLists()
        {
            // add avalible locations
            foreach (var location in Game1.locations)
            {
                if (location.IsFarm || location.IsGreenhouse)
                {
                    plantableList.Add(location);
                }
                else if (!location.IsOutdoors)
                {
                    unplantableIndoors.Add(location);
                }
            }
            // add hoedirts in farms and greenhouses
            foreach (var location in plantableList)
            {
                foreach(var item in location.terrainFeatures.Values.OfType<HoeDirt>())
                {
                    HoeDirtList.Add(item.Tile, item);
                }
                foreach (var item in location.objects.Values)
                {
                    if (item.IsSprinkler())
                    {
                        foreach(var tile in item.GetSprinklerTiles())
                        {
                            HoeDirtList.Remove(tile);
                        }
                    }
                    else if (item is IndoorPot pot)
                    {
                        PotList.Add(pot);
                    }
                }
            }
            // add indoor pots
            foreach (var location in unplantableIndoors)
            {
                foreach (var item in location.objects.Values)
                {
                    if (item is IndoorPot pot)
                    {
                        PotList.Add(pot);
                    }
                }
            }
        }
    }
}

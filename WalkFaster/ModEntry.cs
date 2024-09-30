using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace WalkFaster
{
    internal sealed class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        private void OnSaveLoaded(object? sender, EventArgs e)
        {
            var harmony = new Harmony(ModManifest.UniqueID);
            
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getMovementSpeed)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.postfix))
            );
        }

        private class Patches
        {
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            public static void postfix(ref float __result)
            {
                __result *= 1.2f;
            }
        }
    }
}
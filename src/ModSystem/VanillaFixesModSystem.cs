using HarmonyLib;
using System;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace VanillaFixes {
    public class VanillaFixesModSystem : ModSystem {
        public const string modID = "vanillafixes";
        private Harmony harmony = new Harmony(modID);

        public override void StartClientSide(ICoreClientAPI api) {
            var Original_GetNameTagRenderer = typeof(EntityNameTagRendererRegistry).GetMethod(nameof(EntityNameTagRendererRegistryPatch.GetNameTagRenderer), BindingFlags.NonPublic | BindingFlags.Instance);
            var Patched_GetNameTagRenderer = typeof(EntityNameTagRendererRegistryPatch).GetMethod(nameof(EntityNameTagRendererRegistryPatch.GetNameTagRenderer));
            harmony.Patch(Original_GetNameTagRenderer, prefix: new HarmonyMethod(Patched_GetNameTagRenderer));
        }

        public override void Start(ICoreAPI api) {
            try {
                var Original_OnGroundIdle = typeof(BlockMeal).GetMethod(nameof(BlockMealPatch.OnGroundIdle));
                var Transpiler_OnGroundIdle = typeof(BlockMealPatch).GetMethod(nameof(BlockMealPatch.Transpiler));
                harmony.Patch(Original_OnGroundIdle, transpiler: new HarmonyMethod(Transpiler_OnGroundIdle));
            } catch (Exception e) {
                api.Logger.Error($"[{modID}] {e}");
            }
        }

        public override void Dispose() {
            harmony.UnpatchAll(harmony.Id);
        }
    }
}

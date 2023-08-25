using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace VanillaFixes {
    static class EntityNameTagRendererRegistryPatch {
        public static bool GetNameTagRenderer(Entity entity, ref NameTagRendererDelegate __result) {
            if ((entity as EntityPlayer)?.Player != null) return true;
            __result = EntityNameTagRendererRegistry.DefaultNameTagRenderer;
            return false;
        }
    }
}

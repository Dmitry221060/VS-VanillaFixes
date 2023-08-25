using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace VanillaFixes {
    public static class BlockMealPatch {
        // IL patch, we replace original method body with a call to our patch function
        private static List<CodeInstruction> patch = new List<CodeInstruction>() {
            new CodeInstruction(OpCodes.Ldarg_1), // Pass entityItem as a first argument
            new CodeInstruction(OpCodes.Ldarg_0), // Pass this(BlockMeal) as a second argument
            new CodeInstruction(OpCodes.Call, typeof(BlockMealPatch).GetMethod(nameof(OnGroundIdle))),
            new CodeInstruction(OpCodes.Ret)
        };
        // We use `base.OnGroundIdle(entityItem)` as an anchor.
        // We need it to remain in the original method since we can't call it with Harmony
        private static CodeInstruction anchor = new CodeInstruction(
            OpCodes.Call,
            typeof(CollectibleObject).GetMethod(nameof(OnGroundIdle))
        );

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var startIndex = -1;

            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++) {
                var codeInstr = codes[i];
                if (codeInstr.opcode != anchor.opcode) continue;
                if (codeInstr.operand != anchor.operand) continue;
                startIndex = i + 1;
                break;
            }

            if (startIndex == -1) throw new Exception("Couldn't find transpiler anchor for BlockMeal patch");
            int count = codes.Count - startIndex;
            codes.RemoveRange(startIndex, count);
            codes.AddRange(patch);
            return codes.AsEnumerable();
        }

        public static void OnGroundIdle(EntityItem entityItem, BlockMeal __instance) {
            IWorldAccessor world = entityItem.World;
            if (world.Side != EnumAppSide.Server) return;

            if (entityItem.Swimming && world.Rand.NextDouble() < 0.01) {
                ItemStack[] stacks = __instance.GetContents(world, entityItem.Itemstack);

                if (MealMeshCache.ContentsRotten(stacks)) {
                    for (int i = 0; i < stacks.Length; i++) {
                        if (stacks[i] != null && stacks[i].StackSize > 0 && stacks[i].Collectible.Code.Path == "rot") {
                            world.SpawnItemEntity(stacks[i], entityItem.ServerPos.XYZ);
                        }
                    }
                } else {
                    ItemStack rndStack = stacks[world.Rand.Next(stacks.Length)];
                    world.SpawnCubeParticles(entityItem.ServerPos.XYZ, rndStack, 0.3f, 25, 1, null);
                }

                if (__instance.Class == "BlockPie") return;

                Block block = world.GetBlock(new AssetLocation(__instance.Attributes["eatenBlock"].AsString()));
                entityItem.Itemstack = new ItemStack(block);
                entityItem.WatchedAttributes.MarkPathDirty("itemstack");
            }
        }
    }
}

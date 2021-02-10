using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Core.Loaders;
using StarlightRiver.NPCs.TownUpgrade;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

using StarlightRiver.Core;
using StarlightRiver.Content.GUI;

namespace StarlightRiver.Content.CustomHooks
{
    class NPCUpgrade : HookGroup
    {
        //ON justs sets some values on a UI when a vanilla event is triggered, IL swaps out some names. Questionable for IL, but might classift for fragile due to the sheer amount of IL patches in here.
        public override SafetyLevel Safety => SafetyLevel.Questionable;

        public override void Load()
        {
            On.Terraria.NPC.GetChat += SetUpgradeUI;

            //This set of IL hooks changes the title of the NPCs in chat messages and UI, since attempting to change the actual name of the NPCs makes vanilla shit itself.
            IL.Terraria.WorldGen.SpawnTownNPC += SwapTitle;
            IL.Terraria.NPC.checkDead += SwapTitleDeath;
            IL.Terraria.Main.DrawInventory += SwapTitleMenu;
        }

        public override void Unload()
        {
            IL.Terraria.WorldGen.SpawnTownNPC -= SwapTitle;
            IL.Terraria.NPC.checkDead -= SwapTitleDeath;
            IL.Terraria.Main.DrawInventory -= SwapTitleMenu;
        }

        private string SetUpgradeUI(On.Terraria.NPC.orig_GetChat orig, NPC self)
        {
            if (StarlightWorld.TownUpgrades.TryGetValue(self.TypeName, out bool unlocked))
            {
                if (unlocked)
                    UILoader.GetUIState<ChatboxOverUI>().SetState(TownUpgrade.FromString(self.TypeName));
                else
                    UILoader.GetUIState<ChatboxOverUI>().SetState(new LockedUpgrade());
            }
            else
                UILoader.GetUIState<ChatboxOverUI>().SetState(null);

            return orig(self);
        }

        //custom name and icon for upgraded town NPCs
        private void SwapTitleMenu(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(i => i.MatchLdsfld<Main>("spriteBatch"), i => i.MatchLdsfld<Main>("npcHeadTexture"), i => i.MatchLdloc(78));
            c.Index += 4; //not the safest thing ever ech

            c.Emit(OpCodes.Ldsfld, typeof(Main).GetField("npc", BindingFlags.Static | BindingFlags.Public));
            c.Emit(OpCodes.Ldloc, 71);
            c.Emit(OpCodes.Ldelem_Ref);

            c.EmitDelegate<SwapTitleMenuDelegate>(EmitSwapTitleMenuDelegate);

            c.Emit(OpCodes.Ldloc, 66);
            c.Emit(OpCodes.Ldsfld, typeof(Main).GetField("npc", BindingFlags.Static | BindingFlags.Public));
            c.Emit(OpCodes.Ldloc, 71);
            c.Emit(OpCodes.Ldelem_Ref);

            c.Emit(OpCodes.Ldloc, 73); //X and Y coords to check mouse collision. Fuck you vanilla.
            c.Emit(OpCodes.Ldloc, 74);

            c.EmitDelegate<SwapTextMenuDelegate>(EmitSwapTextMenuDelegate);

            c.Emit(OpCodes.Stloc, 66);
        }

        private delegate string SwapTextMenuDelegate(string input, NPC npc, int x, int y);

        private string EmitSwapTextMenuDelegate(string input, NPC npc, int x, int y)
        {
            bool hovering = Main.mouseX >= x && Main.mouseX <= x + Main.inventoryBackTexture.Width * Main.inventoryScale && Main.mouseY >= y && Main.mouseY <= y + Main.inventoryBackTexture.Height * Main.inventoryScale;

            if (hovering && string.IsNullOrEmpty(input) && Main.mouseItem.type == ItemID.None && StarlightWorld.TownUpgrades.TryGetValue(npc.TypeName, out bool unlocked) && unlocked)
                return npc.GivenName + " the " + TownUpgrade.FromString(npc.TypeName)._title;
            return input;
        }

        private delegate Texture2D SwapTitleMenuDelegate(Texture2D input, NPC npc);

        private Texture2D EmitSwapTitleMenuDelegate(Texture2D input, NPC npc)
        {
            if (StarlightWorld.TownUpgrades.TryGetValue(npc.TypeName, out bool unlocked) && unlocked)
                return TownUpgrade.FromString(npc.TypeName).icon;
            return input;
        }

        //custom "departure" message for upgraded NPCs
        private void SwapTitleDeath(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(i => i.MatchStloc(3));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<SwapTitleDeathDelegate>(EmitSwapTitleDeathDelegate);
        }

        private delegate NetworkText SwapTitleDeathDelegate(NetworkText input, NPC npc);

        private NetworkText EmitSwapTitleDeathDelegate(NetworkText input, NPC npc)
        {
            if (StarlightWorld.TownUpgrades.TryGetValue(npc.TypeName, out bool unlocked) && unlocked)
                return NetworkText.FromLiteral(npc.GivenName + " the " + TownUpgrade.FromString(npc.TypeName)._title + " was slain...");
            return input;
        }

        //Custom arrival message for upgraded NPCs
        private void SwapTitle(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(i => i.MatchStloc(8));
            c.Index++;

            c.Emit(OpCodes.Ldsfld, typeof(Main).GetField("npc", BindingFlags.Static | BindingFlags.Public));
            c.Emit(OpCodes.Ldloc, 7);
            c.Emit(OpCodes.Ldelem_Ref);

            c.Emit(OpCodes.Ldloc, 8);

            c.EmitDelegate<SwapTitleDelegate>(EmitSwapTitleDelegate);
            c.Emit(OpCodes.Stloc, 8);
        }

        private delegate string SwapTitleDelegate(NPC npc, string input);

        private string EmitSwapTitleDelegate(NPC npc, string input)
        {
            if (StarlightWorld.TownUpgrades.TryGetValue(npc.TypeName, out bool unlocked) && unlocked) return npc.GivenName + " the " + TownUpgrade.FromString(npc.TypeName)._title;
            return input;
        }
    }
}
using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.NPCs.TownUpgrade;
using StarlightRiver.Core.Loaders.UILoading;
using System.Reflection;
using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Core.Systems.NPCUpgradeSystem
{
	class NPCUpgradeHook : HookGroup
	{
		//ON justs sets some values on a UI when a vanilla event is triggered, IL swaps out some names. Questionable for IL, but might classift for fragile due to the sheer amount of IL patches in here.
		public override void Load()
		{
			On.Terraria.NPC.GetChat += SetUpgradeUI;

			//This set of IL hooks changes the title of the NPCs in chat messages and UI, since attempting to change the actual name of the NPCs makes vanilla unhappy.
			IL.Terraria.WorldGen.SpawnTownNPC += SwapTitle;
			IL.Terraria.NPC.checkDead += SwapTitleDeath;
			IL.Terraria.Main.DrawNPCHousesInUI += SwapTitleMenu;
		}

		public override void Unload()
		{
			IL.Terraria.WorldGen.SpawnTownNPC -= SwapTitle;
			IL.Terraria.NPC.checkDead -= SwapTitleDeath;
			IL.Terraria.Main.DrawNPCHousesInUI -= SwapTitleMenu;
		}

		private string SetUpgradeUI(On.Terraria.NPC.orig_GetChat orig, NPC self)
		{
			if (NPCUpgradeSystem.townUpgrades.TryGetValue(self.TypeName, out bool unlocked))
			{
				if (unlocked)
					UILoader.GetUIState<ChatboxOverUI>().SetState(TownUpgrade.FromString(self.TypeName));
				else
					UILoader.GetUIState<ChatboxOverUI>().SetState(new LockedUpgrade());
			}
			else
			{
				UILoader.GetUIState<ChatboxOverUI>().SetState(null);
			}

			return orig(self);
		}

		//custom name and icon for upgraded town NPCs
		private void SwapTitleMenu(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(i => i.MatchLdsfld<Main>("spriteBatch"), i => i.MatchLdsfld(typeof(Terraria.GameContent.TextureAssets), "NpcHead"), i => i.MatchLdloc(17));
			c.Index += 5; //not the safest thing ever ech

			c.Emit(OpCodes.Ldsfld, typeof(Main).GetField("npc", BindingFlags.Static | BindingFlags.Public));
			c.Emit(OpCodes.Ldloc, 11);
			c.Emit(OpCodes.Ldelem_Ref);

			c.EmitDelegate<SwapTitleMenuDelegate>(EmitSwapTitleMenuDelegate);

			c.Emit(OpCodes.Ldloc, 1);
			c.Emit(OpCodes.Ldsfld, typeof(Main).GetField("npc", BindingFlags.Static | BindingFlags.Public));
			c.Emit(OpCodes.Ldloc, 11);
			c.Emit(OpCodes.Ldelem_Ref);

			c.Emit(OpCodes.Ldloc, 12); //X and Y coords to check mouse collision
			c.Emit(OpCodes.Ldloc, 13);

			c.EmitDelegate<SwapTextMenuDelegate>(EmitSwapTextMenuDelegate);

			c.Emit(OpCodes.Stloc, 1);
		}

		private delegate string SwapTextMenuDelegate(string input, NPC NPC, int x, int y);

		private string EmitSwapTextMenuDelegate(string input, NPC NPC, int x, int y)
		{
			Texture2D tex = Terraria.GameContent.TextureAssets.InventoryBack.Value;
			bool hovering = Main.mouseX >= x && Main.mouseX <= x + tex.Width * Main.inventoryScale && Main.mouseY >= y && Main.mouseY <= y + tex.Height * Main.inventoryScale;

			if (hovering && string.IsNullOrEmpty(input) && Main.mouseItem.type == ItemID.None && NPCUpgradeSystem.townUpgrades.TryGetValue(NPC.TypeName, out bool unlocked) && unlocked)
				return NPC.GivenName + " the " + TownUpgrade.FromString(NPC.TypeName).title;
			return input;
		}

		private delegate Texture2D SwapTitleMenuDelegate(Texture2D input, NPC NPC);

		private Texture2D EmitSwapTitleMenuDelegate(Texture2D input, NPC NPC)
		{
			if (NPCUpgradeSystem.townUpgrades.TryGetValue(NPC.TypeName, out bool unlocked) && unlocked)
				return TownUpgrade.FromString(NPC.TypeName).icon;

			return input;
		}

		//custom "departure" message for upgraded NPCs
		private void SwapTitleDeath(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(i => i.MatchStloc(6));
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<SwapTitleDeathDelegate>(EmitSwapTitleDeathDelegate);
		}

		private delegate NetworkText SwapTitleDeathDelegate(NetworkText input, NPC NPC);

		private NetworkText EmitSwapTitleDeathDelegate(NetworkText input, NPC NPC)
		{
			if (NPCUpgradeSystem.townUpgrades.TryGetValue(NPC.TypeName, out bool unlocked) && unlocked)
				return NetworkText.FromLiteral(NPC.GivenName + " the " + TownUpgrade.FromString(NPC.TypeName).title + " was slain...");

			return input;
		}

		//Custom arrival message for upgraded NPCs
		private void SwapTitle(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(i => i.MatchStloc(10));
			c.Index++;

			c.Emit(OpCodes.Ldsfld, typeof(Main).GetField("npc", BindingFlags.Static | BindingFlags.Public));
			c.Emit(OpCodes.Ldloc, 9);
			c.Emit(OpCodes.Ldelem_Ref);

			c.Emit(OpCodes.Ldloc, 10);

			c.EmitDelegate<SwapTitleDelegate>(EmitSwapTitleDelegate);
			c.Emit(OpCodes.Stloc, 10);
		}

		private delegate string SwapTitleDelegate(NPC NPC, string input);

		private string EmitSwapTitleDelegate(NPC NPC, string input)
		{
			if (NPCUpgradeSystem.townUpgrades.TryGetValue(NPC.TypeName, out bool unlocked) && unlocked)
				return NPC.GivenName + " the " + TownUpgrade.FromString(NPC.TypeName).title;

			return input;
		}
	}
}
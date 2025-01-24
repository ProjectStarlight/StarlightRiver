using ReLogic.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Bosses.BrainRedux;
using StarlightRiver.Content.Events;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Dungeon;
using StarlightRiver.Content.Items.Haunted;
using StarlightRiver.Content.Items.UndergroundTemple;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.PersistentData;
using StarlightRiver.Content.Tiles.Crimson;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.PersistentDataSystem;
using Steamworks;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using static System.Net.WebRequestMethods;

namespace StarlightRiver.Content.Items
{
	[SLRDebug]
	class DebugStick : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Debug Stick");
			Tooltip.SetDefault("Has whatever effects are needed");
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Melee;
			Item.width = 38;
			Item.height = 40;
			Item.useTime = 18;

			Item.useAnimation = 18;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5f;
			Item.value = 1000;
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item18;
			Item.useTurn = true;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetHandler().StaminaMaxBonus = 1000;

			int x = StarlightWorld.vitricBiome.X - 37;

			Dust.NewDustPerfect(new Vector2((x + 80) * 16, (StarlightWorld.vitricBiome.Center.Y + 20) * 16), DustID.Firefly);

		}

		public override bool? UseItem(Player player)
		{
			StarlightWorld.FlipFlag(WorldFlags.ThinkerBossOpen);

			return true;
		}
	}

	class DebugModerEnabler : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Debug Mode");
			Tooltip.SetDefault("{{Inoculation}} is a keyword\n{{Barrier}} is too\nHere is a really long line so that we can test the wrapping logic of the tooltip panels! I hope this is long enough.\n{{BUFF:OnFire}} {{BUFF:PlexusChaliceBuff}} {{BUFF:BarbedKnifeBleed}}");
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.width = 38;
			Item.height = 38;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.useStyle = ItemUseStyleID.Swing;
		}

		public override bool? UseItem(Player Player)
		{
			StarlightRiver.debugMode = !StarlightRiver.debugMode;
			return true;
		}
	}

	class BrainSpawner : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public static bool betaActive = false;

		public override void Load()
		{
			On_Main.DoDraw += DrawBeta;
		}

		private void DrawBeta(On_Main.orig_DoDraw orig, Main self, GameTime gameTime)
		{
			orig(self, gameTime);

			Main.spriteBatch.Begin();

			DynamicSpriteFont font = Terraria.GameContent.FontAssets.ItemStack.Value;

			Utils.DrawBorderStringBig(Main.spriteBatch, $"STARLIGHT RIVER ALPHA TEST -- THINKER BOSS FIGHT TEST 4", new Vector2(Main.screenWidth / 2, 16), Color.White, 0.6f, 0.5f);
			Utils.DrawBorderStringBig(Main.spriteBatch, $"ALPHA BUILD DOES NOT REPRESENT FINAL PRODUCT", new Vector2(Main.screenWidth / 2, 48), Color.White, 0.6f, 0.5f);
			Utils.DrawBorderStringBig(Main.spriteBatch, $"YOUR ID: {SteamFriends.GetPersonaName()} ({SteamUser.GetSteamID().m_SteamID})", new Vector2(Main.screenWidth / 2, 80), Color.Gray, 0.6f, 0.5f);

			Main.spriteBatch.End();
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Thinker Test");
			Tooltip.SetDefault("Teleports you and spawns the thinker");
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Melee;
			Item.width = 38;
			Item.height = 40;
			Item.useTime = 18;

			Item.useAnimation = 18;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5f;
			Item.value = 1000;
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item18;
			Item.useTurn = true;
		}

		public override bool? UseItem(Player player)
		{
			betaActive = true;

			foreach (NPC npc in Main.npc)
			{
				npc.active = false;
			}

			player.Center = new Vector2(Main.maxTilesX * 8, Main.maxTilesY * 8);

			NPC.NewNPC(null, (int)player.Center.X, (int)player.Center.Y - 200, ModContent.NPCType<TheThinker>());
			NPC.NewNPC(null, (int)player.Center.X, (int)player.Center.Y - 200, ModContent.NPCType<DeadBrain>());

			return true;
		}
	}
}
using ReLogic.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Abilities.Hint;
using StarlightRiver.Content.Bosses.TheThinkerBoss;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Events;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Dungeon;
using StarlightRiver.Content.Items.Haunted;
using StarlightRiver.Content.Items.UndergroundTemple;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Noise;
using StarlightRiver.Content.NPCs.Starlight;
using StarlightRiver.Content.PersistentData;
using StarlightRiver.Content.Tiles.Crimson;
using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.PersistentDataSystem;
using Steamworks;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.WorldBuilding;
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
			player.GetHandler().StaminaMaxBonus = 10;

			int x = StarlightWorld.vitricBiome.X - 37;

			Dust.NewDustPerfect(new Vector2((x + 80) * 16, (StarlightWorld.vitricBiome.Center.Y + 20) * 16), DustID.Firefly);

		}

		public override bool? UseItem(Player player)
		{
			//NPC.NewNPC(null, (int)player.Center.X, (int)player.Center.Y - 600, ModContent.NPCType<TheThinker>());
			//NPC.NewNPC(null, (int)player.Center.X, (int)player.Center.Y - 600, ModContent.NPCType<DeadBrain>());

			//StarlightWorld.FlipFlag(WorldFlags.ThinkerBossOpen);
			//GrayBlob((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);

			GUI.Stamina.gainAnimationTimer = 240;
			AlicanSafetySystem.DebugForceState(0);

			return true;
		}
	}

	[SLRDebug]
	class DebugStick2 : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Debug Stick 2");
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
			//player.GetHandler().StaminaMaxBonus = 1000;

			//int x = StarlightWorld.vitricBiome.X - 37;

			//Dust.NewDustPerfect(new Vector2((x + 80) * 16, (StarlightWorld.vitricBiome.Center.Y + 20) * 16), DustID.Firefly);

			Lighting.AddLight(Main.MouseWorld, new Vector3(0, 1, 0));

		}

		public override bool? UseItem(Player player)
		{
			//StarlightWorld.FlipFlag(WorldFlags.ThinkerBossOpen);
			//ModContent.GetInstance<StarlightWorld>().GraymatterGen(new GenerationProgress(), null);

			/*SplineGlow.Spawn(player.Center, Vector2.Lerp(player.Center, Main.MouseWorld, 0.5f) + Vector2.UnitX.RotatedByRandom(6.28f) * 50, Main.MouseWorld, 120, 1, Color.Teal);

			Tile tile2 = Framing.GetTileSafely((int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16));
			Main.NewText(tile2.TileType + " " + tile2.WallType);

			for (int x = -100; x < 100; x++)
			{
				for (int y = -100; y < 100; y++)
				{
					Tile tile = Framing.GetTileSafely(x + (int)(Main.MouseWorld.X / 16), y + (int)(Main.MouseWorld.Y / 16));
					//tile.IsActuated = false;

					if (tile.TileType == 922)
						tile.ClearEverything();
				}
			}*/

			//Point16 target = new Point16((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
			//Main.NewText($"Deviation at {target} along width 10: {WorldGenHelper.GetElevationDeviation(target, 10, 20, 10, true)}");

			//ModContent.GetInstance<StarlightWorld>().ObservatoryGen(null, null);

			Main.LocalPlayer.GetHandler().Lock<HintAbility>();

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

			Utils.DrawBorderStringBig(Main.spriteBatch, $"STARLIGHT RIVER ALPHA TEST -- THINKER BOSS FIGHT TEST", new Vector2(Main.screenWidth / 2, 16), Color.White, 0.4f, 0.5f);
			Utils.DrawBorderStringBig(Main.spriteBatch, $"ALPHA BUILD DOES NOT REPRESENT FINAL PRODUCT", new Vector2(Main.screenWidth / 2, 48), Color.White, 0.4f, 0.5f);
			Utils.DrawBorderStringBig(Main.spriteBatch, $"Things to test: boss phase 2 changes, all difficulties", new Vector2(Main.screenWidth / 2, 86), new Color(255, 255, 200), 0.3f, 0.5f);

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
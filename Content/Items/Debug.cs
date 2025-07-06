using ReLogic.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Bosses.TheThinkerBoss;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.NPCs.Starlight;
using StarlightRiver.Content.Tiles.BaseTypes;
using StarlightRiver.Content.Tiles.Starlight;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.ArmatureSystem;
using StarlightRiver.Core.Systems.CutsceneSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items
{
	[SLRDebug]
	class DebugStick : ModItem
	{
		Arm arm = new(Vector2.Zero, 5, 64, Assets.Invisible.Value);

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
			ObservatoryBiome.fade = 1f;
			return;

			player.GetHandler().StaminaMaxBonus = 10;

			int x = StarlightWorld.vitricBiome.X - 37;

			Dust.NewDustPerfect(new Vector2((x + 80) * 16, (StarlightWorld.vitricBiome.Center.Y + 20) * 16), DustID.Firefly);

		}

		public override bool? UseItem(Player player)
		{
			ProximityActivatedPylonSystem.activePylons.Clear();

			foreach (var item in DummySystem.dummies)
			{
				if (item is ProximityActivatedPylonDummy d)
				{
					d.activating = false;
					d.timer = 0;
				}
			}
			return true;



			//NPC.NewNPC(null, (int)player.Center.X, (int)player.Center.Y - 600, ModContent.NPCType<TheThinker>());
			//NPC.NewNPC(null, (int)player.Center.X, (int)player.Center.Y - 600, ModContent.NPCType<DeadBrain>());

			//StarlightWorld.FlipFlag(WorldFlags.ThinkerBossOpen);
			//GrayBlob((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);

			//GUI.Stamina.gainAnimationTimer = 240;
			//player.GetHandler().unlockedAbilities.Clear();
			//player.GetHandler().InfusionLimit++;
			//player.GetHandler().InfusionLimit %= 4;
			//AlicanSafetySystem.DebugForceState(2);

			//KeybindHelper.OpenKeybindsWithHelp();

			player.ConsumedLifeFruit = 0;

			for (int k = 0; k < NPCLoader.NPCCount; k++)
			{
				NPC npc = new();
				npc.SetDefaults(k);
				Main.BestiaryTracker.Kills.RegisterKill(npc);
				Main.BestiaryTracker.Chats.RegisterChatStartWith(npc);
				Main.BestiaryTracker.Sights.RegisterWasNearby(npc);
			}

			if (Main.LocalPlayer.controlDown)
			{
				ModContent.GetInstance<StarlightWorld>().GenerateGestaltStructure((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
			}
			else
			{
				for (int x = -100; x < 100; x++)
				{
					for (int y = -100; y < 100; y++)
					{
						Tile tile = Framing.GetTileSafely(x + (int)(Main.MouseWorld.X / 16), y + (int)(Main.MouseWorld.Y / 16));
						tile.WallType = WallID.None;
					}
				}
			}

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
			//return true;

			ObservatorySystem.pylonAppearsOn = false;
			ObservatorySystem.observatoryOpen = false;

			player.GetHandler().unlockedAbilities.Clear();
			player.GetHandler().Shards.Clear();
			player.GetHandler().InfusionLimit = 0;

			AlicanSafetySystem.DebugForceState(0);

			if (player.controlDown)
				player.ActivateCutscene<StarlightPylonActivateCutscene>();

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

			ModContent.GetInstance<StarlightWorld>().ObservatoryGen(null, null);

			//Main.LocalPlayer.GetHandler().Shards.Clear();
			//CagePuzzleSystem.solved = false;

			//WorldGen.KillTile((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);

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

	[SLRDebug]
	class SuperBreaker : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Super Breaker");
			Tooltip.SetDefault("Breaks anything!");
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
			Item.rare = ItemRarityID.Master;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item18;
			Item.useTurn = true;
		}

		public override bool? UseItem(Player player)
		{
			WorldGen.KillTile((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
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
			return;
			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

			DynamicSpriteFont font = Terraria.GameContent.FontAssets.ItemStack.Value;

			Utils.DrawBorderStringBig(Main.spriteBatch, $"STARLIGHT RIVER 0.3 ALPHA", new Vector2(Main.screenWidth / 2, 16), Color.White, 0.4f, 0.5f);
			Utils.DrawBorderStringBig(Main.spriteBatch, $"ALPHA BUILD DOES NOT REPRESENT FINAL PRODUCT", new Vector2(Main.screenWidth / 2, 48), Color.White, 0.4f, 0.5f);
			Utils.DrawBorderStringBig(Main.spriteBatch, $"Press T for target views", new Vector2(Main.screenWidth / 2, 86), Color.White, 0.4f, 0.5f);
			//Utils.DrawBorderStringBig(Main.spriteBatch, $"Things to test: Gravitation potion/globe please", new Vector2(Main.screenWidth / 2, 112), new Color(255, 255, 200), 0.3f, 0.5f);

			/*Terraria.UI.Chat.ChatManager.DrawColorCodedString(Main.spriteBatch, font, ChatManager.ParseMessage("what? [c/ff22ff:This is a test of the\nnational emergency fuck system\nwoo] no way!", Color.White).ToArray(), new Vector2(Main.screenWidth / 2, 126), Color.White, 0, default, Vector2.One, out var hovered, -1);

			Terraria.UI.Chat.ChatManager.DrawColorCodedString(Main.spriteBatch, font, ChatManager.ParseMessage("what? [c/ff22ff:This is a test of the national emergency fuck system woo] no way!", Color.White).ToArray(), new Vector2(Main.screenWidth / 2, 226), Color.White, 0, default, Vector2.One, out var hovered2, -1);

			Terraria.UI.Chat.ChatManager.DrawColorCodedString(Main.spriteBatch, font, "what? [c/ff22ff:This is a test of the national emergency fuck system woo] no way!", new Vector2(Main.screenWidth / 2, 326), Color.White, 0, default, Vector2.One);

			Terraria.UI.Chat.ChatManager.DrawColorCodedString(Main.spriteBatch, font, ChatManager.ParseMessage("what? [c/ff22ff:This is a test of the national emergency fuck system woo] no way!\n\ngreen is green but [c/22ff22: green is greener!]", Color.White).ToArray(), new Vector2(Main.screenWidth / 2, 426), Color.White, 0, default, Vector2.One, out var hovered3, 200);
			*/

			for (int k = 0; k < Main.screenHeight / 32; k++)
			{
				string message = $"ALPHA BUILD DOES NOT REPRESENT FINAL PRODUCT - ";

				if (k % 2 == 0)
					message = "DO NOT STREAM OR RECORD - ";

				for (int x = 0; x < Main.screenWidth; x += (int)(font.MeasureString(message).X * 1.5f))
				{

					Main.spriteBatch.DrawString(font, message, new Vector2(x, k * 32), Color.White * 0.3f, 0, Vector2.Zero, 1.5f, 0, 0);
				}
			}

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.T))
			{
				SpriteBatch spriteBatch = Main.spriteBatch;
				var target1 = new Rectangle(100, 400, Main.screenWidth / 10, Main.screenHeight / 10);
				var target2 = new Rectangle(100, 400 + Main.screenHeight / 10 + 20, Main.screenWidth / 10, Main.screenHeight / 10);
				var target3 = new Rectangle(100, 400 + Main.screenHeight / 10 * 2 + 40, Main.screenWidth / 10, Main.screenHeight / 10);

				var target4 = new Rectangle(100 + Main.screenWidth / 10 + 20, 400, Main.screenWidth / 10, Main.screenHeight / 10);
				var target5 = new Rectangle(100 + Main.screenWidth / 10 + 20, 400 + Main.screenHeight / 10 + 20, Main.screenWidth / 10, Main.screenHeight / 10);
				var target6 = new Rectangle(100 + Main.screenWidth / 10 + 20, 400 + Main.screenHeight / 10 * 2 + 40, PlayerTarget.Target.Width / 2, PlayerTarget.Target.Height / 2);

				target1.Inflate(6, 6);
				UIHelper.DrawBox(spriteBatch, target1, Color.Gray);
				target1.Inflate(-6, -6);
				spriteBatch.Draw(Main.screenTarget, target1, Color.White);

				target2.Inflate(6, 6);
				UIHelper.DrawBox(spriteBatch, target2, Color.Gray);
				target2.Inflate(-6, -6);
				spriteBatch.Draw(Main.screenTargetSwap, target2, Color.White);

				target3.Inflate(6, 6);
				UIHelper.DrawBox(spriteBatch, target3, Color.Gray);
				target3.Inflate(-6, -6);
				spriteBatch.Draw(FinalCaptureSystem.finalScreen, target3, Color.White);

				target4.Inflate(6, 6);
				UIHelper.DrawBox(spriteBatch, target4, Color.Gray);
				target4.Inflate(-6, -6);
				spriteBatch.Draw(LightingBuffer.screenLightingTarget.RenderTarget, target4, Color.White);

				target5.Inflate(6, 6);
				UIHelper.DrawBox(spriteBatch, target5, Color.Gray);
				target5.Inflate(-6, -6);
				spriteBatch.Draw(LightingBuffer.tileLightingTarget.RenderTarget, target5, Color.White);

				target6.Inflate(6, 6);
				UIHelper.DrawBox(spriteBatch, target6, Color.Gray);
				target6.Inflate(-6, -6);
				spriteBatch.Draw(PlayerTarget.Target, target6, Color.White);
			}

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
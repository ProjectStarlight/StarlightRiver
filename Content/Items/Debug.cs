using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items
{
	class DebugStick : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Debug Stick");
			Tooltip.SetDefault("Developer item");
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

			//Item.createTile = ModContent.TileType<Tiles.CrashTech.CrashPod>();
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<ResourceReservationPlayer>().ReserveLife(200);

			Main.NewText(WorldGen.worldSurface);
			Main.NewText(WorldGen.worldSurfaceLow);
			Main.NewText(WorldGen.worldSurfaceHigh);
			Main.NewText(player.Center.Y / 16);

			Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<Dusts.AuroraWater>(), Vector2.Zero, 0, new Color(200, 220, 255) * 0.4f, 1);
		}

		public override bool? UseItem(Player player)
		{
			ScreenTargetHandler.ResizeScreens(new Vector2(Main.screenWidth, Main.screenHeight));
			return true;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			var target = new Rectangle(50, 160, Main.screenWidth / 10, Main.screenHeight / 10);
			var target2 = new Rectangle(50, 160 + Main.screenHeight / 10 * 1 + 20, Main.screenWidth / 10, Main.screenHeight / 10);
			var target3 = new Rectangle(50, 160 + Main.screenHeight / 10 * 2 + 40, Main.screenWidth / 10, Main.screenHeight / 10);

			var targetO = new Rectangle(48, 158, Main.screenWidth / 10 + 4, Main.screenHeight / 10 + 4);
			var targetO2 = new Rectangle(48, 158 + Main.screenHeight / 10 * 1 + 20, Main.screenWidth / 10 + 4, Main.screenHeight / 10 + 4);
			var targetO3 = new Rectangle(48, 158 + Main.screenHeight / 10 * 2 + 40, Main.screenWidth / 10 + 4, Main.screenHeight / 10 + 4);

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, targetO, Color.Black);
			spriteBatch.Draw(Main.screenTarget, target, Color.White);

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, targetO2, Color.Black);
			spriteBatch.Draw(LightingBuffer.screenLightingTarget.RenderTarget, target2, Color.White);

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, targetO3, Color.Black);
			spriteBatch.Draw(LightingBuffer.tileLightingTarget.RenderTarget, target3, Color.White);
		}

		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
		{
			BarrierNPC GNPC = target.GetGlobalNPC<BarrierNPC>();
			GNPC.maxBarrier = 100;
			GNPC.barrier = 100;
			GNPC.drawGlow = true;
		}
	}

	class DebugModerEnabler : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Debug Mode");
			Tooltip.SetDefault("Enables debug mode which does... stuff!\nHold Y to make bosses go at ludicrous speed.");
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

	class Eraser : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Eraser");
			Tooltip.SetDefault("Death");
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Melee;
			Item.width = 38;
			Item.height = 38;
			Item.useTime = 2;
			Item.useAnimation = 2;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5f;
			Item.value = 1000;
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item18;
			Item.useTurn = true;
			Item.accessory = true;
		}

		public override bool? UseItem(Player Player)
		{
			foreach (NPC NPC in Main.npc.Where(n => Vector2.Distance(n.Center, Main.MouseWorld) < 100))
				NPC.StrikeNPC(99999, 0, 0, false, false, false);

			return true;
		}
	}
}

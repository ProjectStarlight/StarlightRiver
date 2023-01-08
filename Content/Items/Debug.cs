using System.Linq;
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
			return true;
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

using StarlightRiver.Content.Tiles.Vitric;
using StarlightRiver.Helpers;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items
{
	class DebugStick : ModItem
	{
		float a;
		float b;
		float c;

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

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Vector2 pos = new(Main.screenWidth / 2, Main.screenHeight / 2);
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Permafrost/WaterStaff").Value;

			DrawHelper.DrawWithPerspective(spriteBatch, tex, pos, Color.White, 1, a, b, c, 0.1f);
		}

		public override void UpdateInventory(Player player)
		{
			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.I))
				a += 0.03f;

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.U))
				a -= 0.03f;

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.K))
				b += 0.03f;

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.J))
				b -= 0.03f;

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.M))
				c += 0.03f;

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.N))
				c -= 0.03f;
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
			int cX = (int)Main.MouseWorld.X / 16;
			int cY = (int)Main.MouseWorld.Y / 16;

			Helper.PlaceMultitile(new Point16(cX, cY - 3), ModContent.TileType<VitricOre>(), Main.rand.Next(3));

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

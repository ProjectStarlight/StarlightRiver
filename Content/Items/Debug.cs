using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Events;
using StarlightRiver.Core.Systems;
using Terraria.ID;

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

		}

		public override bool? UseItem(Player player)
		{
			for(int x = 0; x < 200; x++)
			{
				for (int y = 0; y < 200; y++)
				{
					ushort slabType = StarlightRiver.Instance.Find<ModTile>("AncientSandstone").Type;
					var tile = Framing.GetTileSafely((int)Main.MouseWorld.X / 16 + x, (int)Main.MouseWorld.Y / 16 + y);
					if (tile.TileType == slabType)
						tile.WallType = (ushort)ModContent.WallType<Tiles.Vitric.Temple.VitricTempleWall>();
				}
			}

			/*StarlightEventSequenceSystem.sequence = 0;
			//player.GetHandler().unlockedAbilities.Clear();
			player.GetHandler().InfusionLimit = 0;

			Main.time = 53999;
			Main.dayTime = true;
			StarlightEventSequenceSystem.willOccur = true;*/

			WorldGen.spawnMeteor = true;

			return true;
		}
	}

	class DebugModerEnabler : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Debug Mode");
			Tooltip.SetDefault("Enables debug mode");
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
}
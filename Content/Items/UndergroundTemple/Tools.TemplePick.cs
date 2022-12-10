using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class TemplePick : ModItem
	{
		private int charge;
		private bool spinning;
		private int direction;

		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Whirlwind Pickaxe");
			Tooltip.SetDefault("Hold right click to charge up a spinning pickaxe dash");
		}

		public override void SetDefaults()
		{
			Item.rare = ItemRarityID.Green;
			Item.pick = 45;
			Item.useTime = 16;
			Item.useAnimation = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.damage = 8;
			Item.autoReuse = true;
			Item.channel = true;
			Item.UseSound = SoundID.Item1;
			Item.DamageType = DamageClass.Melee;
		}

		public override bool AltFunctionUse(Player Player)
		{
			return true;
		}

		public override bool CanUseItem(Player Player)
		{
			return !spinning && charge == 0;
		}

		public override bool? UseItem(Player Player)
		{
			if (Player.altFunctionUse == 2)
			{
				Item.noUseGraphic = true;
				Item.noMelee = true;
			}
			else
			{
				Item.noUseGraphic = false;
				Item.noMelee = false;
			}

			return true;
		}

		public override void UpdateInventory(Player Player) //strange hook to be doing this in but it seemeed the best solution at the time.
		{
			if (Player.HeldItem == Item) //bleghhh
			{
				if (Main.mouseRight && charge < 120) //this is gonna go to shiiittt in MPPPPP
				{
					var d = Dust.NewDustPerfect(Player.Center, DustType<Dusts.PickCharge>(), Vector2.UnitY.RotatedBy(charge / 120f * 6.28f) * 30, 0, Color.LightYellow, 2);
					d.customData = Player.whoAmI;

					if (charge == 119)
					{
						Terraria.Audio.SoundEngine.PlaySound(SoundID.MaxMana, Player.Center);

						for (int k = 0; k < 100; k++)
							Dust.NewDustPerfect(Player.Center, DustType<Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10));
					}
				}

				if (!Main.mouseRight && !spinning && charge == 120)
				{
					spinning = true;
					direction = Main.MouseWorld.X > Player.Center.X ? 1 : -1;
				}

				if (spinning)
				{
					charge--;
					Player.velocity.X = direction * 8;
					Player.direction = charge / 3 % 2 == 0 ? 1 : -1;

					if (charge % 3 == 0)
					{
						for (int k = 0; k < 3; k++)
						{
							int i = (int)(Player.Center.X / 16) + direction;
							int j = (int)(Player.position.Y / 16) + k;
							Player.PickTile(i, j, Item.pick);
						}
					}

					if (charge % 10 == 0)
						Terraria.Audio.SoundEngine.PlaySound(SoundID.Item63, Player.Center);

					if (charge <= 0)
						spinning = false;
				}
			}

			if (!Main.mouseRight && charge > 0 && !spinning || Player.HeldItem != Item)
				charge = 0;

			if (Main.mouseRight && Player.HeldItem == Item && charge < 120)
				charge++;
		}
	}
}

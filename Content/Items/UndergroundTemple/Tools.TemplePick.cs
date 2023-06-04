using System.Linq;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class TemplePick : ModItem
	{
		private int charge;

		private int direction;

		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Whirlwind Pickaxe");
			Tooltip.SetDefault("Hold right click to charge up a spinning pickaxe dash, breaking anything in your way");
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
			return !Spinning(Player) && charge == 0;
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

		private bool Spinning(Player player)
		{
			return Main.projectile.Any(n => n.active && n.type == ProjectileType<TemplePickProjectile>() && n.owner == player.whoAmI);
		}

		public override void UpdateInventory(Player Player)
		{
			if (Player.HeldItem == Item)
			{
				if (Main.mouseRight && charge < 120)
				{
					var d = Dust.NewDustPerfect(Player.Center, DustType<Dusts.PickCharge>(), Vector2.UnitY.RotatedBy(charge / 120f * 6.28f) * 30, 0, Color.LightYellow, 2);
					d.customData = Player.whoAmI;

					if (charge == 119)
					{
						Terraria.Audio.SoundEngine.PlaySound(SoundID.MaxMana, Player.Center);

						for (int k = 0; k < 100; k++)
						{
							Dust.NewDustPerfect(Player.Center, DustType<Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10));
						}
					}
				}

				if (!Main.mouseRight && !Spinning(Player) && charge == 120)
				{
					direction = Main.MouseWorld.X > Player.Center.X ? 1 : -1;
					Projectile.NewProjectile(Item.GetSource_FromThis(), Player.Center, Vector2.Zero, ProjectileType<TemplePickProjectile>(), 0, 0, Player.whoAmI, 60, direction);

					charge = 0;
				}
			}

			if (!Main.mouseRight && charge > 0 && !Spinning(Player) || Player.HeldItem != Item)
				charge = 0;

			if (Main.mouseRight && Player.HeldItem == Item && charge < 120)
				charge++;
		}
	}

	class TemplePickProjectile : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public ref float Timer => ref Projectile.ai[0];
		public ref float Direction => ref Projectile.ai[1];

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 999;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			Timer--;

			if (Timer <= 0)
				Projectile.timeLeft = 0;

			Player Player = Main.player[Projectile.owner];

			Projectile.Center = Player.Center;

			Player.velocity.X = Direction * 8;
			Player.direction = Timer / 3 % 2 == 0 ? 1 : -1;

			if (Timer % 4 == 0)
			{
				for (int k = 0; k < 3; k++)
				{
					int i = (int)(Player.Center.X / 16 + Direction);
					int j = (int)(Player.position.Y / 16) + k;
					Player.PickTile(i, j, 45);
				}
			}

			if (Timer % 10 == 0)
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item63, Player.Center);
		}
	}
}
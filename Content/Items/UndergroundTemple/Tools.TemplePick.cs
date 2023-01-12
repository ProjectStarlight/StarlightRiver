using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Vitric.IgnitionGauntlets;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using static Humanizer.In;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class TemplePick : ModItem
	{
		public int charge;
		private bool spinning;
		private int direction;

		private int dustCounter = 0;

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
			dustCounter++;
			if (Player.HeldItem == Item) //bleghhh
			{
				if (Main.mouseRight && !spinning) //this is gonna go to shiiittt in MPPPPP
				{
					if (dustCounter % 5 == 0)
					{
						var dust = Dust.NewDustPerfect(Player.Center, ModContent.DustType<TemplePickDust1>(), default, default, Color.Gold);
						dust.customData = Player.whoAmI;
						dust.scale = Main.rand.NextFloat(0.55f, 0.75f) * (charge / 120f);
						dust.alpha = Main.rand.Next(100);
						if (charge > 60)
							dust.alpha += 100;
						if (charge == 120)
							dust.alpha += 100;
					}

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

	class TemplePickDust1 : IgnitionChargeDustPassive
	{
		public override bool Update(Dust dust)
		{
			dust.fadeIn += 0.5f;

			dust.scale *= 0.99f;
			Player owner = Main.player[(int)dust.customData];

			dust.shader.UseColor(Color.Gold);
			dust.position = owner.Center + new Vector2(0, 15 + dust.alpha % 100 * 0.1f - (float)Math.Pow(dust.fadeIn / 3, 1.75f)) + new Vector2((15 + 3 * (dust.alpha / 100)) * (float)Math.Sin((dust.fadeIn + dust.alpha) * 0.1f), 0) - dust.scale * new Vector2(32, 32);

			if (dust.fadeIn >= 15 && dust.alpha < 100)
				dust.active = false;
			else if (dust.fadeIn >= 18 && dust.alpha < 200)
				dust.active = false;
			else if (dust.fadeIn >= 22)
				dust.active = false;

			return false;
		}
	}
}

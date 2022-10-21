using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.SteampunkSet;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
	[AutoloadEquip(EquipType.Shoes)]
	public class PulseBoots : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;
		public PulseBoots() : base("Pulse Boots", "Grants a directionally boosted double jump") { }

		private bool doubleJumped = false;
		private bool releaseJump = false;
		private const int maxSpeed = 15;

		public override void SafeUpdateEquip(Player Player)
		{
			void jumpSide(int side)
			{
				float velSide = Player.velocity.X * side;
				if (velSide > 0 && velSide < maxSpeed)
					Player.velocity.X += (maxSpeed * side - Player.velocity.X) / 2;
				else if (velSide < 0)
					Player.velocity.X = 6 * side;

				for (int y = 0; y < 10; y++)//placeholder dash dust
					Dust.NewDust(Player.BottomLeft + Player.velocity, Player.width, (int)Player.velocity.Y, 6, 3 * -side, 0, 0, default, 2);
			}

			if (!Player.controlJump && Player.velocity.Y != 0)
				releaseJump = true;
			if (Player.controlJump && Player.velocity.Y != 0 && releaseJump && !doubleJumped)
			{
				doubleJumped = true;
				Player.velocity.Y = -8; //base upward jump
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item61, Player.Center);

				for (float k = 0; k < 6.28f; k += 0.1f)
				{
					float rand = Main.rand.NextFloat(-0.05f, 0.05f);
					float x = (float)Math.Cos(k + rand) * 30;
					float y = (float)Math.Sin(k + rand) * 10;
					float rot = !Player.controlLeft ? Player.controlRight ? 1 : 0 : -1;

					Dust.NewDustPerfect(Player.Center + new Vector2(0, 16), DustType<Content.Dusts.Stamina>(), new Vector2(x, y).RotatedBy(rot) * 0.07f, 0, default, 1.6f);
					Dust.NewDustPerfect(Player.Center + new Vector2(0, 32), DustType<Content.Dusts.Stamina>(), new Vector2(x, y).RotatedBy(rot) * 0.09f, 0, default, 1.2f);
					Dust.NewDustPerfect(Player.Center + new Vector2(0, 48), DustType<Content.Dusts.Stamina>(), new Vector2(x, y).RotatedBy(rot) * 0.11f, 0, default, 0.8f);
				}

				Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot);

				if (Player.controlLeft && Player.controlRight || !Player.controlLeft && !Player.controlRight)
				{
					Player.velocity.Y += -2;//if neither or both, then slightly higher jump

					for (int y = 0; y < 8; y++)//placeholder dash dust
						Dust.NewDust(Player.BottomLeft + Player.velocity, Player.width, (int)Player.velocity.Y, 6, 0, 0, 0, default, 2);
				}
				else if (Player.controlLeft)//-1
				{
					jumpSide(-1);
				}
				else if (Player.controlRight)//1
				{
					jumpSide(1);
				}
			}

			if (Player.velocity.Y == 0)
			{
				releaseJump = false;
				doubleJumped = false;
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<AncientGear>(), 5);
			recipe.AddIngredient(ItemID.RocketBoots);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
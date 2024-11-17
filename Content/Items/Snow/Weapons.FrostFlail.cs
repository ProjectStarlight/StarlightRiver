using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Forest;
using StarlightRiver.Content.Tiles.Misc;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Snow
{
	internal class FrostFlail : AbstractHeavyFlail
	{
		public override string Texture => AssetDirectory.SnowItem + Name;

		public override int ProjType => ModContent.ProjectileType<FrostFlailProjectile>();

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Frost Flail");
			Tooltip.SetDefault("Hold to swing a monstrous ball of ice\nCreates icy shards on impact, inflicting frostburn");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.rare = ItemRarityID.Blue;
			Item.damage = 14;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.IceBlock, 99);
			recipe.AddIngredient(ModContent.ItemType<HeavyFlail>(), 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class FrostFlailProjectile : AbstractHeavyFlailProjectile
	{
		public override string Texture => AssetDirectory.SnowItem + Name;

		public override Asset<Texture2D> ChainAsset => Assets.Items.Snow.FrostFlailChain;

		public override int MaxLength => 160;

		public override void AI()
		{
			base.AI();

			for (int k = 0; k < 2; k++)
			{
				Dust.NewDust(Projectile.Center - Vector2.One * 20, 40, 40, DustID.IceTorch);
			}
		}

		public override void OnImpact(bool wasTile)
		{
			if (wasTile)
			{
				Helpers.Helper.PlayPitched("Magic/FrostHit", 1, 0, Projectile.Center);

				if (Owner == Main.LocalPlayer)
					CameraSystem.shake += 8;

				for (int k = 0; k < 30; k++)
				{
					Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice);
				}

				for (int k = 0; k < 10; k++)
				{
					Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SnowSpray, 0, -1);
				}

				for (int k = 0; k < 3; k++)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.One.RotatedByRandom(6.28f) * 3, ModContent.ProjectileType<FrostFlailShard>(), Projectile.damage / 2, 0, Projectile.owner);
				}

				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Vector2.UnitY * 8, Vector2.Zero, ModContent.ProjectileType<FrostFlailCrack>(), 0, 0);
			}
		}
	}

	internal class FrostFlailCrack : ModProjectile, IDrawOverTiles
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 200;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public void DrawOverTiles(SpriteBatch spriteBatch)
		{		
			float prog = Projectile.timeLeft > 100 ? 1f : Projectile.timeLeft / 100f;

			Color color = Color.Lerp(new Color(150, 240, 255), new Color(50, 80, 150), prog);
			color *= prog;
			color.A = 0;
			var tex = Assets.Misc.AlphaCrack.Value;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, 0, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * prog * 0.25f, 0, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
		}
	}

	internal class FrostFlailShard : ModProjectile
	{
		public override string Texture => AssetDirectory.SnowItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 120;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.penetrate = 15;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.4f;
			Projectile.rotation += Projectile.velocity.X * 0.1f;

			Projectile.velocity.X *= 0.99f;

			if (Projectile.timeLeft < 30)
				Projectile.alpha = (int)((1 - Projectile.timeLeft / 30f) * 255);

			if (Main.rand.NextBool(10))
				Dust.NewDust(Projectile.position, 18, 18, DustID.IceTorch);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.penetrate <= 0)
					return true;

			Projectile.velocity.Y += oldVelocity.Y * -0.8f;

			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Frostburn, 60);
		}
	}
}

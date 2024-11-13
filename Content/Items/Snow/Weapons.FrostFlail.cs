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
			DisplayName.SetDefault("Heavy Flail");
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

		public override void OnImpact(bool wasTile)
		{
			Helpers.Helper.PlayPitched("Magic/FrostHit", 1, 0, Projectile.Center);

			if (Owner == Main.LocalPlayer)
				CameraSystem.shake += 8;

			for (int k = 0; k < 50; k++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice);
			}

			for (int k = 0; k < 3; k++)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.One.RotatedByRandom(6.28f) * 5, ModContent.ProjectileType<FrostFlailShard>(), Projectile.damage / 2, 0, Projectile.owner);
			}

			Projectile.NewProjectile(null, Projectile.Center + Vector2.UnitY * 8, Vector2.Zero, ModContent.ProjectileType<HeavyFlailCrack>(), 0, 0);
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
			Projectile.penetrate = 3;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.penetrate -= 1;

			if (Projectile.penetrate <= 0)
					return true;

			Projectile.velocity += oldVelocity * -0.8f;

			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Frostburn, 60);
		}
	}
}

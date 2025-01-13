using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Forest
{
	internal class HeavyFlail : AbstractHeavyFlail
	{
		public override string Texture => AssetDirectory.ForestItem + Name;

		public override int ProjType => ModContent.ProjectileType<HeavyFlailProjectile>();

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Heavy Flail");
			Tooltip.SetDefault("Hold to swing a monstrous ball of metal\n`We've got the biggest balls of them all!`");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.rare = ItemRarityID.Blue;
			Item.damage = 16;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 20);
			recipe.AddIngredient(ItemID.Mace, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class HeavyFlailProjectile : AbstractHeavyFlailProjectile
	{
		public override string Texture => AssetDirectory.ForestItem + Name;

		public override Asset<Texture2D> ChainAsset => Assets.Items.Forest.HeavyFlailChain;

		public override void OnImpact(bool wasTile)
		{
			if (wasTile)
			{
				Helpers.Helper.PlayPitched("Impacts/StoneStrike", 1, 0, Projectile.Center);

				if (Owner == Main.LocalPlayer)
					CameraSystem.shake += 10;

				for (int k = 0; k < 32; k++)
				{
					Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone);
				}

				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Vector2.UnitY * 8, Vector2.Zero, ModContent.ProjectileType<HeavyFlailCrack>(), 0, 0);
			}
		}
	}

	internal class HeavyFlailCrack : ModProjectile, IDrawOverTiles
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
			Color color = Color.White;
			color *= Projectile.timeLeft > 100 ? 1f : Projectile.timeLeft / 100f;
			Texture2D tex = Assets.Misc.PixelCrack.Value;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, 0, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
		}
	}
}
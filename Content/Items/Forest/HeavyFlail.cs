using StarlightRiver.Content.Items.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public override bool? UseItem(Player player)
		{
			return true;
			for (int k = 100; k < 500; k += 100)
			{
				int i = Projectile.NewProjectile(null, player.Center, player.Center.DirectionTo(Main.MouseWorld) * k * 0.1f, ModContent.ProjectileType<HeavyFlailProjectile>(), 1, 1, player.whoAmI);
				(Main.projectile[i].ModProjectile as HeavyFlailProjectile).MaxLength = k;
			}

			return true;
		}
	}

	internal class HeavyFlailProjectile : AbstractHeavyFlailProjectile
	{
		public override string Texture => AssetDirectory.ForestItem + Name;

		public override Asset<Texture2D> ChainAsset => Assets.Items.Forest.HeavyFlailChain;

		public override void OnImpact(bool wasTile)
		{
			Main.NewText("Thunk!");
		}
	}
}

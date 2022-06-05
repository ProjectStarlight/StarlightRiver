using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	internal class AuroraWaterSplash : ModProjectile
	{
		public override string Texture => AssetDirectory.SquidBoss + Name;

		public override void SetDefaults()
		{
			Projectile.width = 72;
			Projectile.height = 106;
			Projectile.timeLeft = 40;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}
	}
}

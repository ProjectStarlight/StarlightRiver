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
			var spriteBatch = Main.spriteBatch;

			var tex = ModContent.Request<Texture2D>(Texture).Value;
			var frame = new Rectangle(0, (int)(6 - Projectile.timeLeft / 40f * 6) * 106, 72, 106);

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, 0, new Vector2(36, 53), 1, 0, 0);

			return false;
		}
	}
}

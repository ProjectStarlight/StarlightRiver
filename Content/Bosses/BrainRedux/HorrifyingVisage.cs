using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class HorrifyingVisage : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Horrifying Visage");
		}

		public override void SetDefaults()
		{
			Projectile.hostile = true;
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 300;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var tex = Terraria.GameContent.TextureAssets.Npc[NPCID.BrainofCthulhu].Value;

			var frame = new Rectangle(0, 182 * (int)(Projectile.timeLeft / 10f % 4), 200, 182);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White * 0.5f * (Projectile.timeLeft / 300f), 0, new Vector2(100, 91), 1f, 0, 0);

			return false;
		}
	}
}

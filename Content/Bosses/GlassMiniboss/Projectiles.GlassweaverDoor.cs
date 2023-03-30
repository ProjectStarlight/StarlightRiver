using StarlightRiver.Core.Systems.CameraSystem;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassweaverDoor : ModProjectile
	{
		public override string Texture => AssetDirectory.Glassweaver + Name;

		private float closeTimer;

		private bool opening = false;
		private bool closed = false;

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 160;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
		}

		public override bool CanHitPlayer(Player target)
		{
			return false;
		}

		public override void AI()
		{
			NPC parent = Main.npc.Where(n => n.active && n.type == ModContent.NPCType<Glassweaver>()).FirstOrDefault();

			if (parent != default && !opening)
			{
				if (closeTimer < 1)
				{
					closeTimer += 0.025f;
				}
				else if (!closed)
				{
					closed = true;
					CameraSystem.shake += 9;
					Helpers.Helper.PlayPitched("GlassMiniboss/GlassSmash", 1f, 0.3f, Projectile.Center);

					var dustPos = new Vector2(Projectile.Center.X, Projectile.Center.Y - Projectile.height);
					for (int i = 0; i < 15; i++)
						Dust.NewDustPerfect(dustPos + new Vector2(Main.rand.Next(-8, 8), 0), DustID.Copper, Main.rand.NextVector2Circular(3, 3), 0, default, Main.rand.NextFloat(0.85f, 1.15f));
				}
			}
			else
			{
				opening = true;
				closeTimer -= 0.025f;
			}

			Projectile.timeLeft = 2;

			if (closeTimer < 0)
				Projectile.active = false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			int height = (int)(tex.Height * closeTimer);

			var frame = new Rectangle(0, 0, tex.Width, height);
			var origin = new Vector2(tex.Width / 2, 0);

			Main.spriteBatch.Draw(tex, Projectile.Center - new Vector2(0, height - tex.Height / 2) - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
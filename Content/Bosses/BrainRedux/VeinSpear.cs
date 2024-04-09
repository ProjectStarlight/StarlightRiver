using System;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class VeinSpear : ModProjectile
	{
		public bool hit;

		public override string Texture => AssetDirectory.BrainRedux + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vein Spear");
		}

		public override void SetDefaults()
		{
			Projectile.hostile = true;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 300;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Projectile.timeLeft == 270)
				Projectile.velocity *= 50f;

			foreach (Player player in Main.player)
			{
				if (!player.active)
					continue;

				if (Helpers.Helper.CheckLinearCollision(BrainOfCthulu.TheBrain.npc.Center, Projectile.Center, player.Hitbox, out Vector2 collision))
				{
					player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " played the idiot harp..."), 25, 0);
				}
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.AddBuff(BuffID.Bleeding, 300);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (!hit)
			{
				hit = true;
				Projectile.velocity *= 0;
				Helpers.Helper.PlayPitched("Impacts/StabFleshy", 1f, -0.5f, Projectile.Center);
			}

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D chainTex = ModContent.Request<Texture2D>(AssetDirectory.BrainRedux + "VeinSpearChain").Value;
			Texture2D glow = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrailNoEnd").Value;

			float opacity = 1f;

			if (Projectile.timeLeft <= 30)
				opacity = Projectile.timeLeft / 30f;

			Color glowColor = new Color(180, 60, 90) * opacity * 0.25f;
			glowColor.A = 0;

			var gSource = new Rectangle(0, 0, glow.Width, glow.Height);
			var gTarget = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X), (int)(Projectile.Center.Y - Main.screenPosition.Y), (int)Vector2.Distance(BrainOfCthulu.TheBrain.npc.Center, Projectile.Center), 100);
			var gOrigin = new Vector2(0, glow.Height / 2f);

			spriteBatch.Draw(glow, gTarget, gSource, glowColor, (Projectile.Center - BrainOfCthulu.TheBrain.npc.Center).ToRotation() - 3.14f, gOrigin, 0, 0);

			for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(BrainOfCthulu.TheBrain.npc.Center, Projectile.Center) / 16))
			{
				Vector2 pos = Vector2.Lerp(Projectile.Center, BrainOfCthulu.TheBrain.npc.Center, k) - Main.screenPosition;

				spriteBatch.Draw(chainTex, pos, null, new Color(220, 60, 70) * opacity, (Projectile.Center - BrainOfCthulu.TheBrain.npc.Center).ToRotation() + 1.58f, chainTex.Size() / 2, 1, 0, 0);
			}

			if (Projectile.timeLeft > 270)
			{
				Texture2D tell = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrailNoEnd").Value;
				var source = new Rectangle(0, 0, tell.Width, tell.Height);
				var target = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X), (int)(Projectile.Center.Y - Main.screenPosition.Y), 500, 24);
				var origin = new Vector2(0, 12);
				Color color = new Color(255, 40, 40) * (float)Math.Sin((Projectile.timeLeft - 270) / 30f * 3.14f) * 0.5f;
				color.A = 0;

				spriteBatch.Draw(tell, target, source, color, Projectile.rotation, origin, 0, 0);
			}

			return true;
		}
	}
}

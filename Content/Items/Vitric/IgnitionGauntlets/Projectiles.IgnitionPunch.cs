using System;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric.IgnitionGauntlets
{
	public class IgnitionPunch : ModProjectile
	{
		private List<float> oldRotation = new();
		private List<Vector2> oldPosition = new();

		private bool initialized = false;

		private Vector2 posToBe = Vector2.Zero;

		private Player Owner => Main.player[Projectile.owner];

		private bool Front => Projectile.ai[0] == 0;

		private float Fade => Math.Min(1, Projectile.timeLeft / 20f);

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.timeLeft = 20;
			Projectile.extraUpdates = 2;
			Projectile.width = Projectile.height = 18;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void AI()
		{
			if (!initialized)
			{
				oldRotation = new List<float>();
				oldPosition = new List<Vector2>();
				initialized = true;
				Vector2 direction = Owner.DirectionTo(Main.MouseWorld);
				posToBe = Owner.Center + direction * 200;
			}

			if (Projectile.extraUpdates != 0)
			{
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(posToBe) * 7, 0.25f);
				Projectile.rotation = Projectile.velocity.ToRotation();
				oldRotation.Add(Projectile.rotation);
				oldPosition.Add(Projectile.Center);
			}

			if (oldRotation.Count > (Projectile.extraUpdates == 2 ? 16 : 0))
				oldRotation.RemoveAt(0);

			if (oldPosition.Count > (Projectile.extraUpdates == 2 ? 16 : 0))
				oldPosition.RemoveAt(0);

			/*if (Projectile.timeLeft == 2 && Projectile.extraUpdates != 0)
			{
				Projectile.penetrate += 2;
				Projectile.timeLeft = 20;
				Projectile.extraUpdates = 0;
				Projectile.friendly = false;
				Projectile.velocity = Vector2.Zero;
				Projectile.friendly = false;
			}*/
			else
				Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 0.4f);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			int distance = (int)(Owner.Center - Projectile.Center).Length();
			float pushback = (float)Math.Sqrt(200 * EaseFunction.EaseCubicIn.Ease((200 - distance) / 200f));
			Vector2 direction = target.DirectionTo(Owner.Center);
			Owner.velocity += direction * (pushback + 0.01f) * 0.15f;

			var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0.4f, ModContent.ProjectileType<IgnitionGauntletsImpactRing>(), 0, 0, Owner.whoAmI, Main.rand.Next(15, 25), Projectile.velocity.ToRotation());

			for (int i = 0; i < 7; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, 6, -Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(), 0, default, 1.25f).noGravity = true;
			}

			if (Owner.GetModPlayer<IgnitionPlayer>().charge < 150)
				Owner.GetModPlayer<IgnitionPlayer>().charge += 2;

			Projectile.penetrate += 2;
			Projectile.timeLeft = 20;
			Projectile.extraUpdates = 0;
			Projectile.friendly = false;
			Projectile.velocity = Vector2.Zero;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture + (Front ? "" : "_Back")).Value;
			Texture2D afterTex = ModContent.Request<Texture2D>(Texture + "_After").Value;

			/*Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);*/

			for (int k = 15; k > 0; k--)
			{

				float progress = 1 - (float)((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Color color = Color.White * EaseFunction.EaseQuarticOut.Ease(progress) * EaseFunction.EaseQuarticOut.Ease(Fade) * 0.2f;
				if (k > 0 && k < oldRotation.Count)
					Main.spriteBatch.Draw(tex, oldPosition[k] - Main.screenPosition, null, color, oldRotation[k], tex.Size() / 2, Projectile.scale * 0.8f * progress, SpriteEffects.None, 0f);
			}

			/*Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.AlphaBlend, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);*/

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * (float)EaseFunction.EaseQuarticOut.Ease(Fade), Projectile.rotation, tex.Size() / 2, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
			return false;
		}
	}
}
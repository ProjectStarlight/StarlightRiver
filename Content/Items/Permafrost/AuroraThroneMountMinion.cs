using StarlightRiver.Content.Buffs;
using System;
using System.Linq;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Permafrost
{
	internal class AuroraThroneMountMinion : ModProjectile
	{
		public override string Texture => AssetDirectory.SquidBoss + "Auroraling";

		public override void SetStaticDefaults()
		{
			Terraria.ID.ProjectileID.Sets.TrailCacheLength[Type] = 20;
			Terraria.ID.ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 26;
			Projectile.height = 30;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 600;
		}

		public override void AI()
		{
			Projectile.ai[0]++;

			NPC target = Helpers.Helper.FindNearestNPC(Projectile.Center, true);

			if (target != null && Projectile.ai[0] > 30)
				Projectile.velocity += Vector2.Normalize(Projectile.Center - target.Center) * -0.55f;

			if (Projectile.ai[0] < 30 || Projectile.velocity.Length() > 10)
				Projectile.velocity *= 0.95f;

			Projectile.velocity += Vector2.Normalize(Projectile.velocity).RotatedBy(1.57f) * (float)Math.Sin(Projectile.ai[0] * 0.1f) * 0.01f;

			if (Projectile.ai[0] % 15 == 0)
				Projectile.velocity.Y -= 0.5f;

			Projectile.rotation = Projectile.velocity.X * 0.25f;

			float sin = 1 + (float)Math.Sin(Projectile.ai[0] / 10f);
			float cos = 1 + (float)Math.Cos(Projectile.ai[0] / 10f);
			var color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

			Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.5f);
		}

		public override void Kill(int timeLeft)
		{
			foreach (NPC npc in Main.npc.Where(n => n.active && n.CanBeChasedBy(this, false) && Vector2.Distance(n.Center, Projectile.Center) < 120))
			{
				npc.StrikeNPC(npc.SimpleStrike(Projectile.damage, Projectile.Center.X > npc.Center.X ? 1 : -1, false, Projectile.knockBack));
				npc.AddBuff(BuffType<AuroraThroneMountMinionDebuff>(), 300);
			}

			float sin2 = 1 + (float)Math.Sin(Main.GameUpdateCount * 0.1f);
			float cos = 1 + (float)Math.Cos(Main.GameUpdateCount * 0.1f);
			var rainbowColor = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin2 * 0.2f);

			for (int k = 0; k < 20; k++)
			{
				Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Smoke>(), Main.rand.NextVector2Circular(5, 5), 150, rainbowColor * 0.2f, 1);

				Vector2 sparkOff = Main.rand.NextVector2Circular(3, 3);
				Dust.NewDustPerfect(Projectile.Center + sparkOff * 5, DustType<Dusts.Cinder>(), sparkOff, 0, rainbowColor, 1);
			}

			Helpers.Helper.PlayPitched("SquidBoss/LightSplash", 0.6f, 1f, Projectile.Center);
			Helpers.Helper.PlayPitched("JellyBounce", 1f, 1f, Projectile.Center);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffType<AuroraThroneMountMinionDebuff>(), 300);
		}

		public override bool PreDraw(ref Color drawColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			var frame = new Rectangle(26 * ((int)(Projectile.ai[0] / 5) % 3), 0, 26, 30);

			Texture2D tex = Request<Texture2D>(AssetDirectory.SquidBoss + "AuroralingGlow").Value;
			Texture2D tex2 = Request<Texture2D>(AssetDirectory.SquidBoss + "AuroralingGlow2").Value;
			Texture2D tex3 = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;

			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Vector2 pos = Projectile.oldPos[k];
				float opacity = 1 - k / (float)Projectile.oldPos.Length;
				float scale = 1f;

				if (opacity < 1)
				{
					opacity *= 0.5f;
					scale = 0.8f;
				}

				float sin = 1 + (float)Math.Sin(Projectile.ai[0] / 10f);
				float cos = 1 + (float)Math.Cos(Projectile.ai[0] / 10f);
				Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * 0.7f * opacity;

				spriteBatch.Draw(Request<Texture2D>(Texture).Value, pos - Main.screenPosition, frame, drawColor * 1.2f * opacity, Projectile.oldRot[k], Projectile.Size / 2, scale, 0, 0);
				spriteBatch.Draw(tex, pos - Main.screenPosition, frame, color * 0.8f, Projectile.oldRot[k], Projectile.Size / 2, scale, 0, 0);
				spriteBatch.Draw(tex2, pos - Main.screenPosition, frame, color, Projectile.oldRot[k], Projectile.Size / 2, scale, 0, 0);

				color.A = 0;

				spriteBatch.Draw(tex3, pos - Main.screenPosition, null, color, Projectile.oldRot[k], tex3.Size() / 2, opacity, 0, 0);
			}

			return false;
		}
	}

	public class AuroraThroneMountMinionDebuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Debug;

		public AuroraThroneMountMinionDebuff() : base("Inked", "You take increased damage", true, false) { }

		public override void Load()
		{
			StarlightPlayer.ModifyHitByNPCEvent += TakeExtraDamage;
			StarlightPlayer.ModifyHitByProjectileEvent += TakeExtraDamageProjectile;
		}

		private void TakeExtraDamage(Player player, NPC NPC, ref Player.HurtModifiers hit)
		{
			if (Inflicted(player))
				hit.SourceDamage *= 1.25f;
		}

		private void TakeExtraDamageProjectile(Player player, Projectile proj, ref Player.HurtModifiers hit)
		{
			if (Inflicted(player))
				hit.SourceDamage *= 1.25f;
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			float sin2 = 1 + (float)Math.Sin(Main.GameUpdateCount * 0.1f);
			float cos = 1 + (float)Math.Cos(Main.GameUpdateCount * 0.1f);
			var rainbowColor = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin2 * 0.2f);

			Dust.NewDust(npc.position, npc.width, npc.height, DustType<Dusts.Cinder>(), 0, 0, 0, rainbowColor, 0.5f);
			npc.GetGlobalNPC<Core.Systems.ExposureSystem.ExposureNPC>().ExposureMultSummon += 0.25f;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			float sin2 = 1 + (float)Math.Sin(Main.GameUpdateCount * 0.1f);
			float cos = 1 + (float)Math.Cos(Main.GameUpdateCount * 0.1f);
			var rainbowColor = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin2 * 0.2f);

			Dust.NewDust(player.position, player.width, player.height, DustType<Dusts.Cinder>(), 0, 0, 0, rainbowColor, 0.5f);
		}
	}
}
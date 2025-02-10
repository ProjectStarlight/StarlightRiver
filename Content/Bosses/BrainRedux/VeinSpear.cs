using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class VeinSpear : ModProjectile
	{
		public bool hit;

		public int ThinkerWhoAmI
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public ref float Lifetime => ref Projectile.ai[1];

		public NPC Thinker => Main.npc[ThinkerWhoAmI];

		public override string Texture => AssetDirectory.BrainRedux + Name;

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawAura;
		}

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
			Projectile.timeLeft = 99999;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Lifetime == 0)
				Lifetime = 300; // Enforce this as the default

			if (Lifetime < 60)
				Lifetime = 60; // Enforce this as the minimum

			if (Projectile.timeLeft >= 90000)
				Projectile.timeLeft = (int)Lifetime;

			if (Projectile.timeLeft == (int)Lifetime - 30)
				Projectile.velocity *= 50f;

			if (Thinker is null)
				return;

			if (Vector2.Distance(Thinker.Center, Projectile.Center) > 32)
			{
				foreach (Player player in Main.ActivePlayers)
				{
					if (Helpers.Helper.CheckLinearCollision(Thinker.Center, Projectile.Center, player.Hitbox, out Vector2 collision))
					{
						int mult = Main.masterMode ? 6 : Main.expertMode ? 4 : 1;
						player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " played the idiot harp..."), Projectile.damage * mult, 0);
						BuffInflictor.Inflict<Neurosis>(player, Main.masterMode ? 18000 : Main.expertMode ? 3000 : 1500);
					}
				}
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
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

		private void DrawAura(SpriteBatch batch)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.type == Projectile.type)
				{
					var mp = proj.ModProjectile as VeinSpear;
					SpriteBatch spriteBatch = Main.spriteBatch;

					Texture2D glow = Assets.GlowTrailNoEnd.Value;

					float opacity = 1f;

					if (proj.timeLeft <= 30)
						opacity = proj.timeLeft / 30f;

					Color glowColor = Color.White * opacity;
					glowColor.A = 0;

					var gSource = new Rectangle(0, 0, glow.Width, glow.Height);
					var gTarget = new Rectangle((int)(proj.Center.X - Main.screenPosition.X), (int)(proj.Center.Y - Main.screenPosition.Y), (int)Vector2.Distance(mp.Thinker.Center, proj.Center), 250);
					var gOrigin = new Vector2(0, glow.Height / 2f);

					spriteBatch.Draw(glow, gTarget, gSource, glowColor, (proj.Center - mp.Thinker.Center).ToRotation() - 3.14f, gOrigin, 0, 0);
				}
			}
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCs.Add(index);
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
			var gTarget = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X), (int)(Projectile.Center.Y - Main.screenPosition.Y), (int)Vector2.Distance(Thinker.Center, Projectile.Center), 100);
			var gOrigin = new Vector2(0, glow.Height / 2f);

			spriteBatch.Draw(glow, gTarget, gSource, glowColor, (Projectile.Center - Thinker.Center).ToRotation() - 3.14f, gOrigin, 0, 0);

			for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(Thinker.Center, Projectile.Center) / 16))
			{
				Vector2 pos = Vector2.Lerp(Projectile.Center, Thinker.Center, k) - Main.screenPosition;

				spriteBatch.Draw(chainTex, pos, null, new Color(220, 60, 70) * opacity, (Projectile.Center - Thinker.Center).ToRotation() + 1.58f, chainTex.Size() / 2, 1, 0, 0);
			}

			if (Projectile.timeLeft > Lifetime - 30)
			{
				Texture2D tell = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrailNoEnd").Value;
				var source = new Rectangle(0, 0, tell.Width, tell.Height);
				var target = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X), (int)(Projectile.Center.Y - Main.screenPosition.Y), 500, 24);
				var origin = new Vector2(0, 12);
				Color color = new Color(255, 40, 40) * (float)Math.Sin((Projectile.timeLeft - (Lifetime - 30)) / 30f * 3.14f) * 0.5f;
				color.A = 0;

				spriteBatch.Draw(tell, target, source, color, Projectile.rotation, origin, 0, 0);
			}

			return true;
		}
	}
}
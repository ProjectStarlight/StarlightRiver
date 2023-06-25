using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Manabonds
{
	internal class BasicManabond : Manabond
	{
		public override string Texture => AssetDirectory.ManabondItem + Name;

		public BasicManabond() : base("Manabond", "Your minions can store 40 mana\nYour minions siphon 6 mana per second from you untill full\nYour minions spend 6 mana to attack with a magic bolt occasionally") { }

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Blue;
		}

		public override void MinionAI(Projectile minion, ManabondProjectile mp)
		{
			if (mp.timer % 60 == 0 && mp.mana >= 6 && mp.target != null)
			{
				mp.mana -= 6;
				int i = Projectile.NewProjectile(minion.GetSource_FromThis(), minion.Center, Vector2.Normalize(mp.target.Center - minion.Center).RotatedByRandom(0.5f) * 15, ModContent.ProjectileType<MagicBolt>(), 12, 0.25f, minion.owner);

				var bolt = Main.projectile[i].ModProjectile as MagicBolt;
				bolt.target = mp.target;
			}
		}
	}

	internal class MagicBolt : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public NPC target;

		public ref float State => ref Projectile.ai[0];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magical bolt");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			if (State == 0)
			{
				Projectile.rotation = Projectile.velocity.ToRotation();

				Projectile.velocity += Vector2.Normalize(target.Center - Projectile.Center);

				if (Projectile.velocity.Length() > 15f)
					Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 15;

				if (Main.rand.NextBool(4))
				{
					var d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Aurora>(), Projectile.velocity.RotatedByRandom(0.1f) * 0.5f, 0, new Color(0, 100, 200) * 0.6f);
					d.customData = Main.rand.NextFloat(0.6f, 1f);
				}
			}
			else if (State == 1)
			{
				Projectile.velocity *= 0;
			}

			ManageCaches();
			ManageTrail();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			State = 1;
			Projectile.friendly = false;

			var d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Aurora>(), Main.rand.NextVector2Circular(2, 2), 0, new Color(40, 200, 255));
			d.customData = 1.2f;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 30; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 30)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 30, new TriangularTip(40 * 4), factor => factor * 12, factor =>
			{
				float alpha = 1;

				if (factor.X > 0.8f)
					alpha = 1 + (factor.X - 0.8f) * 30;

				if (factor.X >= 0.95f)
					alpha = 0;

				if (Projectile.timeLeft < 15)
					alpha *= Projectile.timeLeft / 15f;

				return new Color(40, 50 + (int)(factor.X * 100), 255) * factor.X * alpha;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

			trail?.Render(effect);
		}
	}
}

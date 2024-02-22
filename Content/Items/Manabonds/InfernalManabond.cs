using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Manabonds
{
	internal class InfernalManabond : Manabond
	{
		public override string Texture => AssetDirectory.ManabondItem + Name;

		public InfernalManabond() : base("Infernal Manabond", "Your minions can store 40 mana\nYour minions siphon 6 mana per second from you untill full\nYour minions spend 20 mana to attack with an exploding fireball occasionally") { }

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 15);
			Item.rare = ItemRarityID.Orange;
		}

		public override void MinionAI(Projectile minion, ManabondProjectile mp)
		{
			if (mp.timer % 120 == 0 && mp.mana >= 20 && mp.target != null)
			{
				mp.mana -= 20;

				if (Main.myPlayer == minion.owner)
					Projectile.NewProjectile(minion.GetSource_FromThis(), minion.Center, minion.Center.DirectionTo(mp.target.Center) * 14, ModContent.ProjectileType<Fireball>(), 35, 1f, minion.owner);
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<BasicManabond>(), 1);
			recipe.AddIngredient(ItemID.Flamelash, 1);
			recipe.AddTile(TileID.Bookcases);
			recipe.Register();
		}
	}

	internal class Fireball : ModProjectile, IDrawAdditive, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public ref float State => ref Projectile.ai[0];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Fireball");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = true;
			Projectile.penetrate = -1;
			Projectile.hostile = false;
		}

		public override void AI()
		{
			if (State == 1)
			{
				Projectile.velocity *= 0;

				if (Projectile.timeLeft > 15)
					Projectile.timeLeft = 15;
			}

			if (Main.rand.NextBool(2))
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Cinder>(), Vector2.UnitY * Main.rand.NextFloat(-2, -1), 0, new Color(255, Main.rand.Next(150, 255), 40), Main.rand.NextFloat(1f));

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (State == 1)
				return;

			Main.player[Projectile.owner].TryGetModPlayer(out StarlightPlayer starlightPlayer);
			starlightPlayer.SetHitPacketStatus(shouldRunProjMethods: true);

			target.AddBuff(BuffID.OnFire, 300);

			Explode();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (State == 1)
				return false;

			Explode();

			return false;
		}

		public void Explode()
		{
			State = 1;
			Projectile.friendly = false;

			Helper.PlayPitched("Magic/FireHit", 0.25f, Main.rand.NextFloat(-0.2f, 0.2f), Projectile.Center);

			var d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Aurora>(), Main.rand.NextVector2Circular(2, 2), 0, new Color(255, 200, 30));
			d.customData = 1.8f;

			for (int k = 0; k < 40; k++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(4, 4), 0, new Color(255, Main.rand.Next(150, 255), 40), Main.rand.NextFloat(2f));
				Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2Circular(5, 5), 0, default, Main.rand.NextFloat(1, 2));
			}

			Rectangle inflated = Projectile.Hitbox;
			inflated.Inflate(100, 100);

			foreach (NPC npc in Main.npc)
			{
				if (npc.active && npc.CanBeChasedBy(this) && !npc.friendly && npc.Hitbox.Intersects(inflated))
				{
					if (Main.myPlayer == Projectile.owner)
						npc.SimpleStrikeNPC(Projectile.damage, 0, false, Projectile.knockBack, Projectile.DamageType);

					npc.AddBuff(BuffID.OnFire, 300, quiet: true); //quiet since the SLR onhit packet will call this too on all clients and server so it doesn't need its own packets
				}
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
			Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
			spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, new Color(255, 220, 20), 0, tex2.Size() / 2, 1.4f, 0, 0);
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, 0, tex.Size() / 2, 0.8f, 0, 0);

			if (State == 1 && Projectile.timeLeft <= 15)
			{
				Texture2D tex3 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Dusts/Aurora").Value;
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 100 + (int)(Projectile.timeLeft / 15f * 155), 50) * (Projectile.timeLeft / 15f), 0, tex.Size() / 2, (15 - Projectile.timeLeft) * 0.6f, 0, 0);
				spriteBatch.Draw(tex3, Projectile.Center - Main.screenPosition, null, new Color(255, 100 + (int)(Projectile.timeLeft / 15f * 155), 50) * (Projectile.timeLeft / 15f) * 1.5f, 0, tex3.Size() / 2, (15 - Projectile.timeLeft) * 0.4f, 0, 0);
			}
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
			trail ??= new Trail(Main.instance.GraphicsDevice, 30, new TriangularTip(40 * 4), factor => factor * 21, factor =>
			{
				float alpha = 1;

				if (factor.X > 0.8f)
					alpha = 1 + (factor.X - 0.8f) * 30;

				if (factor.X >= 0.99f)
					alpha = 0;

				if (Projectile.timeLeft < 15)
					alpha *= Projectile.timeLeft / 15f;

				if (Projectile.timeLeft > 110)
					alpha *= 1 - (Projectile.timeLeft - 110) / 10f;

				return new Color(255, 50 + (int)(factor.X * 160), 30) * factor.X * alpha;
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

			effect.Parameters["opacity"].SetValue(0.25f);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			trail?.Render(effect);

			effect.Parameters["opacity"].SetValue(1f);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);
			trail?.Render(effect);
		}
	}
}
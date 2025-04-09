using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Humanizer.In;

namespace StarlightRiver.Content.Items.Misc.SoilgunFiles
{
	public abstract class SoilClumpProjectile : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		private readonly Vector2[] clumpPositions = new Vector2[5];
		private readonly float[] clumpRotations = new float[5];

		public int dustID;

		public int deathTimer;

		public bool drawTrail = true;

		public Dictionary<string, Color> Colors = new()
		{
			{ "SmokeColor", Color.White },
			{ "TrailColor", Color.White },
			{ "TrailInsideColor", Color.White },
			{ "RingOutsideColor", Color.White },
			{ "RingInsideColor", Color.White },
		};
		public virtual bool gravity => true;
		public virtual int TextureID => ItemID.DirtBlock;
		public float AmmoType => Projectile.ai[0];
		public ref float Time => ref Projectile.ai[1];
		public Player Owner => Main.player[Projectile.owner];
		public override string Texture => AssetDirectory.Invisible;
		protected SoilClumpProjectile(Color trailColor, Color trailInsideColor, Color ringOutsideColor, Color ringInsideColor, Color smokeColor, int dustID)
		{
			Colors["TrailColor"] = trailColor;
			Colors["TrailInsideColor"] = trailInsideColor;
			Colors["RingOutsideColor"] = ringOutsideColor;
			Colors["RingInsideColor"] = ringInsideColor;
			Colors["SmokeColor"] = smokeColor;

			this.dustID = dustID;
		}

		public sealed override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Soil");
		}

		public virtual void SafeSetDefaults() { }

		public sealed override void SetDefaults()
		{
			Projectile.penetrate = 2;
			SafeSetDefaults();

			Projectile.DamageType = DamageClass.Ranged;
			Projectile.Size = new Vector2(24);
			Projectile.friendly = true;
			Projectile.timeLeft = 500;

			Projectile.usesLocalNPCImmunity = true; // the projectile acts as a projectile which can only hit once so local immunity makes it act with vanilla behavior, ex: 5 projectiles can hit at once
			Projectile.localNPCHitCooldown = 10;

			for (int i = 0; i < clumpPositions.Length; i++)
			{
				clumpPositions[i] = Main.rand.NextVector2CircularEdge(7f, 7f);
				clumpRotations[i] = Main.rand.NextFloat(MathHelper.TwoPi);
			}
		}

		public override bool? CanDamage()
		{
			return Projectile.penetrate >= 2;
		}

		public virtual void SafeAI() { }

		public sealed override void AI()
		{
			if (deathTimer > 0)
			{
				Projectile.velocity *= 0f;

				deathTimer--;
				if (deathTimer == 1)
					Projectile.active = false;
			}

			SafeAI();

			Time++;

			Projectile.rotation += Projectile.velocity.Length() * 0.01f;

			if (Projectile.timeLeft < 490 && gravity)
			{
				if (Projectile.velocity.Y < 18f)
				{
					Projectile.velocity.Y += 0.35f;

					if (Projectile.velocity.Y > 0)
					{
						if (Projectile.velocity.Y < 12f)
							Projectile.velocity.Y *= 1.060f;
						else
							Projectile.velocity.Y *= 1.03f;
					}
				}
			}

			for (int i = 0; i < 2; i++)
			{
				Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(15f, 15f), ModContent.DustType<SoilgunSmoke>(),
					Main.rand.NextVector2Circular(1f, 1f), 80, Colors["SmokeColor"], Main.rand.NextFloat(0.04f, 0.08f));

				if (Main.rand.NextBool(6))
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), dustID, Vector2.Zero, 100).noGravity = true;
			}

			if (Main.rand.NextBool(5))
			{
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(24f, 24f), ModContent.DustType<PixelatedGlow>(),
					Projectile.velocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.1f, 0.2f), 0, Colors["TrailInsideColor"] with { A = 0 }, 0.2f);
			}

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public virtual void SafeOnKill()
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), ModContent.DustType<PixelatedGlow>(),
					-Projectile.velocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.4f), 0, Colors["TrailInsideColor"] with { A = 0 }, 0.45f);

				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedImpactLineDust>(),
					-Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.8f), 0, Colors["TrailInsideColor"] with { A = 0 }, 0.1f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), ModContent.DustType<PixelatedGlow>(),
					Main.rand.NextVector2Circular(6f, 6f), 0, Colors["TrailInsideColor"] with { A = 0 }, 0.45f);

				Dust.NewDustPerfect(Projectile.Center, dustID, -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.5f),
					Main.rand.Next(100), default, Main.rand.NextFloat(1f, 1.5f)).noGravity = true;
			}

			for (int i = 0; i < 12; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, dustID, -Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.4f),
					Main.rand.Next(100), default, Main.rand.NextFloat(1f, 1.5f)).noGravity = true;

				Dust.NewDustPerfect(Projectile.Center, dustID, Main.rand.NextVector2Circular(5f, 5f),
					Main.rand.Next(50), default, Main.rand.NextFloat(1f, 1.5f)).noGravity = true;

				Dust dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5f, 5f),
					ModContent.DustType<PixelSmokeColor>(), -Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f),
					Main.rand.Next(100, 140), Colors["SmokeColor"], Main.rand.NextFloat(0.03f, 0.05f));

				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = Colors["SmokeColor"];

				dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5f, 5f),
					ModContent.DustType<PixelSmokeColor>(), Main.rand.NextVector2CircularEdge(5f, 5f) * Main.rand.NextFloat(0.25f),
					Main.rand.Next(100, 120), Colors["SmokeColor"], Main.rand.NextFloat(0.05f, 0.08f));

				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = Colors["SmokeColor"];
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (deathTimer <= 0)
			{
				deathTimer = 10;

				SafeOnKill();
			}

			return false;
		}

		public virtual void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{

		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (deathTimer <= 0 && Projectile.penetrate == 2)
			{
				SafeOnKill();

				deathTimer = 10;
			}

			SafeOnHitNPC(target, hit, damageDone);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();
			Texture2D tex = Terraria.GameContent.TextureAssets.Item[TextureID].Value;
			Texture2D bloomTex = Assets.Masks.GlowAlpha.Value;

			// just gonna hardcode this since its the only exception
			if (Projectile.ModProjectile is SoilgunVitricSandSoil)
				tex = Assets.Tiles.Vitric.VitricSandItem.Value;

			float opacity = FadeOut();

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Colors["RingInsideColor"] with { A = 0 } * opacity, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.35f, 0f, 0f);

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Colors["RingOutsideColor"] with { A = 0 } * opacity, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.25f, 0f, 0f);

			for (int i = 0; i < clumpPositions.Length; i++)
			{
				Main.spriteBatch.Draw(tex, Projectile.Center + clumpPositions[i].RotatedBy(Projectile.rotation) - Main.screenPosition, null, lightColor * opacity, Projectile.rotation + clumpRotations[i], tex.Size() / 2f, Projectile.scale, 0f, 0f);

				Main.spriteBatch.Draw(bloomTex, Projectile.Center + clumpPositions[i].RotatedBy(Projectile.rotation) - Main.screenPosition, null, Colors["TrailInsideColor"] with { A = 0 } * opacity, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.25f, 0f, 0f);
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor * opacity, Projectile.rotation, tex.Size() / 2f, Projectile.scale, 0f, 0f);

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
			{
				Effect effect = ShaderLoader.GetShader("DistortSprite").Value;

				if (effect != null)
				{
					Main.spriteBatch.End();
					Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null);

					effect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.035f);
					effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.0035f);
					effect.Parameters["screenPos"].SetValue(Main.screenPosition * new Vector2(0.5f, 0.1f) / new Vector2(Main.screenWidth, Main.screenHeight));

					effect.Parameters["offset"].SetValue(new Vector2(0.001f));
					effect.Parameters["repeats"].SetValue(1);
					effect.Parameters["uImage1"].SetValue(Assets.Noise.SwirlyNoiseLooping.Value);
					effect.Parameters["uImage2"].SetValue(Assets.Noise.SwirlyNoiseLooping.Value);

					Color color = Colors["TrailColor"] with { A = 0 } * opacity;

					effect.Parameters["uColor"].SetValue(color.ToVector4());
					effect.Parameters["noiseImage1"].SetValue(Assets.Items.Misc.Soilgun_Noise.Value);

					effect.CurrentTechnique.Passes[0].Apply();

					Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 1f, 0f, 0f);

					color = Colors["TrailInsideColor"] with { A = 0 } * opacity;

					effect.Parameters["uColor"].SetValue(color.ToVector4());

					effect.CurrentTechnique.Passes[0].Apply();

					Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.5f, 0f, 0f);

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default);

					Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, color * opacity, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.5f, 0f, 0f);
				}
			});

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 18; i++)
				{
					cache.Add(Projectile.Center + Projectile.velocity);
				}
			}

			cache.Add(Projectile.Center + Projectile.velocity);

			while (cache.Count > 18)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 18, new TriangularTip(4), factor => 12, factor => Colors["TrailColor"] * factor.X * FadeOut());

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 18, new TriangularTip(4), factor => 6, factor => Colors["TrailInsideColor"] * factor.X * 0.5f * FadeOut());

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			if (!drawTrail)
				return;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
			{
				Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

				if (effect != null)
				{
					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Matrix.Identity;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.05f);
					effect.Parameters["repeats"].SetValue(1);
					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);

					trail?.Render(effect);
					trail2?.Render(effect);

					effect.Parameters["repeats"].SetValue(2);
					effect.Parameters["sampleTexture"].SetValue(Assets.FireTrail.Value);

					trail?.Render(effect);
					trail?.Render(effect);
				}
			});
		}

		private float FadeOut()
		{
			if (deathTimer > 0)
				return deathTimer / 10f;

			return 1f;
		}
	}

	public class SoilgunDirtClump : SoilClumpProjectile
	{
		public override int TextureID => ItemID.DirtBlock;
		public SoilgunDirtClump() : base(new Color(30, 19, 12), new Color(30, 19, 12), new Color(81, 47, 27), new Color(105, 67, 44), new Color(82, 45, 22), DustID.Dirt) { }
	}

	public class SoilgunSandClump : SoilClumpProjectile
	{
		public override int TextureID => ItemID.SandBlock;
		public SoilgunSandClump() : base(new Color(80, 50, 20), new Color(160, 131, 59), new Color(139, 131, 59), new Color(212, 192, 100), new Color(150, 120, 59), DustID.Sand) { }

		public override void SafeOnKill()
		{
			base.SafeOnKill();

			if (Projectile.owner == Main.myPlayer)
			{
				Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero,
						ModContent.ProjectileType<SoilgunSandExplosion>(), Projectile.damage, 0f, Owner.whoAmI);
			}
		}
	}

	public class SoilgunCrimsandClump : SoilClumpProjectile
	{
		public override int TextureID => ItemID.CrimsandBlock;
		public SoilgunCrimsandClump() : base(new Color(56, 17, 14), new Color(135, 43, 34), new Color(56, 17, 14), new Color(135, 43, 34), new Color(40, 10, 10) * 0.6f, DustID.CrimsonPlants) { }

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Owner.AddBuff(BuffID.Regeneration, 600);
		}
	}

	public class SoilgunEbonsandClump : SoilClumpProjectile
	{
		public override int TextureID => ItemID.EbonsandBlock;
		public SoilgunEbonsandClump() : base(new Color(26, 18, 31), new Color(62, 45, 75), new Color(62, 45, 75), new Color(119, 106, 138), new Color(30, 25, 45) * 0.6f, DustID.Ebonwood) { }

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(ModContent.BuffType<EbonsandDebuff>(), 600);
		}
	}

	public class SoilgunPearlsandClump : SoilClumpProjectile
	{
		private NPC target;
		public override bool gravity => false;
		public override int TextureID => ItemID.PearlsandBlock;
		public SoilgunPearlsandClump() : base(new Color(87, 77, 106), new Color(174, 168, 186), new Color(87, 77, 106), new Color(246, 235, 228), new Color(120, 110, 140), DustID.Pearlsand) { }
		public override void SafeAI()
		{
			target = Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Projectile.Center) < 1250f && Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1)).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();

			if (target != null)
			{
				if (!target.active || Projectile.Distance(target.Center) > 1250f)
				{
					target = null;
					return;
				}

				Projectile.velocity = (Projectile.velocity * 35f + Utils.SafeNormalize(target.Center - Projectile.Center, Vector2.UnitX) * 35f) / 36f;
			}
			else
			{
				Projectile.velocity *= 0.96f;

				if (Projectile.velocity.Length() < 5f)
					Projectile.timeLeft--;
			}
		}
	}

	public class SoilgunVitricSandClump : SoilClumpProjectile
	{
		public SoilgunVitricSandClump() : base(new Color(87, 129, 140), new Color(99, 183, 173), new Color(87, 129, 140), new Color(171, 230, 167), new Color(86, 57, 47), ModContent.DustType<VitricSandDust>()) { }

		public override void SafeSetDefaults()
		{
			Projectile.penetrate = Main.rand.Next(3, 6);

			Projectile.localNPCHitCooldown = 15;
			Projectile.usesLocalNPCImmunity = true;
		}
	}

	public class SoilgunSlushClump : SoilClumpProjectile
	{
		public override int TextureID => ItemID.SlushBlock;
		public SoilgunSlushClump() : base(new Color(27, 40, 51), new Color(62, 95, 104), new Color(27, 40, 51), new Color(164, 182, 180), new Color(77, 106, 113), DustID.Slush) { }

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Frostburn, 600);
		}
	}

	public class SoilgunSiltClump : SoilClumpProjectile
	{
		public override int TextureID => ItemID.SiltBlock;
		public SoilgunSiltClump() : base(new Color(22, 24, 32), new Color(49, 51, 61), new Color(49, 51, 61), new Color(106, 107, 118), new Color(89, 83, 83), DustID.Silt) { }

		public override void OnSpawn(IEntitySource source)
		{
			// vanilla extracinator code is like a billion lines so i modified it slightly
			// ore drops and stuff arent able to be got from this, just gems and coins, but like who is actually going to use this seriously
			if (Main.rand.NextBool(2))
			{
				int quantity = 2 + Main.rand.Next(50);
				int type;

				if (Main.rand.NextFloat() < 0.75f) // give the player money 75% of the time
				{
					float roll = Main.rand.NextFloat();

					if (roll < 0.0005f)
					{
						quantity = Main.rand.Next(3);
						type = ItemID.PlatinumCoin;
					}
					else if (roll < 0.01f)
					{
						quantity = Main.rand.Next(10);
						type = ItemID.GoldCoin;
					}
					else if (roll < 0.1f)
					{
						type = ItemID.SilverCoin;
					}
					else
					{
						type = ItemID.CopperCoin;
					}
				}
				else // otherwise give them a random gem
				{
					type = Main.rand.Next(new int[] { ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire, ItemID.Emerald, ItemID.Ruby, ItemID.Diamond, ItemID.Amber });
					quantity = 1 + Main.rand.Next(3);
				}

				Item.NewItem(Projectile.GetSource_Loot(), Projectile.getRect(), type, quantity);
			}
		}
	}

	public class SoilgunMudClump : SoilClumpProjectile
	{
		private bool spawnBee;
		public override int TextureID => ItemID.MudBlock;
		public SoilgunMudClump() : base(new Color(30, 21, 24), new Color(23, 68, 9), new Color(73, 57, 63), new Color(111, 83, 89), new Color(73, 57, 63), DustID.Mud) { }

		public override void SafeSetDefaults()
		{
			spawnBee = Main.rand.NextBool(2);
		}

		public override void SafeAI()
		{
			if (spawnBee)
			{
				if (Main.rand.NextBool(5))
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.Honey2, Vector2.Zero, 50, Scale: 1.5f).noGravity = true;
			}
		}

		public override void SafeOnKill()
		{
			base.SafeOnKill();

			if (spawnBee)
			{
				for (int i = 0; i < 3; i++)
				{
					Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2CircularEdge(6f, 6f),
					Owner.beeType(), Owner.beeDamage(Projectile.damage / 2), Owner.beeKB(Projectile.knockBack), Owner.whoAmI);
				}

				for (int i = 0; i < 12; i++)
				{
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.Honey2, Main.rand.NextVector2Circular(5f, 5f), Main.rand.Next(150)).noGravity = true;

					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.Honey2, Main.rand.NextVector2Circular(5f, 5f), 150);
				}
			}
		}
	}

	public class SoilgunAshClump : SoilClumpProjectile
	{
		public override int TextureID => ItemID.AshBlock;
		public SoilgunAshClump() : base(new Color(246, 86, 22), new Color(252, 147, 20), new Color(73, 57, 63), new Color(111, 83, 89), new Color(32, 27, 34) * 0.75f, DustID.Ash) { }

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire, 300);
		}
	}
}
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.ExposureSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Utilities;

namespace StarlightRiver.Content.Items.Misc.SoilgunFiles
{
	public abstract class SoilProjectile : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		public int dustID;

		public int deathTimer;

		public float maxTimeleft;

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
		public virtual int ClumpType => -1;
		public float AmmoType => Projectile.ai[0];
		public ref float Time => ref Projectile.ai[1];
		public Player Owner => Main.player[Projectile.owner];
		public override string Texture => AssetDirectory.Invisible;
		protected SoilProjectile(Color trailColor, Color trailInsideColor, Color ringOutsideColor, Color ringInsideColor, Color smokeColor, int dustID)
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
			Projectile.Size = new Vector2(12);
			Projectile.friendly = true;
			Projectile.timeLeft = 30;

			Projectile.usesLocalNPCImmunity = true; // the projectile acts as a projectile which can only hit once so local immunity makes it act with vanilla behavior, ex: 5 projectiles can hit at once
			Projectile.localNPCHitCooldown = 10;

			Projectile.ArmorPenetration = 5;
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

			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Projectile.timeLeft < maxTimeleft - 10 && gravity)
			{
				if (Projectile.velocity.Y < 18f)
				{
					Projectile.velocity.Y += 0.45f;

					if (Projectile.velocity.Y > 0)
					{
						if (Projectile.velocity.Y < 12f)
							Projectile.velocity.Y *= 1.025f;
						else
							Projectile.velocity.Y *= 1.0125f;
					}
				}
			}

			Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<SoilgunSmoke>(),
				 Main.rand.NextVector2Circular(1f, 1f), 130, Colors["SmokeColor"], Main.rand.NextFloat(0.05f, 0.08f));

			if (Main.rand.NextBool(6))
				Dust.NewDustPerfect(Projectile.Center, dustID, Vector2.Zero, 100).noGravity = true;

			if (Main.rand.NextBool(3))
			{
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), dustID, Projectile.velocity.RotatedByRandom(MathHelper.Lerp(0.1f, 1f, 1f - Projectile.timeLeft / maxTimeleft)), 150, default, 1f).noGravity = true;

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), dustID, Projectile.velocity.RotatedByRandom(MathHelper.Lerp(0.1f, 1f, 1f - Projectile.timeLeft / maxTimeleft)) * 0.6f, 150, default, 0.65f);

				Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<SoilgunSmoke>(),
					Projectile.velocity.RotatedByRandom(MathHelper.Lerp(0.1f, 1f, 1f - Projectile.timeLeft / maxTimeleft)) * Main.rand.NextFloat(0.35f, 1f), Main.rand.Next(80, 120), Colors["SmokeColor"], Main.rand.NextFloat(0.03f, 0.05f));
			}

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public virtual void SafeOnKill()
		{

		}

		public sealed override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (deathTimer <= 0)
			{
				deathTimer = 10;

				SafeOnKill();

				SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

				for (int i = 0; i < 6; i++)
				{
					Dust.NewDustPerfect(Projectile.Center, dustID, -Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.4f),
						Main.rand.Next(100), default, Main.rand.NextFloat(1f, 1.5f)).noGravity = true;

					Dust dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelSmokeColor>(), -Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f),
						Main.rand.Next(100, 140), Colors["SmokeColor"], Main.rand.NextFloat(0.02f, 0.03f));

					dust.rotation = Main.rand.NextFloat(6.28f);
					dust.customData = Colors["SmokeColor"];

					dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelSmokeColor>(), -Projectile.velocity.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.25f),
						Main.rand.Next(100, 120), Colors["SmokeColor"], Main.rand.NextFloat(0.03f, 0.05f));

					dust.rotation = Main.rand.NextFloat(6.28f);
					dust.customData = Colors["SmokeColor"];
				}
			}

			return false;
		}

		public virtual void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{

		}

		public sealed override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (deathTimer <= 0 && Projectile.penetrate == 2)
			{
				SafeOnKill();

				deathTimer = 10;
			}

			SafeOnHitNPC(target, hit, damageDone);

			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, dustID, -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.5f),
					Main.rand.Next(100), default, Main.rand.NextFloat(1f, 1.5f)).noGravity = true;

				Dust dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5f, 5f),
					ModContent.DustType<PixelSmokeColor>(), -Projectile.velocity.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.3f),
					Main.rand.Next(120, 140), Colors["TrailColor"], Main.rand.NextFloat(0.03f, 0.06f));

				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = Colors["SmokeColor"];
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Masks + "GlowAlpha").Value;

			float lerper = FadeOut();

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Colors["RingInsideColor"] with { A = 0 } * lerper, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.35f, 0f, 0f);

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Colors["RingOutsideColor"] with { A = 0 } * lerper, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.25f, 0f, 0f);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor * lerper, Projectile.rotation, tex.Size() / 2f, Projectile.scale, 0f, 0f);

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
					effect.Parameters["uImage1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/SwirlyNoiseLooping").Value);
					effect.Parameters["uImage2"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/SwirlyNoiseLooping").Value);

					Color color = Colors["TrailColor"] with { A = 0 } * lerper;

					effect.Parameters["uColor"].SetValue(color.ToVector4());
					effect.Parameters["noiseImage1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "Soilgun_Noise").Value);

					effect.CurrentTechnique.Passes[0].Apply();

					Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.65f, 0f, 0f);

					color = Colors["TrailInsideColor"] with { A = 0 } * lerper;

					effect.Parameters["uColor"].SetValue(color.ToVector4());

					effect.CurrentTechnique.Passes[0].Apply();

					Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.45f, 0f, 0f);

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default);
				}
			});

			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 13; i++)
				{
					cache.Add(Projectile.Center + Projectile.velocity);
				}
			}

			cache.Add(Projectile.Center + Projectile.velocity);

			while (cache.Count > 13)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
				trail = new Trail(Main.instance.GraphicsDevice, 13, new NoTip(), factor => 8, factor => Colors["TrailColor"] * factor.X * FadeOut());

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(4), factor => 4, factor => Colors["TrailInsideColor"] * factor.X * 0.5f * FadeOut());

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
					effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "FireTrail").Value);

					trail?.Render(effect);
					trail?.Render(effect);
				}
			});
		}

		private float FadeOut()
		{
			float opacity = 1f;

			if (Projectile.timeLeft < 10)
				opacity *= Projectile.timeLeft / 10f;

			if (deathTimer > 0)
				opacity *= deathTimer / 10f;

			return opacity;
		}
	}

	public class SoilgunDirtSoil : SoilProjectile
	{
		public override int ClumpType => ModContent.ProjectileType<SoilgunDirtClump>();
		public override string Texture => "Terraria/Images/Item_" + ItemID.DirtBlock;
		public SoilgunDirtSoil() : base(new Color(30, 19, 12), new Color(30, 19, 12), new Color(81, 47, 27), new Color(105, 67, 44), new Color(82, 45, 22), DustID.Dirt) { }
	}

	public class SoilgunSandSoil : SoilProjectile
	{
		public override int ClumpType => ModContent.ProjectileType<SoilgunSandClump>();
		public override string Texture => "Terraria/Images/Item_" + ItemID.SandBlock;
		public SoilgunSandSoil() : base(new Color(80, 50, 20), new Color(160, 131, 59), new Color(139, 131, 59), new Color(212, 192, 100), new Color(150, 120, 59), DustID.Sand) { }

		public override void SafeOnKill()
		{
			if (Projectile.owner == Main.myPlayer)
			{
				Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero,
						ModContent.ProjectileType<SoilgunSandExplosion>(), Projectile.damage / 2, 0f, Owner.whoAmI);
			}
		}
	}

	class SoilgunSandExplosion : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sand");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;

			Projectile.DamageType = DamageClass.Ranged;
			Projectile.Size = new Vector2(50);
			Projectile.friendly = true;
			Projectile.timeLeft = 10;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;

			Projectile.ArmorPenetration = 5;
		}
	}

	public class SoilgunCrimsandSoil : SoilProjectile
	{
		public override int ClumpType => ModContent.ProjectileType<SoilgunCrimsandClump>();
		public override string Texture => "Terraria/Images/Item_" + ItemID.CrimsandBlock;
		public SoilgunCrimsandSoil() : base(new Color(56, 17, 14), new Color(135, 43, 34), new Color(56, 17, 14), new Color(135, 43, 34), new Color(40, 10, 10) * 0.6f, DustID.CrimsonPlants) { }

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Owner.AddBuff(BuffID.Regeneration, 300);
		}
	}

	public class SoilgunEbonsandSoil : SoilProjectile
	{
		public override int ClumpType => ModContent.ProjectileType<SoilgunEbonsandClump>();
		public override string Texture => "Terraria/Images/Item_" + ItemID.EbonsandBlock;
		public SoilgunEbonsandSoil() : base(new Color(26, 18, 31), new Color(62, 45, 75), new Color(62, 45, 75), new Color(119, 106, 138), new Color(30, 25, 45) * 0.6f, DustID.Ebonwood) { }

		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(ModContent.BuffType<EbonsandDebuff>(), 300);
		}
	}

	class EbonsandDebuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Debug;

		public EbonsandDebuff() : base("Weakened", "Damage taken increased by 15%", false) { }

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.GetGlobalNPC<ExposureNPC>().ExposureMultAll += 0.15f;

			if (Main.rand.NextBool(10))
			{
				Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width / 2, npc.height / 2), ModContent.DustType<PixelatedImpactLineDustGlow>(),
					Vector2.UnitY * -Main.rand.NextFloat(3f, 6f), 50, new Color(255, 0, 255, 0), 0.15f);
			}
		}
	}

	public class SoilgunPearlsandSoil : SoilProjectile
	{
		private NPC target;
		public override bool gravity => false;
		public override int ClumpType => ModContent.ProjectileType<SoilgunPearlsandClump>();
		public override string Texture => "Terraria/Images/Item_" + ItemID.PearlsandBlock;
		public SoilgunPearlsandSoil() : base(new Color(87, 77, 106), new Color(174, 168, 186), new Color(87, 77, 106), new Color(246, 235, 228), new Color(120, 110, 140), DustID.Pearlsand) { }
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

				Projectile.velocity = (Projectile.velocity * 35f + Utils.SafeNormalize(target.Center - Projectile.Center, Vector2.UnitX) * 25f) / 36f;
			}
			else
			{
				Projectile.velocity *= 0.96f;

				if (Projectile.velocity.Length() < 1f)
					Projectile.timeLeft--;
			}
		}
	}

	public class SoilgunVitricSandSoil : SoilProjectile
	{
		public override int ClumpType => ModContent.ProjectileType<SoilgunVitricSandClump>();
		public override string Texture => AssetDirectory.VitricTile + "VitricSandItem";
		public SoilgunVitricSandSoil() : base(new Color(87, 129, 140), new Color(99, 183, 173), new Color(87, 129, 140), new Color(171, 230, 167), new Color(86, 57, 47), ModContent.DustType<VitricSandDust>()) { }
		public override void SafeSetDefaults()
		{
			Projectile.penetrate = Main.rand.Next(2, 5);

			Projectile.localNPCHitCooldown = 20;
			Projectile.usesLocalNPCImmunity = true;
		}
	}

	public class SoilgunSlushSoil : SoilProjectile
	{
		public override int ClumpType => ModContent.ProjectileType<SoilgunSlushClump>();
		public override string Texture => "Terraria/Images/Item_" + ItemID.SlushBlock;
		public SoilgunSlushSoil() : base(new Color(27, 40, 51), new Color(62, 95, 104), new Color(27, 40, 51), new Color(164, 182, 180), new Color(77, 106, 113), DustID.Slush) { }
		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Frostburn, 300);
		}
	}

	public class SoilgunSiltSoil : SoilProjectile
	{
		public override int ClumpType => ModContent.ProjectileType<SoilgunSiltClump>();
		public override string Texture => "Terraria/Images/Item_" + ItemID.SiltBlock;
		public SoilgunSiltSoil() : base(new Color(22, 24, 32), new Color(49, 51, 61), new Color(49, 51, 61), new Color(106, 107, 118), new Color(89, 83, 83), DustID.Silt) { }
		public override void OnSpawn(IEntitySource source)
		{
			// vanilla extracinator code is like a billion lines so i modified it slightly
			// ore drops and stuff arent able to be got from this, just gems and coins, but like who is actually going to use this seriously idk
			if (Main.rand.NextBool(5))
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

	public class SoilgunMudSoil : SoilProjectile
	{
		public override int ClumpType => ModContent.ProjectileType<SoilgunMudClump>();
		public override string Texture => "Terraria/Images/Item_" + ItemID.MudBlock;
		public SoilgunMudSoil() : base(new Color(30, 21, 24), new Color(23, 68, 9), new Color(73, 57, 63), new Color(111, 83, 89), new Color(73, 57, 63), DustID.Mud) { }
		public override void SafeOnKill()
		{
			if (Main.rand.NextBool(5))
			{
				if (Projectile.owner == Main.myPlayer)
				{
					Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2CircularEdge(6f, 6f),
							Owner.beeType(), Owner.beeDamage(Projectile.damage), Owner.beeKB(Projectile.knockBack), Owner.whoAmI);
				}
			}
		}
	}

	public class SoilgunAshSoil : SoilProjectile
	{
		public override int ClumpType => ModContent.ProjectileType<SoilgunAshClump>();
		public override string Texture => "Terraria/Images/Item_" + ItemID.AshBlock;
		public SoilgunAshSoil() : base(new Color(246, 86, 22), new Color(252, 147, 20), new Color(73, 57, 63), new Color(111, 83, 89), new Color(32, 27, 34) * 0.75f, DustID.Ash) { }
		public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire, 300);
		}
	}
}